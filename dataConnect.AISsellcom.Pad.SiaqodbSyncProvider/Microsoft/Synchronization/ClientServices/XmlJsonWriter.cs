// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlJsonWriter
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    internal class XmlJsonWriter : XmlWriter
  {
    private string attributeText;
    private XmlJsonWriter.JsonDataType dataType;
    private int depth;
    private bool endElementBuffer;
    private bool isWritingDataTypeAttribute;
    private bool isWritingServerTypeAttribute;
    private bool isWritingXmlnsAttribute;
    private XmlJsonWriter.NameState nameState;
    private JsonNodeType nodeType;
    private StreamWriter nodeWriter;
    private JsonNodeType[] scopes;
    private string serverTypeValue;
    private WriteState writeState;
    private bool wroteServerTypeAttribute;
    private const char HIGH_SURROGATE_START = '\xD800';
    private const char LOW_SURROGATE_END = '\xDFFF';
    private const char MAX_CHAR = '\xFFFE';

    public XmlJsonWriter(Stream stream)
    {
      if (this.nodeWriter == null)
        this.nodeWriter = new StreamWriter(stream);
      this.InitializeWriter();
    }

    public override XmlWriterSettings Settings
    {
      get
      {
        return (XmlWriterSettings) null;
      }
    }

    public override WriteState WriteState
    {
      get
      {
        if (this.writeState == WriteState.Closed)
          return WriteState.Closed;
        if (this.HasOpenAttribute)
          return WriteState.Attribute;
        switch (this.nodeType)
        {
          case JsonNodeType.None:
            return WriteState.Start;
          case JsonNodeType.Element:
            return WriteState.Element;
          case JsonNodeType.EndElement:
          case JsonNodeType.QuotedText:
          case JsonNodeType.StandaloneText:
            return WriteState.Content;
          default:
            return WriteState.Error;
        }
      }
    }

    public override string XmlLang
    {
      get
      {
        return (string) null;
      }
    }

    public override XmlSpace XmlSpace
    {
      get
      {
        return XmlSpace.None;
      }
    }

    private bool HasOpenAttribute
    {
      get
      {
        if (!this.isWritingDataTypeAttribute && !this.isWritingServerTypeAttribute && !this.IsWritingNameAttribute)
          return this.isWritingXmlnsAttribute;
        return true;
      }
    }

    private bool IsClosed
    {
      get
      {
        return this.WriteState == WriteState.Closed;
      }
    }

    private bool IsWritingCollection
    {
      get
      {
        if (this.depth > 0)
          return this.scopes[this.depth] == JsonNodeType.Collection;
        return false;
      }
    }

    private bool IsWritingNameAttribute
    {
      get
      {
        return (this.nameState & XmlJsonWriter.NameState.IsWritingNameAttribute) == XmlJsonWriter.NameState.IsWritingNameAttribute;
      }
    }

    private bool IsWritingNameWithMapping
    {
      get
      {
        return (this.nameState & XmlJsonWriter.NameState.IsWritingNameWithMapping) == XmlJsonWriter.NameState.IsWritingNameWithMapping;
      }
    }

    private bool WrittenNameWithMapping
    {
      get
      {
        return (this.nameState & XmlJsonWriter.NameState.WrittenNameWithMapping) == XmlJsonWriter.NameState.WrittenNameWithMapping;
      }
    }

    public override void Close()
    {
      if (this.IsClosed)
        return;
      try
      {
        this.WriteEndDocument();
      }
      finally
      {
        try
        {
          this.nodeWriter.Flush();
        }
        finally
        {
          this.writeState = WriteState.Closed;
          this.depth = 0;
        }
      }
    }

    public override void Flush()
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      this.nodeWriter.Flush();
    }

    public override string LookupPrefix(string ns)
    {
      if (ns == null)
        throw new ArgumentException(nameof (ns));
      if (ns == "http://www.w3.org/2000/xmlns/")
        return "xmlns";
      if (ns == "http://www.w3.org/XML/1998/namespace")
        return "xml";
      return ns == string.Empty ? string.Empty : (string) null;
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      if (buffer == null)
        throw new ArgumentException(nameof (buffer));
      if (index < 0)
        throw new ArgumentOutOfRangeException(nameof (index), "ValueMustBeNonNegative");
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), "ValueMustBeNonNegative");
      if (count > buffer.Length - index)
        throw new ArgumentOutOfRangeException(nameof (count), "JsonSizeExceedsRemainingBufferSpace");
      this.StartText();
      this.nodeWriter.Write((object) buffer);
    }

    public override void WriteBinHex(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException(nameof (WriteBinHex));
    }

    public override void WriteCData(string text)
    {
      this.WriteString(text);
    }

    public override void WriteCharEntity(char ch)
    {
      this.WriteString(ch.ToString());
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      if (buffer == null)
        throw new ArgumentException(nameof (buffer));
      if (index < 0)
        throw new ArgumentOutOfRangeException(nameof (index), "ValueMustBeNonNegative");
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), "ValueMustBeNonNegative");
      if (count > buffer.Length - index)
        throw new ArgumentOutOfRangeException(nameof (count), "JsonSizeExceedsRemainingBufferSpace");
      this.WriteString(new string(buffer, index, count));
    }

    public override void WriteComment(string text)
    {
      throw new NotSupportedException(nameof (WriteComment));
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      throw new NotSupportedException(nameof (WriteDocType));
    }

    public override void WriteEndAttribute()
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (!this.HasOpenAttribute)
        throw new XmlException("JsonNoMatchingStartAttribute");
      if (this.isWritingDataTypeAttribute)
      {
        string attributeText = this.attributeText;
        if (!(attributeText == "number"))
        {
          if (!(attributeText == "string"))
          {
            if (!(attributeText == "array"))
            {
              if (!(attributeText == "object"))
              {
                if (!(attributeText == "null"))
                {
                  if (!(attributeText == "boolean"))
                    throw new XmlException("JsonUnexpectedAttributeValue");
                  this.ThrowIfServerTypeWritten("boolean");
                  this.dataType = XmlJsonWriter.JsonDataType.Boolean;
                }
                else
                {
                  this.ThrowIfServerTypeWritten("null");
                  this.dataType = XmlJsonWriter.JsonDataType.Null;
                }
              }
              else
                this.dataType = XmlJsonWriter.JsonDataType.Object;
            }
            else
            {
              this.ThrowIfServerTypeWritten("array");
              this.dataType = XmlJsonWriter.JsonDataType.Array;
            }
          }
          else
          {
            this.ThrowIfServerTypeWritten("string");
            this.dataType = XmlJsonWriter.JsonDataType.String;
          }
        }
        else
        {
          this.ThrowIfServerTypeWritten("number");
          this.dataType = XmlJsonWriter.JsonDataType.Number;
        }
        this.attributeText = (string) null;
        this.isWritingDataTypeAttribute = false;
        if (this.IsWritingNameWithMapping && !this.WrittenNameWithMapping)
          return;
        this.WriteDataTypeServerType();
      }
      else if (this.isWritingServerTypeAttribute)
      {
        this.serverTypeValue = this.attributeText;
        this.attributeText = (string) null;
        this.isWritingServerTypeAttribute = false;
        if (this.IsWritingNameWithMapping && !this.WrittenNameWithMapping || this.dataType != XmlJsonWriter.JsonDataType.Object)
          return;
        this.WriteServerTypeAttribute();
      }
      else if (this.IsWritingNameAttribute)
      {
        this.WriteJsonElementName(this.attributeText);
        this.attributeText = (string) null;
        this.nameState = XmlJsonWriter.NameState.IsWritingNameWithMapping | XmlJsonWriter.NameState.WrittenNameWithMapping;
        this.WriteDataTypeServerType();
      }
      else
      {
        if (!this.isWritingXmlnsAttribute)
          return;
        this.attributeText = (string) null;
        this.isWritingXmlnsAttribute = false;
      }
    }

    public override void WriteEndDocument()
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (this.nodeType == JsonNodeType.None)
        return;
      while (this.depth > 0)
        this.WriteEndElement();
    }

    public override void WriteEndElement()
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (this.depth == 0)
        throw new XmlException("JsonEndElementNoOpenNodes");
      if (this.HasOpenAttribute)
        throw new XmlException("JsonOpenAttributeMustBeClosedFirst");
      this.endElementBuffer = false;
      JsonNodeType jsonNodeType = this.ExitScope();
      if (jsonNodeType == JsonNodeType.Collection)
      {
        this.nodeWriter.Write("]");
        jsonNodeType = this.ExitScope();
      }
      else if (this.nodeType == JsonNodeType.QuotedText)
        this.WriteJsonQuote();
      else if (this.nodeType == JsonNodeType.Element)
      {
        if (this.dataType == XmlJsonWriter.JsonDataType.None && this.serverTypeValue != null)
          throw new XmlException("JsonMustSpecifyDataType");
        if (this.IsWritingNameWithMapping && !this.WrittenNameWithMapping)
          throw new XmlException("JsonMustSpecifyDataType");
        if (this.dataType == XmlJsonWriter.JsonDataType.None || this.dataType == XmlJsonWriter.JsonDataType.String)
        {
          this.nodeWriter.Write("\"");
          this.nodeWriter.Write("\"");
        }
      }
      if ((uint) this.depth > 0U)
      {
        if (jsonNodeType == JsonNodeType.Element)
          this.endElementBuffer = true;
        else if (jsonNodeType == JsonNodeType.Object)
        {
          this.nodeWriter.Write("}");
          if (this.depth > 0 && this.scopes[this.depth] == JsonNodeType.Element)
          {
            int num = (int) this.ExitScope();
            this.endElementBuffer = true;
          }
        }
      }
      this.dataType = XmlJsonWriter.JsonDataType.None;
      this.nodeType = JsonNodeType.EndElement;
      this.nameState = XmlJsonWriter.NameState.None;
      this.wroteServerTypeAttribute = false;
    }

    public override void WriteEntityRef(string name)
    {
      throw new NotSupportedException("JsonMethodNotSupported");
    }

    public override void WriteFullEndElement()
    {
      this.WriteEndElement();
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (!name.Equals("xml", StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException("JsonXmlProcessingInstructionNotSupported");
      if ((uint) this.WriteState > 0U)
        throw new XmlException("JsonXmlInvalidDeclaration");
    }

    public override void WriteQualifiedName(string localName, string ns)
    {
      if (localName == null)
        throw new ArgumentException(nameof (localName));
      if (localName.Length == 0)
        throw new ArgumentException(nameof (localName), "JsonInvalidLocalNameEmpty");
      if (ns == null)
        ns = string.Empty;
      base.WriteQualifiedName(localName, ns);
    }

    public override void WriteRaw(string data)
    {
      this.nodeWriter.Write(data);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      throw new NotSupportedException("WriteRaw (char[] buffer, int index, int count)");
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (!string.IsNullOrEmpty(prefix))
      {
        if (!this.IsWritingNameWithMapping || !(prefix == "xmlns"))
          throw new ArgumentException(nameof (prefix), "JsonPrefixMustBeNullOrEmpty");
        if (ns != null && ns != "http://www.w3.org/2000/xmlns/")
          throw new ArgumentException("XmlPrefixBoundToNamespace");
      }
      else if (this.IsWritingNameWithMapping && ns == "http://www.w3.org/2000/xmlns/" && localName != "xmlns")
        prefix = "xmlns";
      if (!string.IsNullOrEmpty(ns))
      {
        if (!this.IsWritingNameWithMapping || !(ns == "http://www.w3.org/2000/xmlns/"))
          throw new ArgumentException(nameof (ns), "JsonNamespaceMustBeEmpty");
        prefix = "xmlns";
      }
      if (localName == null)
        throw new ArgumentException(nameof (localName));
      if (localName.Length == 0)
        throw new ArgumentException(nameof (localName), "JsonInvalidLocalNameEmpty");
      if (this.nodeType != JsonNodeType.Element && !this.wroteServerTypeAttribute)
        throw new XmlException("JsonAttributeMustHaveElement");
      if (this.HasOpenAttribute)
        throw new XmlException("JsonOpenAttributeMustBeClosedFirst");
      if (prefix == "xmlns")
      {
        this.isWritingXmlnsAttribute = true;
      }
      else
      {
        string str = localName;
        if (!(str == "type"))
        {
          if (!(str == "__type"))
          {
            if (!(str == "item"))
              throw new ArgumentException(nameof (localName), "JsonUnexpectedAttributeLocalName");
            if (this.WrittenNameWithMapping)
              throw new XmlException("JsonAttributeAlreadyWritten");
            if (!this.IsWritingNameWithMapping)
              throw new XmlException("JsonEndElementNoOpenNodes");
            this.nameState |= XmlJsonWriter.NameState.IsWritingNameAttribute;
          }
          else
          {
            if (this.serverTypeValue != null)
              throw new XmlException("JsonAttributeAlreadyWritten");
            if (this.dataType != XmlJsonWriter.JsonDataType.None && this.dataType != XmlJsonWriter.JsonDataType.Object)
              throw new XmlException("JsonServerTypeSpecifiedForInvalidDataType");
            this.isWritingServerTypeAttribute = true;
          }
        }
        else
        {
          if ((uint) this.dataType > 0U)
            throw new XmlException("JsonAttributeAlreadyWritten");
          this.isWritingDataTypeAttribute = true;
        }
      }
    }

    public override void WriteStartDocument(bool standalone)
    {
      this.WriteStartDocument();
    }

    public override void WriteStartDocument()
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (this.WriteState != WriteState.Start)
        throw new XmlException("JsonInvalidWriteState");
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      if (localName == null)
        throw new ArgumentException(nameof (localName));
      if (localName.Length == 0)
        throw new ArgumentException(nameof (localName), "JsonInvalidLocalNameEmpty");
      if (!string.IsNullOrEmpty(prefix) && (string.IsNullOrEmpty(ns) || !this.TrySetWritingNameWithMapping(localName, ns)))
        throw new ArgumentException(nameof (prefix), "JsonPrefixMustBeNullOrEmpty");
      if (!string.IsNullOrEmpty(ns) && !this.TrySetWritingNameWithMapping(localName, ns))
        throw new ArgumentException(nameof (ns), "JsonNamespaceMustBeEmpty");
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (this.HasOpenAttribute)
        throw new XmlException("JsonOpenAttributeMustBeClosedFirst");
      if (this.nodeType != JsonNodeType.None && this.depth == 0)
        throw new XmlException("JsonMultipleRootElementsNotAllowedOnWriter");
      switch (this.nodeType)
      {
        case JsonNodeType.None:
          if (!localName.Equals("root"))
            throw new XmlException("JsonInvalidRootElementName");
          this.EnterScope(JsonNodeType.Element);
          break;
        case JsonNodeType.Element:
          if (this.dataType != XmlJsonWriter.JsonDataType.Array && this.dataType != XmlJsonWriter.JsonDataType.Object)
            throw new XmlException("JsonNodeTypeArrayOrObjectNotSpecified");
          if (!this.IsWritingCollection)
          {
            if (this.nameState != XmlJsonWriter.NameState.IsWritingNameWithMapping)
              this.WriteJsonElementName(localName);
          }
          else if (!localName.Equals("item"))
            throw new XmlException("JsonInvalidItemNameForArrayElement");
          this.EnterScope(JsonNodeType.Element);
          break;
        case JsonNodeType.EndElement:
          if (this.endElementBuffer)
            this.nodeWriter.Write(",");
          if (!this.IsWritingCollection)
          {
            if (this.nameState != XmlJsonWriter.NameState.IsWritingNameWithMapping)
              this.WriteJsonElementName(localName);
          }
          else if (!localName.Equals("item"))
            throw new XmlException("JsonInvalidItemNameForArrayElement");
          this.EnterScope(JsonNodeType.Element);
          break;
        default:
          throw new XmlException("JsonInvalidStartElementCall");
      }
      this.isWritingDataTypeAttribute = false;
      this.isWritingServerTypeAttribute = false;
      this.isWritingXmlnsAttribute = false;
      this.wroteServerTypeAttribute = false;
      this.serverTypeValue = (string) null;
      this.dataType = XmlJsonWriter.JsonDataType.None;
      this.nodeType = JsonNodeType.Element;
    }

    public override void WriteString(string text)
    {
      if (this.HasOpenAttribute && text != null)
      {
        this.attributeText += text;
      }
      else
      {
        this.StartText();
        this.WriteEscapedJsonString(text);
      }
    }

    private void WriteEscapedJsonString(string str)
    {
      char[] chars = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(str));
      int index1 = 0;
      int index2;
      for (index2 = 0; index2 < chars.Length; ++index2)
      {
        char ch = str[index2];
        if (ch <= '/')
        {
          if (ch == '/' || ch == '"')
          {
            this.nodeWriter.Write(chars, index1, index2 - index1);
            this.nodeWriter.Write('\\');
            this.nodeWriter.Write(ch);
            index1 = index2 + 1;
          }
          else if (ch < ' ')
          {
            this.nodeWriter.Write(chars, index1, index2 - index1);
            this.nodeWriter.Write('\\');
            this.nodeWriter.Write('u');
            this.nodeWriter.Write(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0:x4}", (object) ch));
            index1 = index2 + 1;
          }
        }
        else if (ch == '\\')
        {
          this.nodeWriter.Write(chars, index1, index2 - index1);
          this.nodeWriter.Write('\\');
          this.nodeWriter.Write(ch);
          index1 = index2 + 1;
        }
        else if (ch >= '\xD800' && (ch <= '\xDFFF' || ch >= '\xFFFE'))
        {
          this.nodeWriter.Write(chars, index1, index2 - index1);
          this.nodeWriter.Write('\\');
          this.nodeWriter.Write('u');
          this.nodeWriter.Write(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0:x4}", (object) ch));
          index1 = index2 + 1;
        }
      }
      if (index1 >= index2)
        return;
      this.nodeWriter.Write(chars, index1, index2 - index1);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      this.WriteString(highChar.ToString() + lowChar.ToString());
    }

    public override void WriteValue(bool value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    public override void WriteValue(Decimal value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    public override void WriteValue(double value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    public override void WriteValue(float value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    public override void WriteValue(int value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    public override void WriteValue(long value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    public void WriteValue(Guid value)
    {
      this.StartText();
      this.nodeWriter.Write((object) value);
    }

    public new void WriteValue(DateTime value)
    {
      this.StartText();
      this.nodeWriter.Write((object) value);
    }

    public override void WriteValue(string value)
    {
      this.WriteString(value);
    }

    public void WriteValue(TimeSpan value)
    {
      this.StartText();
      this.nodeWriter.Write((object) value);
    }

    public void WriteValue(UniqueId value)
    {
      if (value == (UniqueId) null)
        throw new ArgumentException(nameof (value));
      this.StartText();
      this.nodeWriter.Write((object) value);
    }

    public override void WriteValue(object value)
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (value == null)
        throw new ArgumentException(nameof (value));
      if (value is Array)
        this.WriteValue((Array) value);
      else
        this.WritePrimitiveValue(value);
    }

    public override void WriteWhitespace(string ws)
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (ws == null)
        throw new ArgumentException(nameof (ws));
      this.nodeWriter.Write(ws);
    }

    internal static bool CharacterNeedsEscaping(char ch)
    {
      if (ch == '/' || ch == '"' || ch < ' ' || ch == '\\')
        return true;
      if (ch < '\xD800')
        return false;
      if (ch > '\xDFFF')
        return ch >= '\xFFFE';
      return true;
    }

    private static void ThrowClosed()
    {
      throw new InvalidOperationException("JsonWriterClosed");
    }

    private void CheckText(JsonNodeType nextNodeType)
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (this.depth == 0)
        throw new InvalidOperationException("XmlIllegalOutsideRoot");
      if (nextNodeType == JsonNodeType.StandaloneText && this.nodeType == JsonNodeType.QuotedText)
        throw new XmlException("JsonCannotWriteStandaloneTextAfterQuotedText");
    }

    private void EnterScope(JsonNodeType currentNodeType)
    {
      ++this.depth;
      if (this.scopes == null)
        this.scopes = new JsonNodeType[4];
      else if (this.scopes.Length == this.depth)
      {
        JsonNodeType[] jsonNodeTypeArray = new JsonNodeType[this.depth * 2];
        Array.Copy((Array) this.scopes, (Array) jsonNodeTypeArray, this.depth);
        this.scopes = jsonNodeTypeArray;
      }
      this.scopes[this.depth] = currentNodeType;
    }

    private JsonNodeType ExitScope()
    {
      JsonNodeType scope = this.scopes[this.depth];
      this.scopes[this.depth] = JsonNodeType.None;
      --this.depth;
      return scope;
    }

    private void InitializeWriter()
    {
      this.nodeType = JsonNodeType.None;
      this.dataType = XmlJsonWriter.JsonDataType.None;
      this.isWritingDataTypeAttribute = false;
      this.wroteServerTypeAttribute = false;
      this.isWritingServerTypeAttribute = false;
      this.serverTypeValue = (string) null;
      this.attributeText = (string) null;
      this.depth = 0;
      if (this.scopes != null && this.scopes.Length > 25)
        this.scopes = (JsonNodeType[]) null;
      this.writeState = WriteState.Start;
      this.endElementBuffer = false;
    }

    private void StartText()
    {
      if (this.HasOpenAttribute)
        throw new InvalidOperationException("JsonMustUseWriteStringForWritingAttributeValues");
      if (this.dataType == XmlJsonWriter.JsonDataType.None && this.serverTypeValue != null)
        throw new XmlException("JsonMustSpecifyDataType");
      if (this.IsWritingNameWithMapping && !this.WrittenNameWithMapping)
        throw new XmlException("JsonMustSpecifyDataType");
      switch (this.dataType)
      {
        case XmlJsonWriter.JsonDataType.None:
        case XmlJsonWriter.JsonDataType.String:
          this.CheckText(JsonNodeType.QuotedText);
          if (this.nodeType != JsonNodeType.QuotedText)
            this.WriteJsonQuote();
          this.nodeType = JsonNodeType.QuotedText;
          break;
        case XmlJsonWriter.JsonDataType.Boolean:
        case XmlJsonWriter.JsonDataType.Number:
          this.CheckText(JsonNodeType.StandaloneText);
          this.nodeType = JsonNodeType.StandaloneText;
          break;
      }
    }

    private void ThrowIfServerTypeWritten(string dataTypeSpecified)
    {
      if (this.serverTypeValue != null)
        throw new XmlException("JsonInvalidDataTypeSpecifiedForServerType");
    }

    private bool TrySetWritingNameWithMapping(string localName, string ns)
    {
      if (!localName.Equals("item") || !ns.Equals("item"))
        return false;
      this.nameState = XmlJsonWriter.NameState.IsWritingNameWithMapping;
      return true;
    }

    private void WriteDataTypeServerType()
    {
      if (this.dataType == XmlJsonWriter.JsonDataType.None)
        return;
      switch (this.dataType)
      {
        case XmlJsonWriter.JsonDataType.Null:
          this.nodeWriter.Write("null");
          break;
        case XmlJsonWriter.JsonDataType.Object:
          this.EnterScope(JsonNodeType.Object);
          this.nodeWriter.Write("{");
          break;
        case XmlJsonWriter.JsonDataType.Array:
          this.EnterScope(JsonNodeType.Collection);
          this.nodeWriter.Write("[");
          break;
      }
      if (this.serverTypeValue == null)
        return;
      this.WriteServerTypeAttribute();
    }

    private void WriteJsonElementName(string localName)
    {
      this.WriteJsonQuote();
      this.nodeWriter.Write(localName);
      this.WriteJsonQuote();
      this.nodeWriter.Write(":");
    }

    private void WriteJsonQuote()
    {
      this.nodeWriter.Write("\"");
    }

    private void WritePrimitiveValue(object value)
    {
      if (this.IsClosed)
        XmlJsonWriter.ThrowClosed();
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (value is ulong)
        this.WriteValue((ulong) value);
      else if (value is string)
        this.WriteValue((string) value);
      else if (value is int)
        this.WriteValue((int) value);
      else if (value is long)
        this.WriteValue((long) value);
      else if (value is bool)
        this.WriteValue((bool) value);
      else if (value is double)
        this.WriteValue((double) value);
      else if (value is DateTime)
        base.WriteValue((DateTime) value);
      else if (value is float)
        this.WriteValue((float) value);
      else if (value is Decimal)
        this.WriteValue((Decimal) value);
      else if (value is XmlDictionaryString)
        base.WriteValue(value);
      else if (value is UniqueId)
        base.WriteValue(value);
      else if (value is Guid)
        base.WriteValue((object) (Guid) value);
      else if (value is TimeSpan)
      {
        base.WriteValue((object) (TimeSpan) value);
      }
      else
      {
        if (value.GetType().IsArray)
          throw new ArgumentException("JsonNestedArraysNotSupported");
        base.WriteValue(value);
      }
    }

    private void WriteServerTypeAttribute()
    {
      string serverTypeValue = this.serverTypeValue;
      XmlJsonWriter.JsonDataType dataType = this.dataType;
      XmlJsonWriter.NameState nameState = this.nameState;
      this.WriteStartElement("__type");
      this.WriteValue(serverTypeValue);
      this.WriteEndElement();
      this.dataType = dataType;
      this.nameState = nameState;
      this.wroteServerTypeAttribute = true;
    }

    private void WriteValue(ulong value)
    {
      this.StartText();
      this.nodeWriter.Write(value);
    }

    private void WriteValue(Array array)
    {
      XmlJsonWriter.JsonDataType dataType = this.dataType;
      this.dataType = XmlJsonWriter.JsonDataType.String;
      this.StartText();
      for (int index = 0; index < array.Length; ++index)
      {
        if ((uint) index > 0U)
          this.nodeWriter.Write(" ");
        this.WritePrimitiveValue(array.GetValue(index));
      }
      this.dataType = dataType;
    }

    private enum JsonDataType
    {
      None,
      Null,
      Boolean,
      Number,
      String,
      Object,
      Array,
    }

    [System.Flags]
    private enum NameState
    {
      None = 0,
      IsWritingNameWithMapping = 1,
      IsWritingNameAttribute = 2,
      WrittenNameWithMapping = 4,
    }
  }
}
