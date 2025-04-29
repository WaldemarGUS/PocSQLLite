// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.HttpState
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

namespace Microsoft.Synchronization.ClientServices {
    public enum HttpState
  {
    Start,
    WriteRequest,
    ReadResponse,
    End,
  }
}
