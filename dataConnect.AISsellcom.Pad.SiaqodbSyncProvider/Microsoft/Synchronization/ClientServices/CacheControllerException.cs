// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.CacheControllerException
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.ClientServices {
    public class CacheControllerException : Exception
  {
    public CacheControllerException(string message)
      : base(message)
    {
    }

    public CacheControllerException(string message, Exception inner)
      : base(message, inner)
    {
    }

    public CacheControllerException()
    {
    }

    internal static CacheControllerException CreateCacheBusyException()
    {
      return new CacheControllerException("Cannot complete WebRequest as another Refresh() operation is in progress.");
    }
  }
}
