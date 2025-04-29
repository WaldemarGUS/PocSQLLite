// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.AsyncWorkManagerException
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.ClientServices {
    internal class AsyncWorkManagerException : Exception
  {
    public AsyncWorkManagerException(string message)
      : base(message)
    {
    }

    public AsyncWorkManagerException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
