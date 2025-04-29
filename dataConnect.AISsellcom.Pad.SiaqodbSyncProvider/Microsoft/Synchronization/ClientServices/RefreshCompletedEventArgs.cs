// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.RefreshCompletedEventArgs
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.ClientServices {
    public class RefreshCompletedEventArgs : EventArgs
  {
    public Exception Error { get; private set; }

    public CacheRefreshStatistics Statistics { get; private set; }

    public bool Cancelled { get; private set; }

    internal RefreshCompletedEventArgs(
      CacheRefreshStatistics stats,
      Exception error,
      bool cancelled)
    {
      if (stats == null)
        throw new ArgumentNullException(nameof (stats));
      this.Statistics = stats;
      this.Error = error;
      this.Cancelled = cancelled;
    }
  }
}
