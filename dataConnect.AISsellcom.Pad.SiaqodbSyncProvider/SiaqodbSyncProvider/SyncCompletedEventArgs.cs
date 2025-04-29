// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.SyncCompletedEventArgs
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;

namespace SiaqodbSyncProvider {
    public class SyncCompletedEventArgs : EventArgs
  {
    public SyncCompletedEventArgs(
      bool cancelled,
      Exception error,
      CacheRefreshStatistics statistics)
    {
      this.Cancelled = cancelled;
      this.Error = error;
      this.Statistics = statistics;
    }

    public bool Cancelled { get; private set; }

    public Exception Error { get; private set; }

    public CacheRefreshStatistics Statistics { get; private set; }
  }
}
