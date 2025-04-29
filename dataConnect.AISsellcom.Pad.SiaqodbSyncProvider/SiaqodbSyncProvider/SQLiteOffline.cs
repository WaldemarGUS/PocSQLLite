using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Synchronization.Services;

namespace SiaqodbSyncProvider
{
    public class SQLiteOffline
    {
        private readonly SQLiteAsyncConnection _connection;
        private readonly string _dbPath;

        public SQLiteOffline(string dbPath)
        {
            _dbPath = dbPath;
            _connection = new SQLiteAsyncConnection(dbPath);
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            // Create tables for sync metadata
            await _connection.CreateTableAsync<SyncMetadata>();
            await _connection.CreateTableAsync<DirtyEntity>();
        }

        public async Task<T> LoadObjectByOID<T>(int oid) where T : new()
        {
            return await _connection.Table<T>().Where(x => ((ISqoDataObject)x).OID == oid).FirstOrDefaultAsync();
        }

        public async Task<List<T>> LoadAll<T>() where T : new()
        {
            return await _connection.Table<T>().ToListAsync();
        }

        public async Task StoreObject<T>(T obj)
        {
            await _connection.InsertOrReplaceAsync(obj);
        }

        public async Task DeleteObject<T>(T obj)
        {
            await _connection.DeleteAsync(obj);
        }

        public async Task BeginTransaction()
        {
            await _connection.RunInTransactionAsync(conn => { });
        }

        public async Task CommitTransaction()
        {
            // SQLite handles commit automatically in RunInTransactionAsync
        }

        public async Task RollbackTransaction()
        {
            // SQLite handles rollback automatically in RunInTransactionAsync
        }

        public void Flush()
        {
            // SQLite handles this automatically
        }

        public async Task StoreDirtyEntity(DirtyEntity entity)
        {
            await _connection.InsertOrReplaceAsync(entity);
        }

        public async Task<List<DirtyEntity>> LoadAllDirtyEntities()
        {
            return await _connection.Table<DirtyEntity>().ToListAsync();
        }

        public async Task DeleteDirtyEntity(DirtyEntity entity)
        {
            await _connection.DeleteAsync(entity);
        }
    }

    public interface ISqoDataObject
    {
        int OID { get; set; }
    }
} 