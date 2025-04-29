// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.AtomHelper
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal static class AtomHelper
  {
    internal static bool IsAtomElement(XmlReader reader, string name)
    {
      return reader.NodeType == XmlNodeType.Element && reader.LocalName == name && (XNamespace) reader.NamespaceURI == FormatterConstants.AtomNamespaceUri;
    }

    internal static bool IsAtomTombstone(XmlReader reader, string name)
    {
      return reader.NodeType == XmlNodeType.Element && reader.LocalName == name && (XNamespace) reader.NamespaceURI == FormatterConstants.AtomDeletedEntryNamespace;
    }

    internal static bool IsODataNamespace(XmlReader reader, XNamespace ns)
    {
      return reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == ns.NamespaceName;
    }

    internal static bool IsSyncElement(XmlReader reader, string name)
    {
      return reader.NodeType == XmlNodeType.Element && reader.LocalName == name && reader.NamespaceURI == FormatterConstants.SyncNamespace.NamespaceName;
    }

    internal static string CleanInvalidXmlChars(string text)
    {
      string pattern = "[^\\x09\\x0A\\x0D\\x20-\\uD7FF\\uE000-\\uFFFD\\u10000-u10FFFF]";
      return Regex.Replace(text, pattern, "");
    }
  }
}
