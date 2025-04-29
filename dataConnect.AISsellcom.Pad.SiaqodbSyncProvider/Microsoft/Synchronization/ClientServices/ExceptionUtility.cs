// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.ExceptionUtility
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Synchronization.ClientServices {
    internal static class ExceptionUtility
  {
    internal static bool IsFatal(Exception exception)
    {
      for (; exception != null; exception = exception.InnerException)
      {
        if (exception is OutOfMemoryException || exception is SEHException)
          return true;
      }
      return false;
    }
  }
}
