using SQLite;
using System;

namespace SiaqodbSyncProvider
{
    public abstract class BaseEntity : ISqoDataObject
    {
        public virtual int OID { get; set; }

        // ISqoDataObject implementation
        public int Id { get => OID; set => OID = value; }
    }
} 