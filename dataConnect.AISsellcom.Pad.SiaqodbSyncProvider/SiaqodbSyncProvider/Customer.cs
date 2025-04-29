using SQLite;

namespace SiaqodbSyncProvider
{
    [Table("customer")]
    public class Customer : BaseEntity
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public new int Id { get; set; }

        public override int OID 
        { 
            get => Id; 
            set => Id = value; 
        }

        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
} 