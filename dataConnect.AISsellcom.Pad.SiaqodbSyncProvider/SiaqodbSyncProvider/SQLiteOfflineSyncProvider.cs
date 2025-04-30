using Microsoft.Synchronization.ClientServices;
using Microsoft.Synchronization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiaqodbSyncProvider
{
    public class SQLiteOfflineSyncProvider : OfflineSyncProvider
    {
        private Dictionary<Guid, ICollection<IOfflineEntity>> currentChanges = new Dictionary<Guid, ICollection<IOfflineEntity>>();
        private Dictionary<Guid, ICollection<DirtyEntity>> currentDirtyItems = new Dictionary<Guid, ICollection<DirtyEntity>>();
        private SQLiteOffline sqlite;

        public CacheController CacheController { get; set; }

        internal event EventHandler<SyncProgressEventArgs> SyncProgress;

        public event EventHandler<ConflictsEventArgs> ConflictOccur;

        public SQLiteOfflineSyncProvider(SQLiteOffline sqlite, Uri uri)
        {
            this.CacheController = new CacheController(uri, "DefaultScope", this);
            this.sqlite = sqlite;
        }

        public SQLiteOfflineSyncProvider(SQLiteOffline sqlite, Uri uri, string syncScopeName)
        {
            this.CacheController = new CacheController(uri, syncScopeName, this);
            this.sqlite = sqlite;
        }

        public bool UseElevatedTrust { get; set; }

        public string ClientScopeName
        {
            get
            {
                string result = null;
                byte[] blob = this.GetServerBlob();

                if (blob != null && blob.Any())
                {
                    string s = Convert.ToBase64String(blob);
                    SyncBlob syncBlob = SyncBlob.DeSerialize(blob);
                    result = syncBlob.ClientScopeName;
                }

                return result;
            }
        }

        public override Task BeginSession()
        {
            return Task.Run(() =>
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
            });
        }

        public override void EndSession()
        {
            this.OnSyncProgress(new SyncProgressEventArgs("Synchronization finished."));
        }

        public override async Task<ChangeSet> GetChangeSet(Guid state)
        {
            try
            {
                ChangeSet changeSet = new ChangeSet();
                this.OnSyncProgress(new SyncProgressEventArgs("Getting local changes..."));
                
                var changes = await GetChanges();
                this.OnSyncProgress(new SyncProgressEventArgs($"Uploading: {changes.Key.Count} changes..."));

                changeSet.Data = changes.Key.Select(c => (IOfflineEntity)c).ToList();
                changeSet.IsLastBatch = true;
                changeSet.ServerBlob = this.GetServerBlob();

                if (changeSet.ServerBlob == null)
                {
                    changeSet.Data = new List<IOfflineEntity>();
                    changeSet.IsLastBatch = true;
                }

                this.currentChanges[state] = changeSet.Data;
                this.currentDirtyItems[state] = changes.Value;
                return changeSet;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<KeyValuePair<ICollection<SQLiteOfflineEntity>, ICollection<DirtyEntity>>> GetChanges()
        {
            List<SQLiteOfflineEntity> entities = new List<SQLiteOfflineEntity>();
            var dirtyEntities = await sqlite.LoadAll<DirtyEntity>();

            foreach (var entity in dirtyEntities)
            {
                try
                {
                    var obj = await sqlite.LoadObjectByOID<SQLiteOfflineEntity>(Guid.NewGuid());
                    if (obj != null)
                    {
                        entities.Add(obj);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return new KeyValuePair<ICollection<SQLiteOfflineEntity>, ICollection<DirtyEntity>>(entities, dirtyEntities);
        }

        public override byte[] GetServerBlob()
        {
            return sqlite.GetServerBlob(CacheController.ControllerBehavior.ScopeName);
        }

        public override async Task SaveChangeSet(ChangeSet changeSet)
        {
            if (changeSet == null)
                return;

            try
            {
                this.OnSyncProgress(new SyncProgressEventArgs("Download finished, saving object on local db..."));
                IEnumerable<SQLiteOfflineEntity> entities = changeSet.Data.Cast<SQLiteOfflineEntity>();

                var groupedEntities = entities.GroupBy(x => x.GetType().FullName);

                foreach (var typeToImport in groupedEntities)
                {
                    await SaveDownloadedChanges(changeSet.ServerBlob, typeToImport.AsEnumerable());
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Sync finished!"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task SaveDownloadedChanges(byte[] anchor, IEnumerable<SQLiteOfflineEntity> entities, int level = 0, Exception lastEx = null)
        {
            try
            {
                foreach (var entity in entities)
                {
                    if (!entity.IsTombstone && level > 0)
                    {
                        entity.OID = 0;
                    }

                    await sqlite.StoreObject(entity);
                }

                sqlite.SaveAnchor(anchor, CacheController.ControllerBehavior.ScopeName);
            }
            catch (Exception ex)
            {
                if (level > 3)
                {
                    lastEx.Data.Add("level", level);
                    lastEx.Data.Add("errorEntity", entities.First());
                    throw lastEx;
                }

                await SaveDownloadedChanges(anchor, entities, level + 1, ex);
            }
        }

        public override async Task OnChangeSetUploaded(Guid state, ChangeSetResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (response.Error != null)
                throw response.Error;

            this.OnSyncProgress(new SyncProgressEventArgs("Upload finished, marking local entities as uploaded..."));

            try
            {
                if (response.UpdatedItems != null && response.UpdatedItems.Count > 0)
                {
                    foreach (var item in response.UpdatedItems)
                    {
                        if (item is SQLiteOfflineEntity entity)
                        {
                            entity.IsDirty = false;
                            entity.IsTombstone = false;
                            await sqlite.StoreObject(entity);
                        }
                    }
                }

                if (response.Conflicts != null && response.Conflicts.Count > 0)
                {
                    var ceArgs = new ConflictsEventArgs(response.Conflicts);
                    this.OnConflictOccur(ceArgs);

                    if (!ceArgs.CancelResolvingConflicts)
                    {
                        foreach (var conflict in response.Conflicts)
                        {
                            if (conflict.LiveEntity is SQLiteOfflineEntity entity)
                            {
                                entity.IsDirty = false;
                                entity.IsTombstone = false;
                                await sqlite.StoreObject(entity);
                            }
                        }
                    }
                }

                var changesJustUploaded = this.currentChanges[state];
                foreach (var offlineEntity in changesJustUploaded.Where(x => x != null))
                {
                    if (offlineEntity is SQLiteOfflineEntity en)
                    {
                        if (!en.IsTombstone)
                        {
                            en.IsDirty = false;
                            await sqlite.StoreObject(en);
                        }
                    }
                }

                foreach (var dirtyEntity in this.currentDirtyItems[state])
                {
                    await sqlite.Delete(dirtyEntity);
                }

                sqlite.SaveAnchor(response.ServerBlob, CacheController.ControllerBehavior.ScopeName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            this.OnSyncProgress(new SyncProgressEventArgs("Downloading changes from server..."));
            this.currentChanges.Remove(state);
        }

        public void AddType<T>() where T : IOfflineEntity
        {
            this.CacheController.ControllerBehavior.AddType<T>();
        }

        public void Reinitialize()
        {
            try
            {
                foreach (Type knownType in this.CacheController.ControllerBehavior.KnownTypes)
                {
                    sqlite.DropType(knownType);
                }
                sqlite.DropAnchor(CacheController.ControllerBehavior.ScopeName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected virtual void OnConflictOccur(ConflictsEventArgs args)
        {
            this.ConflictOccur?.Invoke(this, args);
        }

        internal void OnSyncProgress(SyncProgressEventArgs args)
        {
            this.SyncProgress?.Invoke(this, args);
        }
    }
} 