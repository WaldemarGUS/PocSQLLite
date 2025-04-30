using Microsoft.Synchronization.ClientServices;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;
using SQLite;

namespace SiaqodbSyncProvider
{
    public class SQLiteOfflineEntity : ISqoDataObject, IOfflineEntity
    {
        private bool isTombstone;
        private bool isDirty;
        private string _etag;
        
        [Column("id_meta")]
        [JsonProperty]
        internal string IdMeta { get; set; }
        
        [Column("id_meta_hash")]
        [Indexed]
        internal int IdMetaHash { get; set; }
        
        [Ignore]
        private OfflineEntityMetadata _entityMetadata { get; set; }

        [Column("oid")]
        public int OID { get; set; }

        [JsonProperty]
        public virtual bool IsTombstone
        {
            get => isTombstone;
            set => isTombstone = value;
        }

        public virtual bool IsDirty
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
                    _entityMetadata.Id = IdMeta;
                    _entityMetadata.IsTombstone = isTombstone;
                }
                return _entityMetadata;
            }
            set
            {
                _entityMetadata = value;
                _etag = _entityMetadata.ETag;
                IdMeta = _entityMetadata.Id;
                IdMetaHash = IdMeta?.GetHashCode() ?? 0;
                isTombstone = _entityMetadata.IsTombstone;
            }
        }

        [Column("last_modified")]
        public DateTime LastModified { get; set; }

        public SQLiteOfflineEntity()
        {
        }
    }
} 