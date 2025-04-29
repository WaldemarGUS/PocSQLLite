using SQLite;

namespace PocSQLLite
{
    public class BaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int OID { get; set; }
    }
} 