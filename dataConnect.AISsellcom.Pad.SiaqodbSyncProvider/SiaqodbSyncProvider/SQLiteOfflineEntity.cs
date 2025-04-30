using Microsoft.Synchronization.ClientServices;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace SiaqodbSyncProvider
{
    public class SQLiteOfflineEntity : ISqoDataObject, IOfflineEntity
    {
        private bool isTombstone;
        private bool isDirty;
        private string _etag;
        private string _idMeta;
        private int _idMetaHash;
        private OfflineEntityMetadata _entityMetadata;

        public int OID { get; set; }

        [JsonProperty]
        public bool IsTombstone
        {
            get => isTombstone;
            set => isTombstone = value;
        }

        public bool IsDirty
        {
            get => isDirty;
            set => isDirty = value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual OfflineEntityMetadata ServiceMetadata
        {
            get
            {
                if (_entityMetadata == null)
                {
                    _entityMetadata = new OfflineEntityMetadata();
                    _entityMetadata.ETag = _etag;
                    _entityMetadata.Id = _idMeta;
                    _entityMetadata.IsTombstone = isTombstone;
                }
                return _entityMetadata;
            }
            set
            {
                _entityMetadata = value;
                _etag = _entityMetadata.ETag;
                _idMeta = _entityMetadata.Id;
                _idMetaHash = _idMeta?.GetHashCode() ?? 0;
                isTombstone = _entityMetadata.IsTombstone;
            }
        }

        public DateTime LastModified { get; set; }

        public SQLiteOfflineEntity()
        {
        }
    }
} 