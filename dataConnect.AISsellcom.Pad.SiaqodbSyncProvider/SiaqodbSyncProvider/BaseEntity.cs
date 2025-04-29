using SQLite;
using System;

namespace SiaqodbSyncProvider
{
    public abstract class BaseEntity : ISqoDataObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ISqoDataObject implementation
        public int OID { get => Id; set => Id = value; }
    }
} 