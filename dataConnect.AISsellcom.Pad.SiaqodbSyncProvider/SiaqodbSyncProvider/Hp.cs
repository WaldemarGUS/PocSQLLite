using SQLite;

namespace SiaqodbSyncProvider
{
    [Table("hp")]
    public class Hp
    {
        [PrimaryKey]
        [Column("tid")]
        public System.Guid TId { get; set; }

        [Column("aisstatus")]
        public int AISStatus { get; set; }

        [Column("hpnr")]
        public string HpNr { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
} 


