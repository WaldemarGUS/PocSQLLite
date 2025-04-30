using SQLite;
using Microsoft.Synchronization.ClientServices;

namespace SiaqodbSyncProvider
{
    [Table("hp")]
    public class Hp : SQLiteOfflineEntity
    {
        [PrimaryKey]
        [Column("tid")]
        public System.Guid TId { get; set; }

        [Column("aisstatus")]
        public short AISStatus { get; set; }

        [Column("hpnr")]
        public string HpNr { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Ignore]
        public override OfflineEntityMetadata ServiceMetadata { get; set; }

        [Ignore]
        public override bool IsTombstone { get; set; }

        [Ignore]
        public override bool IsDirty { get; set; }
    }
} 


