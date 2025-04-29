// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlComplexTextNode
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlComplexTextNode : XmlTextNode
  {
    public XmlComplexTextNode(XmlBufferReader bufferReader)
      : base(XmlNodeType.Text, new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlNode.XmlNodeFlags.HasValue | XmlNode.XmlNodeFlags.HasContent, ReadState.Interactive, (XmlAttributeTextNode) null, 0)
    {
    }
  }
}
