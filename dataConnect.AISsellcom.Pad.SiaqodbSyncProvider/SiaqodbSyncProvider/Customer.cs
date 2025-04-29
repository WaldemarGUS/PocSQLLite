using SQLite;

namespace SiaqodbSyncProvider
{
    [Table("customer")]
    public class Customer : BaseEntity
    {
        [Column("id")]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
} 