// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.DirtyEntity
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Sqo.Attributes;
using System.Reflection;

namespace SiaqodbSyncProvider {
    [Obfuscation(Exclude = true)]
  internal class DirtyEntity
  {
    public int EntityOID;
    [MaxLength(200)]
    public string EntityType;
    public DirtyOperation DirtyOp;
    public byte[] TombstoneObj;

    public int OID { get; set; }
  }
}
