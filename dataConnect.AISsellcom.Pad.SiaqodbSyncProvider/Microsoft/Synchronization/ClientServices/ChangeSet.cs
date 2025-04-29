// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.ChangeSet
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Collections.Generic;

namespace Microsoft.Synchronization.ClientServices {
    public class ChangeSet
  {
    public byte[] ServerBlob { get; set; }

    public ICollection<IOfflineEntity> Data { get; set; }

    public bool IsLastBatch { get; set; }

    public ChangeSet()
    {
      this.ServerBlob = (byte[]) null;
      this.Data = (ICollection<IOfflineEntity>) new List<IOfflineEntity>();
      this.IsLastBatch = true;
    }

    internal void AddItem(IOfflineEntity iOfflineEntity)
    {
      if (this.Data == null)
        this.Data = (ICollection<IOfflineEntity>) new List<IOfflineEntity>();
      this.Data.Add(iOfflineEntity);
    }
  }
}
