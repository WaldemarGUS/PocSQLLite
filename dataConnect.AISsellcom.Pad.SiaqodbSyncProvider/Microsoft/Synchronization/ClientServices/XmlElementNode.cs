// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlElementNode
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlElementNode : XmlNode
  {
    private XmlEndElementNode endElementNode;
    private int bufferOffset;
    public int NameOffset;
    public int NameLength;

    public XmlEndElementNode EndElement
    {
      get
      {
        return this.endElementNode;
      }
    }

    public int BufferOffset
    {
      get
      {
        return this.bufferOffset;
      }
      set
      {
        this.bufferOffset = value;
      }
    }

    public XmlElementNode(XmlBufferReader bufferReader)
      : this(new StringHandle(bufferReader), new ValueHandle(bufferReader))
    {
    }

    private XmlElementNode(StringHandle localName, ValueHandle value)
      : base(XmlNodeType.Element, localName, value, XmlNode.XmlNodeFlags.CanGetAttribute | XmlNode.XmlNodeFlags.HasContent, ReadState.Interactive, (XmlAttributeTextNode) null, -1)
    {
      this.endElementNode = new XmlEndElementNode(localName, value);
    }
  }
}
