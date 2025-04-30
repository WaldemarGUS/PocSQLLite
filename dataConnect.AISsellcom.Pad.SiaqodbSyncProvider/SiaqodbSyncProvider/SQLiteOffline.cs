using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Synchronization.Services;
using Microsoft.Synchronization.ClientServices;
using SiaqodbSyncProvider.Utilities;

namespace SiaqodbSyncProvider
{
    public class SQLiteOffline
    {
        private readonly SQLiteAsyncConnection _connection;
        private readonly string _dbPath;
        private readonly object _locker = new object();
        private SQLiteOfflineSyncProvider _provider;

        public event EventHandler<SyncProgressEventArgs> SyncProgress;
        public event EventHandler<SyncCompletedEventArgs> SyncCompleted;

        public SQLiteOffline(string dbPath, Uri uri)
        {
            _dbPath = dbPath;
            _connection = new SQLiteAsyncConnection(dbPath);
            _provider = new SQLiteOfflineSyncProvider(this, uri);
            InitializeDatabase();
        }

        public SQLiteOffline(string dbPath, Uri uri, string scopeName)
        {
            _dbPath = dbPath;
            _connection = new SQLiteAsyncConnection(dbPath);
            _provider = new SQLiteOfflineSyncProvider(this, uri, scopeName);
            InitializeDatabase();
        }

        public SQLiteOffline(string dbPath, SQLiteOfflineSyncProvider provider)
        {
            _dbPath = dbPath;
            _connection = new SQLiteAsyncConnection(dbPath);
            _provider = provider;
            InitializeDatabase();
        }

        public SQLiteOfflineSyncProvider SyncProvider
        {
            get => _provider;
            set => _provider = value;
        }

        private async void InitializeDatabase()
        {
            await _connection.CreateTableAsync<DirtyEntity>();
            await _connection.CreateTableAsync<Hp>();
        }

        private async Task CreateDirtyEntity(object obj, DirtyOperation dop)
        {
            var dirtyEntity = new DirtyEntity
            {
                EntityOID = GetOID(obj),
                DirtyOp = dop,
                EntityType = obj.GetType().Name
            };
            await _connection.InsertOrReplaceAsync(dirtyEntity);
        }

        private async Task CreateTombstoneDirtyEntity(SQLiteOfflineEntity obj, int oid)
        {
            // Get all dirty entities for this object
            var allDirtyEntities = await _connection.Table<DirtyEntity>().ToListAsync();
            var thisEntityOperations = allDirtyEntities
                .Where(x => x.EntityType == obj.GetType().Name && x.EntityOID == oid)
                .ToList();

            bool noDeleteTracking = thisEntityOperations.Any(o => o.DirtyOp == DirtyOperation.Inserted);

            foreach (var item in thisEntityOperations)
            {
                await _connection.DeleteAsync(item);
            }

            if (noDeleteTracking)
            {
                return;
            }

            var dirtyEntity = new DirtyEntity
            {
                EntityOID = oid,
                EntityType = obj.GetType().Name,
                DirtyOp = DirtyOperation.Deleted,
                TombstoneObj = System.Text.Encoding.UTF8.GetString(JSerializer.Serialize(obj))
            };

            obj.IsDirty = true;
            obj.IsTombstone = true;
            await _connection.InsertOrReplaceAsync(dirtyEntity);
        }

        public async Task StoreObject(object obj)
        {
            if (!(obj is SQLiteOfflineEntity entity))
                throw new Exception("Entity should be SQLiteOfflineEntity type");

            entity.IsDirty = true;
            var dop = entity.OID == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
            
            await _connection.InsertOrReplaceAsync(obj);
            await CreateDirtyEntity(obj, dop);
        }

        public async Task Delete(object obj)
        {
            if (!(obj is SQLiteOfflineEntity entity))
                throw new Exception("Entity should be SQLiteOfflineEntity type");

            await CreateTombstoneDirtyEntity(entity, entity.OID);
            await _connection.DeleteAsync(obj);
        }

        public async Task<T> LoadObjectByOID<T>(System.Guid id) where T : new()
        {
            return await _connection.Table<T>().Where(x => ((Hp)(object)x).TId == id).FirstOrDefaultAsync();
        }

        public async Task<List<T>> LoadAll<T>() where T : new()
        {
            return await _connection.Table<T>().ToListAsync();
        }

        public async Task<CacheRefreshStatistics> Synchronize()
        {
            if (_provider == null)
                throw new Exception("Provider cannot be null");

            _provider.SyncProgress -= Provider_SyncProgress;
            _provider.SyncProgress += Provider_SyncProgress;

            var stat = await _provider.CacheController.SynchronizeAsync();
            var args = new SyncCompletedEventArgs(stat.Cancelled, stat.Error, stat);
            OnSyncCompleted(args);
            return stat;
        }

        private void Provider_SyncProgress(object sender, SyncProgressEventArgs e)
        {
            OnSyncProgress(e);
        }

        public void AddTypeForSync<T>() where T : IOfflineEntity
        {
            if (_provider == null)
                throw new Exception("Provider cannot be null");
            _provider.CacheController.ControllerBehavior.AddType<T>();
        }

        public void AddScopeParameters(string key, string value)
        {
            if (_provider == null)
                throw new Exception("Provider cannot be null");
            _provider.CacheController.ControllerBehavior.AddScopeParameters(key, value);
        }

        protected void OnSyncProgress(SyncProgressEventArgs args)
        {
            SyncProgress?.Invoke(this, args);
        }

        protected void OnSyncCompleted(SyncCompletedEventArgs args)
        {
            SyncCompleted?.Invoke(this, args);
        }

        private int GetOID(object obj)
        {
            if (obj is ISqoDataObject sqoObj)
                return sqoObj.OID;
            return 0;
        }

        public void SaveAnchor(byte[] anchor, string scopeName)
        {
            if (anchor == null) return;
            var anchorKey = $"anchor_{scopeName}";
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(_dbPath), anchorKey), anchor);
        }

        public bool DropAnchor(string scopeName)
        {
            var anchorKey = $"anchor_{scopeName}";
            var anchorPath = Path.Combine(Path.GetDirectoryName(_dbPath), anchorKey);
            if (File.Exists(anchorPath))
            {
                File.Delete(anchorPath);
                return true;
            }
            return false;
        }

        public byte[] GetServerBlob(string scopeName)
        {
            var anchorKey = $"anchor_{scopeName}";
            var anchorPath = Path.Combine(Path.GetDirectoryName(_dbPath), anchorKey);
            if (File.Exists(anchorPath))
            {
                return File.ReadAllBytes(anchorPath);
            }
            return null;
        }
    }

    public interface ISqoDataObject
    {
        int OID { get; set; }
    }
} 