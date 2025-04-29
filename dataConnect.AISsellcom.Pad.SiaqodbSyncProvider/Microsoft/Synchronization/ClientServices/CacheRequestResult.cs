// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.CacheRequestResult
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.ClientServices {
    public class CacheRequestResult
  {
    public Guid Id { get; set; }

    public ChangeSet ChangeSet { get; set; }

    public ChangeSetResponse ChangeSetResponse { get; set; }

    public Exception Error { get; set; }

    public object State { get; set; }

    public HttpState HttpStep { get; set; }

    public uint BatchUploadCount { get; set; }

    public CacheRequestResult(
      Guid id,
      ChangeSetResponse response,
      int uploadCount,
      Exception error,
      HttpState step,
      object state)
    {
      this.ChangeSetResponse = response;
      this.Error = error;
      this.State = state;
      this.HttpStep = step;
      this.Id = id;
      this.BatchUploadCount = (uint) uploadCount;
      if (this.Error == null)
        return;
      if (this.ChangeSetResponse == null)
        this.ChangeSetResponse = new ChangeSetResponse();
      this.ChangeSetResponse.Error = this.Error;
    }

    public CacheRequestResult(
      Guid id,
      ChangeSet changeSet,
      Exception error,
      HttpState step,
      object state)
    {
      this.ChangeSet = changeSet;
      this.Error = error;
      this.State = state;
      this.Id = id;
      this.HttpStep = step;
    }
  }
}
