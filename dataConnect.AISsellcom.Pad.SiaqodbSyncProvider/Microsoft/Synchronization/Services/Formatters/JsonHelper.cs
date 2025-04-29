// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.JsonHelper
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Xml;

namespace Microsoft.Synchronization.Services.Formatters {
    internal static class JsonHelper
  {
    public static bool IsElement(XmlReader reader, string elementName)
    {
      return reader.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase);
    }
  }
}
