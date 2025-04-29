using SQLite;

namespace SiaqodbSyncProvider
{
    public class SyncMetadata
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string ScopeName { get; set; }
        
        public string LastSyncAnchor { get; set; }
        
        public string LastSyncTime { get; set; }
    }
} 