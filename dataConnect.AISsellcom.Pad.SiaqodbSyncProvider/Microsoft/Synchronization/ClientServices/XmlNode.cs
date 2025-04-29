// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlNode
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlNode
  {
    private XmlNodeType nodeType;
    private StringHandle localName;
    private ValueHandle value;
    private bool hasValue;
    private bool canGetAttribute;
    private bool canMoveToElement;
    private ReadState readState;
    private XmlAttributeTextNode attributeTextNode;
    private bool exitScope;
    private int depthDelta;
    private bool skipValue;
    private bool hasContent;
    private bool isEmptyElement;
    private char quoteChar;
    private bool isAtomicValue;

    public bool HasValue
    {
      get
      {
        return this.hasValue;
      }
    }

    public ReadState ReadState
    {
      get
      {
        return this.readState;
      }
    }

    public StringHandle LocalName
    {
      get
      {
        return this.localName;
      }
    }

    public bool CanGetAttribute
    {
      get
      {
        return this.canGetAttribute;
      }
    }

    public bool CanMoveToElement
    {
      get
      {
        return this.canMoveToElement;
      }
    }

    public XmlAttributeTextNode AttributeText
    {
      get
      {
        return this.attributeTextNode;
      }
    }

    public bool SkipValue
    {
      get
      {
        return this.skipValue;
      }
    }

    public ValueHandle Value
    {
      get
      {
        return this.value;
      }
    }

    public int DepthDelta
    {
      get
      {
        return this.depthDelta;
      }
    }

    public bool HasContent
    {
      get
      {
        return this.hasContent;
      }
    }

    public XmlNodeType NodeType
    {
      get
      {
        return this.nodeType;
      }
      set
      {
        this.nodeType = value;
      }
    }

    public bool IsAtomicValue
    {
      get
      {
        return this.isAtomicValue;
      }
      set
      {
        this.isAtomicValue = value;
      }
    }

    public bool ExitScope
    {
      get
      {
        return this.exitScope;
      }
      set
      {
        this.exitScope = value;
      }
    }

    public bool IsEmptyElement
    {
      get
      {
        return this.isEmptyElement;
      }
      set
      {
        this.isEmptyElement = value;
      }
    }

    public char QuoteChar
    {
      get
      {
        return this.quoteChar;
      }
      set
      {
        this.quoteChar = value;
      }
    }

    public string ValueAsString
    {
      get
      {
        return this.Value.GetString();
      }
    }

    protected XmlNode(
      XmlNodeType nodeType,
      StringHandle localName,
      ValueHandle value,
      XmlNode.XmlNodeFlags nodeFlags,
      ReadState readState,
      XmlAttributeTextNode attributeTextNode,
      int depthDelta)
    {
      this.nodeType = nodeType;
      this.localName = localName;
      this.value = value;
      this.hasValue = (uint) (nodeFlags & XmlNode.XmlNodeFlags.HasValue) > 0U;
      this.canGetAttribute = (uint) (nodeFlags & XmlNode.XmlNodeFlags.CanGetAttribute) > 0U;
      this.canMoveToElement = (uint) (nodeFlags & XmlNode.XmlNodeFlags.CanMoveToElement) > 0U;
      this.IsAtomicValue = (uint) (nodeFlags & XmlNode.XmlNodeFlags.AtomicValue) > 0U;
      this.skipValue = (uint) (nodeFlags & XmlNode.XmlNodeFlags.SkipValue) > 0U;
      this.hasContent = (uint) (nodeFlags & XmlNode.XmlNodeFlags.HasContent) > 0U;
      this.readState = readState;
      this.attributeTextNode = attributeTextNode;
      this.exitScope = nodeType == XmlNodeType.EndElement;
      this.depthDelta = depthDelta;
      this.isEmptyElement = false;
      this.quoteChar = '"';
    }

    public bool IsLocalName(string name)
    {
      return this.LocalName == name;
    }

    public bool IsNamespaceUri(string iNs)
    {
      return false;
    }

    public bool IsLocalNameAndNamespaceUri(string name, string iNs)
    {
      return false;
    }

    public bool IsPrefixAndLocalName(string prefix, string slocalName)
    {
      return false;
    }

    [System.Flags]
    protected enum XmlNodeFlags
    {
      None = 0,
      CanGetAttribute = 1,
      CanMoveToElement = 2,
      HasValue = 4,
      AtomicValue = 8,
      SkipValue = 16, // 0x00000010
      HasContent = 32, // 0x00000020
    }
  }
}
