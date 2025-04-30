// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.DirtyEntity
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Reflection;
using SQLite;

namespace SiaqodbSyncProvider
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    internal class DirtyEntity
    {
        [PrimaryKey, AutoIncrement]
        public int OID { get; set; }

        public int EntityOID { get; set; }
        
        [MaxLength(200)]
        public string EntityType { get; set; }
        
        public DirtyOperation DirtyOp { get; set; }
        
        public byte[] TombstoneObj { get; set; }
    }
}


