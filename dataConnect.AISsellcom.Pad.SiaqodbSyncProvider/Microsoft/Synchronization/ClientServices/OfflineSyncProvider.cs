// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.OfflineSyncProvider
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Threading.Tasks;

namespace Microsoft.Synchronization.ClientServices {
    public abstract class OfflineSyncProvider
  {
    public abstract Task BeginSession();

    public abstract Task<ChangeSet> GetChangeSet(Guid state);

    public abstract Task OnChangeSetUploaded(Guid state, ChangeSetResponse response);

    public abstract byte[] GetServerBlob();

    public abstract Task SaveChangeSet(ChangeSet changeSet);

    public abstract void EndSession();
  }
}
