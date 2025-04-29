// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.SiaqodbOfflineSyncProvider
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using Microsoft.Synchronization.Services;
using SiaqodbSyncProvider.Utilities;
using Sqo;
using Sqo.Exceptions;
using Sqo.Internal;
using Sqo.Transactions;
using Sqo.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SiaqodbSyncProvider {
    public class SiaqodbOfflineSyncProvider : OfflineSyncProvider {
        private Dictionary<Guid, ICollection<IOfflineEntity>> currentChanges = new Dictionary<Guid, ICollection<IOfflineEntity>>();
        private Dictionary<Guid, ICollection<DirtyEntity>> currentDirtyItems = new Dictionary<Guid, ICollection<DirtyEntity>>();
        private SiaqodbOffline siaqodb;

        public CacheController CacheController { get; set; }

        internal event EventHandler<SyncProgressEventArgs> SyncProgress;

        public event EventHandler<ConflictsEventArgs> ConflictOccur;

        public SiaqodbOfflineSyncProvider(SiaqodbOffline siaqodb, Uri uri) {
            if (!_bs._hsy())
                throw new Exception("Siaqodb Sync Provider License not valid!");
            this.CacheController = new CacheController(uri, "DefaultScope", (OfflineSyncProvider)this);
            this.siaqodb = siaqodb;
        }

        public SiaqodbOfflineSyncProvider(SiaqodbOffline siaqodb, Uri uri, string syncScopeName) {
            if (!_bs._hsy())
                throw new Exception("Siaqodb Sync Provider License not valid!");
            this.CacheController = new CacheController(uri, syncScopeName, (OfflineSyncProvider)this);
            this.siaqodb = siaqodb;
        }

        public bool UseElevatedTrust { get; set; }

        // Beginn change durch dC - dataConnect --> neuen Task erstellen (war async Methode ohne await)
        public override Task BeginSession() {
            return System.Threading.Tasks.Task.Run(() => {
                this.OnSyncProgress(new SyncProgressEventArgs("Synchronization started..."));
            });
        }
        // Ende change durch dC - dataConnect

        public override void EndSession() {
            this.siaqodb.Flush();
            this.OnSyncProgress(new SyncProgressEventArgs("Synchronization finished."));
        }

        // Beginn change durch dC - dataConnect --> neuen Task erstellen (war async Methode ohne await)
        public override Task<ChangeSet> GetChangeSet(Guid state) {
            return System.Threading.Tasks.Task.Run(() => {
                try {
                    ChangeSet changeSet = new ChangeSet();
                    this.OnSyncProgress(new SyncProgressEventArgs("Getting local changes..."));
                    KeyValuePair<ICollection<SiaqodbOfflineEntity>, ICollection<DirtyEntity>> changes = this.GetChanges();
                    this.OnSyncProgress(new SyncProgressEventArgs("Uploading:" + (object)changes.Key.Count + " changes..."));
                    changeSet.Data = (ICollection<IOfflineEntity>)changes.Key.Select<SiaqodbOfflineEntity, IOfflineEntity>((Func<SiaqodbOfflineEntity, IOfflineEntity>)(c => (IOfflineEntity)c)).ToList<IOfflineEntity>();
                    changeSet.IsLastBatch = true;
                    changeSet.ServerBlob = this.GetServerBlob();
                    if (changeSet.ServerBlob == null) {
                        changeSet.Data = (ICollection<IOfflineEntity>)new List<IOfflineEntity>();
                        changeSet.IsLastBatch = true;
                    }
                    this.currentChanges[state] = changeSet.Data;
                    this.currentDirtyItems[state] = changes.Value;
                    return changeSet;
                } catch (Exception ex) {
                    throw ex;
                }
            });
        }
        // Ende change durch dC - dataConnect

        private KeyValuePair<ICollection<SiaqodbOfflineEntity>, ICollection<DirtyEntity>> GetChanges() {
            List<SiaqodbOfflineEntity> siaqodbOfflineEntityList = new List<SiaqodbOfflineEntity>();
            IList<DirtyEntity> source1 = (IList<DirtyEntity>)this.siaqodb.LoadAll<DirtyEntity>();
            ILookup<string, DirtyEntity> lookup = source1.ToLookup<DirtyEntity, string>((Func<DirtyEntity, string>)(a => a.EntityType));
            foreach (IGrouping<string, DirtyEntity> grouping in (IEnumerable<IGrouping<string, DirtyEntity>>)lookup) {
                IEnumerable<DirtyEntity> source2 = lookup[grouping.Key];
                Type byDiscoveringName = ReflectionHelper.GetTypeByDiscoveringName(source2.First<DirtyEntity>().EntityType);
                Dictionary<int, Tuple<object, DirtyEntity>> dictionary1 = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> dictionary2 = new Dictionary<int, Tuple<object, DirtyEntity>>();
                Dictionary<int, Tuple<object, DirtyEntity>> dictionary3 = new Dictionary<int, Tuple<object, DirtyEntity>>();
                if (this.CacheController.ControllerBehavior.KnownTypes.Contains(byDiscoveringName)) {
                    foreach (DirtyEntity dirtyEntity in source2) {
                        try {
                            if (dirtyEntity.DirtyOp == DirtyOperation.Deleted) {
                                if (dictionary1.ContainsKey(dirtyEntity.EntityOID)) {
                                    dictionary1.Remove(dirtyEntity.EntityOID);
                                    continue;
                                }
                                if (dictionary2.ContainsKey(dirtyEntity.EntityOID))
                                    dictionary2.Remove(dirtyEntity.EntityOID);
                            } else if (dictionary3.ContainsKey(dirtyEntity.EntityOID) || dictionary1.ContainsKey(dirtyEntity.EntityOID) || dictionary2.ContainsKey(dirtyEntity.EntityOID))
                                continue;
                            object obj = dirtyEntity.DirtyOp != DirtyOperation.Deleted ? _bs._lobjby((Siaqodb)this.siaqodb, byDiscoveringName, dirtyEntity.EntityOID) : (object)(SiaqodbOfflineEntity)JSerializer.Deserialize(byDiscoveringName, dirtyEntity.TombstoneObj);
                            if (dirtyEntity.DirtyOp == DirtyOperation.Inserted)
                                dictionary1.Add(dirtyEntity.EntityOID, new Tuple<object, DirtyEntity>(obj, dirtyEntity));
                            else if (dirtyEntity.DirtyOp == DirtyOperation.Updated)
                                dictionary2.Add(dirtyEntity.EntityOID, new Tuple<object, DirtyEntity>(obj, dirtyEntity));
                            else if (dirtyEntity.DirtyOp == DirtyOperation.Deleted)
                                dictionary3.Add(dirtyEntity.EntityOID, new Tuple<object, DirtyEntity>(obj, dirtyEntity));
                            siaqodbOfflineEntityList.Add((SiaqodbOfflineEntity)obj);
                        } catch (Exception ex) {

                            throw ex;
                        }
                    }
                }
            }
            return new KeyValuePair<ICollection<SiaqodbOfflineEntity>, ICollection<DirtyEntity>>((ICollection<SiaqodbOfflineEntity>)siaqodbOfflineEntityList, (ICollection<DirtyEntity>)source1);
        }


        public string ClientScopeName {
            get {
                string result = null;

                byte[] blob = this.GetServerBlob();

                if (blob != null && blob.Any()) {
                    string s = Convert.ToBase64String(blob);

                    SyncBlob syncBlob = SyncBlob.DeSerialize(blob);
                    result = syncBlob.ClientScopeName;
                }

                return result;
            }
        }

        // Beginn change durch dC - dataConnect --> neuen Task erstellen (war async Methode ohne await)
        public override Task OnChangeSetUploaded(Guid state, ChangeSetResponse response) {
            return System.Threading.Tasks.Task.Run(() => {
                if (response == null)
                    throw new ArgumentNullException(nameof(response));
                if (response.Error != null)
                    throw response.Error;
                this.OnSyncProgress(new SyncProgressEventArgs("Upload finished,mark local entities as uploaded..."));
                ITransaction transaction = this.siaqodb.BeginTransaction();
                try {
                    if (response.UpdatedItems != null && (uint)response.UpdatedItems.Count > 0U) {
                        foreach (IOfflineEntity updatedItem in response.UpdatedItems) {
                            try {
                                IOfflineEntity item = updatedItem;
                                SiaqodbOfflineEntity offlineEntity = (SiaqodbOfflineEntity)item;
                                offlineEntity.IsDirty = false;
                                offlineEntity.IsTombstone = false;
                                this.SaveEntityByPK(offlineEntity, transaction);
                                offlineEntity = (SiaqodbOfflineEntity)null;
                                item = (IOfflineEntity)null;
                            } catch (Exception) {
                                throw;
                            }
                        }
                    }
                    if (response.Conflicts != null && response.Conflicts.Count > 0) {
                        ConflictsEventArgs ceArgs = new ConflictsEventArgs((IEnumerable<Conflict>)response.Conflicts);
                        this.OnConflictOccur(ceArgs);
                        if (!ceArgs.CancelResolvingConflicts) {
                            foreach (Conflict conflict1 in response.Conflicts) {
                                Conflict conflict = conflict1;
                                SiaqodbOfflineEntity offlineEntity = (SiaqodbOfflineEntity)conflict.LiveEntity;
                                offlineEntity.IsDirty = false;
                                offlineEntity.IsTombstone = false;
                                this.SaveEntity(offlineEntity, transaction);
                                offlineEntity = (SiaqodbOfflineEntity)null;
                                conflict = (Conflict)null;
                            }
                        }
                        ceArgs = (ConflictsEventArgs)null;
                    }
                    ICollection<IOfflineEntity> changesJustUploaded = this.currentChanges[state];
                    int counter = 0;
                    foreach (IOfflineEntity offlineEntity1 in ((IEnumerable<IOfflineEntity>)changesJustUploaded).Where(x => x != null)) {
                        IOfflineEntity enI = offlineEntity1;
                        SiaqodbOfflineEntity en = enI as SiaqodbOfflineEntity;

                        var updatedItemsList = response.UpdatedItems.ToList();

                        counter++;
                        if (updatedItemsList != null && (uint)updatedItemsList.Count > 0U) {
                            try {

                                bool existsUpdated = updatedItemsList.Any(offlineEntity => offlineEntity is SiaqodbOfflineEntity && this.EntitiesEqualByPK(offlineEntity as SiaqodbOfflineEntity, en));
                                if (existsUpdated)
                                    continue;
                            } catch (Exception ex) {
                                throw ex;
                            }

                            //bool existsUpdated = false;
                            //foreach (IOfflineEntity updatedItem in response.UpdatedItems)
                            //{
                            //    IOfflineEntity item = updatedItem;
                            //    SiaqodbOfflineEntity offlineEntity = (SiaqodbOfflineEntity)item;
                            //    if (this.EntitiesEqualByPK(offlineEntity, en)) {
                            //        existsUpdated = true;
                            //        break;
                            //    }

                            //    offlineEntity = (SiaqodbOfflineEntity)null;
                            //    item = (IOfflineEntity)null;
                            //}
                            //if (existsUpdated)
                            //    continue;
                        }
                        if (response.Conflicts != null && response.Conflicts.Count > 0) {
                            bool existsUpdated = false;
                            foreach (Conflict conflict1 in response.Conflicts) {
                                Conflict conflict = conflict1;
                                SiaqodbOfflineEntity offlineEntity = (SiaqodbOfflineEntity)conflict.LiveEntity;
                                if (this.EntitiesEqualByPK(offlineEntity, en)) {
                                    existsUpdated = true;
                                    break;
                                }

                                offlineEntity = (SiaqodbOfflineEntity)null;
                                conflict = (Conflict)null;
                            }
                            if (existsUpdated)
                                continue;
                        }
                        if (!en.IsTombstone) {
                            en.IsDirty = false;
                            // Beginn change durch dC - dataConnect
                            try {
                                this.siaqodb.StoreObjectBase((ISqoDataObject)en, transaction);
                            } catch (Exception ex) {
                                if (typeof(Sqo.Exceptions.OptimisticConcurrencyException) != ex.GetType() && !ex.Message.StartsWith("Another version of object with OID")) {
                                    throw ex;
                                }
                            }
                            // Ende Change
                        }
                        en = (SiaqodbOfflineEntity)null;
                        enI = (IOfflineEntity)null;
                    }
                    foreach (DirtyEntity dirtyEntity in (IEnumerable<DirtyEntity>)this.currentDirtyItems[state]) {
                        DirtyEntity dEn = dirtyEntity;
                        this.siaqodb.DeleteBase((object)dEn, transaction);
                        dEn = (DirtyEntity)null;
                    }
                    this.SaveAnchor(response.ServerBlob);
                    transaction.Commit();
                    changesJustUploaded = (ICollection<IOfflineEntity>)null;
                } catch (Exception ex) {
                    transaction.Rollback();
                    throw ex;
                } finally {
                    this.siaqodb.Flush();
                }
                this.OnSyncProgress(new SyncProgressEventArgs("Downloading changes from server..."));
                this.currentChanges.Remove(state);
            });
        }
        // Ende change durch dC - dataConnect

        private Dictionary<Type, List<PropertyInfo>> reflectedPropertiesPerType = new Dictionary<Type, List<PropertyInfo>>();

        private bool EntitiesEqualByPK(SiaqodbOfflineEntity a, SiaqodbOfflineEntity b) {
            if (((object)a).GetType() == ((object)b).GetType()) {
                List<PropertyInfo> propertyInfoList = null;
                reflectedPropertiesPerType.TryGetValue(a.GetType(), out propertyInfoList);

                if (propertyInfoList == null) {
                    propertyInfoList = new List<PropertyInfo>();
                    foreach (PropertyInfo property in (IEnumerable<PropertyInfo>)((object)a).GetType().GetProperties()) {
                        Type attributeType = typeof(KeyAttribute);
                        object[] customAttributes = property.GetCustomAttributes(attributeType, false);
                        if (customAttributes != null && ((IEnumerable<object>)customAttributes).ToList<object>().Count > 0)
                            propertyInfoList.Add(property);
                    }

                    reflectedPropertiesPerType.Add(a.GetType(), propertyInfoList);
                }
                if (propertyInfoList.Count > 0) {
                    foreach (PropertyInfo propertyInfo in propertyInfoList) {
                        object obj1 = propertyInfo.GetValue((object)a);
                        object obj2 = propertyInfo.GetValue((object)b);
                        if (obj1 == null || obj2 == null) {
                            if (obj1 != obj2)
                                return false;
                        } else if ((uint)((IComparable)obj1).CompareTo((object)(IComparable)obj2) > 0U)
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public void SaveAnchor(byte[] anchor) {
            string str = "anchor_" + this.CacheController.ControllerBehavior.ScopeName;
            _bs._sanc((Siaqodb)this.siaqodb, anchor, str);
        }

        public bool DropAnchor() {
            _bs._danc((Siaqodb)this.siaqodb, "anchor_" + this.CacheController.ControllerBehavior.ScopeName);
            return true;
        }

        public override byte[] GetServerBlob() {
            return _bs._ganc((Siaqodb)this.siaqodb, "anchor_" + this.CacheController.ControllerBehavior.ScopeName);
        }

        // Beginn change durch dC - dataConnect --> neuen Task erstellen (war async Methode ohne await)
        public override Task SaveChangeSet(ChangeSet changeSet) {
            return System.Threading.Tasks.Task.Run(() => {
                if (changeSet == null) {
                    throw new ArgumentException("changeSet is null", nameof(changeSet));
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Download finished, saving object on local db..."));
                IEnumerable<SiaqodbOfflineEntity> entities = changeSet.Data.Cast<SiaqodbOfflineEntity>();

                try {
                    var groupedEntities = entities.GroupBy(x => x.GetType().FullName);

                    foreach (var typeToImport in groupedEntities) {
                        this.SaveDownloadedChanges(changeSet.ServerBlob, typeToImport.AsEnumerable());
                    }
                } catch (Exception) {
                    throw;
                }

                this.OnSyncProgress(new SyncProgressEventArgs("Sync finished!"));
            });
        }
        // Ende change durch dC - dataConnect
        public void AddType<T>() where T : IOfflineEntity {
            this.CacheController.ControllerBehavior.AddType<T>();
        }

        private void SaveDownloadedChanges(byte[] anchor, IEnumerable<SiaqodbOfflineEntity> entities, int level = 0, Exception lastEx = null) {
            ITransaction transaction = this.siaqodb.BeginTransaction("storeTypeTransaction");
            entities = entities.GroupBy(y => y._idMetaHash).Select(g => g.First());
            try {
                foreach (SiaqodbOfflineEntity entity in entities) {
                    // Beginn change durch dC - dataConnect
                    try {
                        if (!entity.IsTombstone && level > 0) {
                            entity.OID = 0;
                        }

                        this.SaveEntity(entity, transaction);
                    } catch (Sqo.Exceptions.OptimisticConcurrencyException ex) {
                        if (!ex.Message.StartsWith("Another version of object with OID")) {
                            throw;
                        } else {
                            if (level > 3) {
                                lastEx.Data.Add("level", level);
                                lastEx.Data.Add("errorEntity", entity);
                                //lastEx.Data.Add("allEntities", entities);

                                throw lastEx;
                            }

                            TryRollbackAndDispose(transaction);
                            repairAllTypesInDB(entities);
                            this.SaveDownloadedChanges(anchor, entities, level + 1, ex);
                            return;
                        }
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine(entity.GetType().FullName);
                        System.Diagnostics.Debug.WriteLine(ex);
                        throw;
                    }

                }

                this.SaveAnchor(anchor);
                transaction.Commit();

            } catch (Exception ex) {

                var resizeDb = ex.Message.StartsWith("MDB_MAP_FULL") || ((ex is SiaqodbException siaqExc) && siaqExc.ChildException != null && siaqExc.ChildException.Message.StartsWith("MDB_MAP_FULL"));

                if (resizeDb)
                {
                    var maxSize = this.siaqodb.DbInfo.MaxSize;
                    var subDBSize = this.siaqodb.DbInfo.MaxSubDatabases;
                    var path = this.siaqodb.GetDBPath();

                    var newMaxSize = maxSize + (10 * 1024 * 1024);

                    TryRollbackAndDispose(transaction);

                    this.siaqodb.Close();
                    this.siaqodb.Open(path, newMaxSize, subDBSize);

                    System.Diagnostics.Debug.WriteLine($"------ Reopen Database with new MAX size. NewSize:{newMaxSize.ToString("N0")} OldMaxSize:{maxSize.ToString("N0")}");

                    SaveDownloadedChanges(anchor, entities);
                    return;
                } else {
                    throw;
                }
            }
            TryRollbackAndDispose(transaction);
            this.siaqodb.Flush();

        }

        private void TryRollbackAndDispose(ITransaction transaction) {
            try {
                transaction.Rollback();

            } catch (Exception) { }

            try {
                transaction.Dispose();
            } catch (Exception) { }
        }

        private void repairAllTypesInDB(IEnumerable<SiaqodbOfflineEntity> entities) {
            this.siaqodb.Flush();
            IEnumerable<Type> typeList = entities.Select(x => x.GetType()).GroupBy(y => y.FullName).Select(z => z.FirstOrDefault()).ToList();


            foreach (var type in typeList) {
                MethodInfo methodInfo = this.GetType().GetMethod("refreshType", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type);
                methodInfo.Invoke(this, new object[] { });
            }
            this.siaqodb.Flush();
        }

        private void refreshType<T>() where T : ISqoDataObject { // !!! Called by Reflection !!!
            try {
                var allValuesOfType = this.siaqodb.Query<T>().OrderBy(x => x.OID).ToList();
                this.siaqodb.DropType<T>();
                foreach (var item in allValuesOfType) {
                    this.siaqodb.StoreObjectBase(item);
                }
            } catch (Exception) {
                throw;
            }
        }
        // Ende change durch dC - dataConnect

        internal void SaveEntityByPK(SiaqodbOfflineEntity en, ITransaction transaction) {
            List<string> stringList = new List<string>();
            foreach (PropertyInfo property in (IEnumerable<PropertyInfo>)((object)en).GetType().GetProperties()) {
                Type attributeType = typeof(KeyAttribute);
                object[] customAttributes = property.GetCustomAttributes(attributeType, false);
                if (customAttributes != null && ((IEnumerable<object>)customAttributes).ToList<object>().Count > 0)
                    stringList.Add(ExternalMetaHelper.GetBackingField((MemberInfo)property));
            }
            if (stringList.Count <= 0)
                return;
            if (en.IsTombstone)
                this.siaqodb.DeleteObjectByBase((ISqoDataObject)en, transaction, stringList.ToArray());
            else
                this.siaqodb.UpdateObjectByBase((ISqoDataObject)en, transaction, stringList.ToArray());
        }

        internal void SaveEntity(SiaqodbOfflineEntity en, ITransaction transaction) {
            if (en.IsTombstone) {
                try {
                    this.siaqodb.DeleteObjectByBase((ISqoDataObject)en, transaction, "_idMetaHash");
                } catch (Exception ex) {
                    if (ex.Message.StartsWith("MDB_NOTFOUND")) {
                        return;
                    }

                    throw;
                }
            } else if (!this.siaqodb.UpdateObjectByBase((ISqoDataObject)en, transaction, "_idMetaHash"))
                try {
                    this.siaqodb.StoreObjectBase((ISqoDataObject)en, transaction);
                } catch (Exception) {
                    throw;
                }
        }

        protected virtual void OnConflictOccur(ConflictsEventArgs args) {
            if (this.ConflictOccur == null)
                return;
            this.ConflictOccur((object)this, args);
        }

        public void Reinitialize() {
            ITransaction itransaction = this.siaqodb.BeginTransaction();
            try {
                foreach (Type knownType in this.CacheController.ControllerBehavior.KnownTypes)
                    this.siaqodb.DropType(knownType, itransaction);
                this.DropAnchor();
                itransaction.Commit();
            } catch (Exception ex) {
                itransaction.Rollback();
                throw ex;
            }
        }

        internal void OnSyncProgress(SyncProgressEventArgs args) {
            if (this.SyncProgress == null)
                return;
            this.SyncProgress((object)this, args);
        }
    }
}
