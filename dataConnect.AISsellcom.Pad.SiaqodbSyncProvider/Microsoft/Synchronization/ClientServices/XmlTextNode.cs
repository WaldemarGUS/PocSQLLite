// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlTextNode
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlTextNode : XmlNode
  {
    protected XmlTextNode(
      XmlNodeType nodeType,
      StringHandle localName,
      ValueHandle value,
      XmlNode.XmlNodeFlags nodeFlags,
      ReadState readState,
      XmlAttributeTextNode attributeTextNode,
      int depthDelta)
      : base(nodeType, localName, value, nodeFlags, readState, attributeTextNode, depthDelta)
    {
    }
  }
}
