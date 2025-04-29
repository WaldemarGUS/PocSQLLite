// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.ProcessCacheRequestCompletedEventArgs
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.ClientServices {
    internal class ProcessCacheRequestCompletedEventArgs : EventArgs
  {
    public Guid Id;
    public ChangeSet ChangeSet;
    public ChangeSetResponse ChangeSetResponse;
    public Exception Error;
    public object State;
    public uint BatchUploadCount;

    public ProcessCacheRequestCompletedEventArgs(
      Guid id,
      ChangeSetResponse response,
      int uploadCount,
      Exception error,
      object state)
    {
      this.ChangeSetResponse = response;
      this.Error = error;
      this.State = state;
      this.Id = id;
      this.BatchUploadCount = (uint) uploadCount;
      if (this.Error == null)
        return;
      if (this.ChangeSetResponse == null)
        this.ChangeSetResponse = new ChangeSetResponse();
      this.ChangeSetResponse.Error = this.Error;
    }

    public ProcessCacheRequestCompletedEventArgs(
      Guid id,
      ChangeSet changeSet,
      Exception error,
      object state)
    {
      this.ChangeSet = changeSet;
      this.Error = error;
      this.State = state;
      this.Id = id;
    }
  }
}
