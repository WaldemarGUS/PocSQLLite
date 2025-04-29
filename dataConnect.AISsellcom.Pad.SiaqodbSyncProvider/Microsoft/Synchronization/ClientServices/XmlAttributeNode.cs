// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlAttributeNode
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlAttributeNode : XmlNode
  {
    public XmlAttributeNode(XmlBufferReader bufferReader)
      : this(new StringHandle(bufferReader), new ValueHandle(bufferReader))
    {
    }

    private XmlAttributeNode(StringHandle localName, ValueHandle value)
      : base(XmlNodeType.Attribute, localName, value, XmlNode.XmlNodeFlags.CanGetAttribute | XmlNode.XmlNodeFlags.CanMoveToElement | XmlNode.XmlNodeFlags.HasValue | XmlNode.XmlNodeFlags.AtomicValue, ReadState.Interactive, new XmlAttributeTextNode(localName, value), 0)
    {
    }
  }
}
