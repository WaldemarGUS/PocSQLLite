// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.DirtyEntity
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Sqo.Attributes;
using System.Reflection;
using SQLite;

namespace SiaqodbSyncProvider {
    [Obfuscation(Exclude = true)]
    public class DirtyEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [SQLite.MaxLength(200)]
        public string EntityType { get; set; }
        
        public System.Guid EntityOID { get; set; }
        
        public DirtyOperation DirtyOp { get; set; }
        
        public byte[] TombstoneObj { get; set; }

        public int OID { get; set; }
    }
}
