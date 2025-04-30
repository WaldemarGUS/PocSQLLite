// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.DirtyEntity
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using SQLite;
using System;

namespace SiaqodbSyncProvider
{
    [Table("dirty_entities")]
    public class DirtyEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("entity_oid")]
        public int EntityOID { get; set; }

        [Column("entity_type")]
        public string EntityType { get; set; }

        [Column("dirty_op")]
        public DirtyOperation DirtyOp { get; set; }

        [Column("tombstone_obj")]
        public string TombstoneObj { get; set; }
    }
}


