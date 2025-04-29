// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.CacheRefreshStatistics
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.ClientServices {
    public class CacheRefreshStatistics
  {
    public Exception Error { get; set; }

    public bool Cancelled { get; set; }

    public DateTime StartTime { get; internal set; }

    public DateTime EndTime { get; internal set; }

    public uint TotalChangeSetsDownloaded { get; internal set; }

    public uint TotalChangeSetsUploaded { get; internal set; }

    public uint TotalUploads { get; internal set; }

    public uint TotalDownloads { get; internal set; }

    public uint TotalSyncConflicts { get; internal set; }

    public uint TotalSyncErrors { get; internal set; }
  }
}
