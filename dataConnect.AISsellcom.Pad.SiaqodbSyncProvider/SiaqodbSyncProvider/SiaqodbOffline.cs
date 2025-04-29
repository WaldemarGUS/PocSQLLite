// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.SiaqodbOffline
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using SiaqodbSyncProvider.Utilities;
using Sqo;
using Sqo.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiaqodbSyncProvider {
    public class SiaqodbOffline : Siaqodb {
        private readonly object _locker = "abc";
        private SiaqodbOfflineSyncProvider provider;

        public event EventHandler<SyncProgressEventArgs> SyncProgress;

        public event EventHandler<SyncCompletedEventArgs> SyncCompleted;

        public SiaqodbOffline(string path, Uri uri) : base(path)
        {
            this.provider = new SiaqodbOfflineSyncProvider(this, uri);
        }

        public SiaqodbOffline(string path, Uri uri, string scopeName) : base(path)
        {

            this.provider = new SiaqodbOfflineSyncProvider(this, uri, scopeName);
        }

        public SiaqodbOffline(string path, SiaqodbOfflineSyncProvider provider) : base(path)
        {

            this.provider = provider;
        }

        public SiaqodbOffline() : base()
        {
        }

        private void CreateDirtyEntity(object obj, DirtyOperation dop, ITransaction transaction)
        {
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = this.GetOID(obj);
            dirtyEntity.DirtyOp = dop;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(obj.GetType());
            if (transaction != null)
                base.StoreObject((object)dirtyEntity, transaction);
            else
                base.StoreObject((object)dirtyEntity);
        }

        private void CreateTombstoneDirtyEntity(
          SiaqodbOfflineEntity obj,
          int oid,
          ITransaction transaction)
        {
            bool noDeleteTracking = false;
            IEnumerable<DirtyEntity> thisEntityOperations = base.Query<DirtyEntity>()?.Where(x => x.EntityType == ReflectionHelper.GetDiscoveringTypeName(((object)obj).GetType()) && x.EntityOID == oid);
            noDeleteTracking = thisEntityOperations?.Any(o => o.DirtyOp == DirtyOperation.Inserted) ?? false;

            foreach (var item in thisEntityOperations)
            {
                if(item != null)
                {
                    base.Delete(item);
                }
            }
            if (noDeleteTracking)
            {
                return;
            }
            DirtyEntity dirtyEntity = new DirtyEntity();
            dirtyEntity.EntityOID = oid;
            dirtyEntity.EntityType = ReflectionHelper.GetDiscoveringTypeName(((object)obj).GetType());
            dirtyEntity.DirtyOp = DirtyOperation.Deleted;
            obj.IsDirty = true;
            obj.IsTombstone = true;
            dirtyEntity.TombstoneObj = JSerializer.Serialize((object)obj);
            if (transaction != null)
                base.StoreObject((object)dirtyEntity, transaction);
            else
                base.StoreObject((object)dirtyEntity);
        }

        public SiaqodbOfflineSyncProvider SyncProvider {
            get {
                return this.provider;
            }
            set {
                this.provider = value;
            }
        }

        public void StoreObject(object obj)
        {
            lock (this._locker)
            {
                SiaqodbOfflineEntity siaqodbOfflineEntity = obj as SiaqodbOfflineEntity;
                if (siaqodbOfflineEntity == null)
                    throw new Exception("Entity should be SiaqodbOfflineEntity type");
                siaqodbOfflineEntity.IsDirty = true;
                DirtyOperation dop = siaqodbOfflineEntity.OID == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
                base.StoreObject(obj);
                this.CreateDirtyEntity(obj, dop, (ITransaction)null);
            }
        }

        public void StoreObject(object obj, ITransaction transaction)
        {
            lock (this._locker)
            {
                SiaqodbOfflineEntity siaqodbOfflineEntity = obj as SiaqodbOfflineEntity;
                if (siaqodbOfflineEntity == null)
                    throw new Exception("Entity should be SiaqodbOfflineEntity type");
                siaqodbOfflineEntity.IsDirty = true;
                DirtyOperation dop = siaqodbOfflineEntity.OID == 0 ? DirtyOperation.Inserted : DirtyOperation.Updated;
                base.StoreObject(obj, transaction);
                this.CreateDirtyEntity(obj, dop, transaction);
            }
        }

        internal void StoreObjectBase(ISqoDataObject obj)
        {
            base.StoreObject((object)obj);
        }

        internal void StoreObjectBase(ISqoDataObject obj, ITransaction transaction)
        {
            base.StoreObject((object)obj, transaction);
        }

        public void Delete(object obj)
        {
            lock (this._locker)
            {
                SiaqodbOfflineEntity siaqodbOfflineEntity = obj as SiaqodbOfflineEntity;
                if (siaqodbOfflineEntity == null)
                    throw new Exception("Entity should be SqoOfflineEntity type");
                this.CreateTombstoneDirtyEntity(siaqodbOfflineEntity, siaqodbOfflineEntity.OID, (ITransaction)null);
                base.Delete(obj);
            }
        }

        public void Delete(object obj, ITransaction transaction)
        {
            lock (this._locker)
            {
                SiaqodbOfflineEntity siaqodbOfflineEntity = obj as SiaqodbOfflineEntity;
                if (siaqodbOfflineEntity == null)
                    throw new Exception("Entity should be SqoOfflineEntity type");
                this.CreateTombstoneDirtyEntity(siaqodbOfflineEntity, siaqodbOfflineEntity.OID, transaction);
                base.Delete(obj, transaction);
            }
        }

        internal void DeleteBase(object obj)
        {
            base.Delete(obj);
        }

        internal void DeleteBase(object obj, ITransaction transaction)
        {
            base.Delete(obj, transaction);
        }

        public bool UpdateObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }

        public bool UpdateObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }

        public bool UpdateObjectBy(object obj, ITransaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }

        internal bool UpdateObjectByBase(string fieldName, ISqoDataObject obj)
        {
            return base.UpdateObjectBy(fieldName, (object)obj);
        }

        internal bool UpdateObjectByBase(ISqoDataObject obj, params string[] fieldNames)
        {
            return base.UpdateObjectBy((object)obj, fieldNames);
        }

        internal bool UpdateObjectByBase(
          ISqoDataObject obj,
          ITransaction transaction,
          params string[] fieldNames)
        {
            return base.UpdateObjectBy((object)obj, transaction, fieldNames);
        }

        public bool DeleteObjectBy(object obj, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }

        public bool DeleteObjectBy(object obj, ITransaction transaction, params string[] fieldNames)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }

        public bool DeleteObjectBy(string fieldName, object obj)
        {
            throw new NotSupportedException("This method is not supported in SiaqodbOffline.");
        }

        internal bool DeleteObjectByBase(string fieldName, ISqoDataObject obj)
        {
            return base.DeleteObjectBy(fieldName, (object)obj);
        }

        internal bool DeleteObjectByBase(ISqoDataObject obj, params string[] fieldNames)
        {
            return base.DeleteObjectBy((object)obj, fieldNames);
        }

        internal bool DeleteObjectByBase(
          ISqoDataObject obj,
          ITransaction transaction,
          params string[] fieldNames)
        {
            return base.DeleteObjectBy((object)obj, transaction, fieldNames);
        }

        public void AddTypeForSync<T>() where T : IOfflineEntity
        {
            if (this.provider == null)
                throw new Exception("Provider cannot be null");
            this.provider.AddType<T>();
        }

        public async Task<CacheRefreshStatistics> Synchronize()
        {
            if (this.provider == null)
                throw new Exception("Provider cannot be null");
            this.provider.SyncProgress -= new EventHandler<SyncProgressEventArgs>(this.provider_SyncProgress);
            this.provider.SyncProgress += new EventHandler<SyncProgressEventArgs>(this.provider_SyncProgress);
            CacheRefreshStatistics stat = await this.provider.CacheController.SynchronizeAsync();
            SyncCompletedEventArgs args = new SyncCompletedEventArgs(stat.Cancelled, stat.Error, stat);
            this.OnSyncCompleted(args);
            return stat;
        }

        private void provider_SyncProgress(object sender, SyncProgressEventArgs e)
        {
            this.OnSyncProgress(e);
        }

        public void AddScopeParameters(string key, string value)
        {
            if (this.provider == null)
                throw new Exception("Provider cannot be null");
            this.provider.CacheController.ControllerBehavior.AddScopeParameters(key, value);
        }

        protected void OnSyncProgress(SyncProgressEventArgs args)
        {
            if (this.SyncProgress == null)
                return;
            this.SyncProgress((object)this, args);
        }

        protected void OnSyncCompleted(SyncCompletedEventArgs args)
        {
            if (this.SyncCompleted == null)
                return;
            this.SyncCompleted((object)this, args);
        }
    }
}
