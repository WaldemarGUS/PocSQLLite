// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlEndOfFileNode
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlEndOfFileNode : XmlNode
  {
    public XmlEndOfFileNode(XmlBufferReader bufferReader)
      : base(XmlNodeType.None, new StringHandle(bufferReader), new ValueHandle(bufferReader), XmlNode.XmlNodeFlags.None, ReadState.EndOfFile, (XmlAttributeTextNode) null, 0)
    {
    }
  }
}
