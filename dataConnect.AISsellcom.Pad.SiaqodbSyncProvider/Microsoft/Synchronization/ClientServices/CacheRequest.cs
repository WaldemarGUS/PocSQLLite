// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.CacheRequest
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Collections.Generic;

namespace Microsoft.Synchronization.ClientServices {
    internal class CacheRequest
  {
    public Guid RequestId;
    public ICollection<IOfflineEntity> Changes;
    public CacheRequestType RequestType;
    public SerializationFormat Format;
    public byte[] KnowledgeBlob;
    public bool IsLastBatch;
  }
}
