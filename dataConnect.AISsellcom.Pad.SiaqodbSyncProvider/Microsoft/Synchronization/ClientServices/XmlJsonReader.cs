// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlJsonReader
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlJsonReader : XmlReader {
        private static XmlInitialNode initialNode = new XmlInitialNode(XmlBufferReader.Empty);
        private static XmlEndOfFileNode endOfFileNode = new XmlEndOfFileNode(XmlBufferReader.Empty);
        private static XmlClosedNode closedNode = new XmlClosedNode(XmlBufferReader.Empty);
        private Dictionary<int, XmlElementNode> elementNodes = new Dictionary<int, XmlElementNode>();
        private Dictionary<int, XmlAttributeNode> attributeNodes = new Dictionary<int, XmlAttributeNode>();
        private readonly Dictionary<int, JsonNodeType> scopes = new Dictionary<int, JsonNodeType>();
        private JsonComplexTextMode complexTextMode = JsonComplexTextMode.None;
        private const string Xmlns = "xmlns";
        private const string Xml = "xml";
        private const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        private const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        private const int BufferAllocation = 128;
        private const int MaxTextChunk = 2048;
        private XmlAtomicTextNode atomicTextNode;
        private XmlComplexTextNode complexTextNode;
        private XmlWhitespaceTextNode whitespaceTextNode;
        private XmlCDataNode cdataNode;
        private XmlCommentNode commentNode;
        private XmlElementNode rootElementNode;
        private int depth;
        private int attributeCount;
        private int attributeStart;
        private XmlDictionaryReaderQuotas quotas;
        private XmlNameTable nameTable;
        private int attributeIndex;
        private string localName;
        private string value;
        private bool rootElement;
        private bool readingElement;
        private byte[] charactersToSkipOnNextRead;
        private bool expectingFirstElementInNonPrimitiveChild;
        private int maxBytesPerRead;
        private int scopeDepth;

        protected XmlBufferReader BufferReader { get; private set; }

        protected XmlNode Node { get; private set; }

        protected XmlElementNode ElementNode {
            get {
                if (this.depth == 0)
                    return this.rootElementNode;
                return this.elementNodes[this.depth];
            }
        }

        public override bool CanReadBinaryContent {
            get {
                return true;
            }
        }

        public override bool CanReadValueChunk {
            get {
                return true;
            }
        }

        public override string BaseURI {
            get {
                return string.Empty;
            }
        }

        public override bool HasValue {
            get {
                return this.Node.HasValue;
            }
        }

        public override bool IsDefault {
            get {
                return false;
            }
        }

        public override string this[int index] {
            get {
                return this.GetAttribute(index);
            }
        }

        public override string this[string name] {
            get {
                return this.GetAttribute(name);
            }
        }

        public override string this[string iLocalName, string namespaceUri] {
            get {
                return this.GetAttribute(iLocalName, namespaceUri);
            }
        }

        public override int AttributeCount {
            get {
                return this.Node.CanGetAttribute ? this.attributeCount : 0;
            }
        }

        public override sealed int Depth {
            get {
                return this.depth + this.Node.DepthDelta;
            }
        }

        public override bool EOF {
            get {
                return this.Node.ReadState == ReadState.EndOfFile;
            }
        }

        public override sealed bool IsEmptyElement {
            get {
                return this.Node.IsEmptyElement;
            }
        }

        public override string LocalName {
            get {
                if (this.localName == null)
                    this.localName = this.Node.LocalName.GetString(this.NameTable);
                return this.localName;
            }
        }

        public override string NamespaceURI {
            get {
                return "";
            }
        }

        public override XmlNameTable NameTable {
            get {
                if (this.nameTable == null) {
                    this.nameTable = (XmlNameTable)new System.Xml.NameTable();
                    this.nameTable.Add("xml");
                    this.nameTable.Add("xmlns");
                    this.nameTable.Add("http://www.w3.org/2000/xmlns/");
                    this.nameTable.Add("http://www.w3.org/XML/1998/namespace");
                }
                return this.nameTable;
            }
        }

        public override sealed XmlNodeType NodeType {
            get {
                return this.Node.NodeType;
            }
        }

        public override string Prefix {
            get {
                return string.Empty;
            }
        }

        public override ReadState ReadState {
            get {
                return this.Node.ReadState;
            }
        }

        private bool IsAttributeValue {
            get {
                if (this.Node.NodeType != XmlNodeType.Attribute)
                    return this.Node is XmlAttributeTextNode;
                return true;
            }
        }

        private bool IsReadingCollection {
            get {
                if (this.scopeDepth > 0)
                    return this.scopes[this.scopeDepth] == JsonNodeType.Collection;
                return false;
            }
        }

        private bool IsReadingComplexText {
            get {
                if (!this.Node.IsAtomicValue)
                    return this.Node.NodeType == XmlNodeType.Text;
                return false;
            }
        }

        public override string Value {
            get {
                return this.value ?? (this.value = this.Node.ValueAsString);
            }
        }

        public override Type ValueType {
            get {
                if (this.value == null) {
                    Type type = this.Node.Value.ToType();
                    if (this.Node.IsAtomicValue || type == typeof(byte[]))
                        return type;
                }
                return typeof(string);
            }
        }

        public override string XmlLang {
            get {
                return string.Empty;
            }
        }

        public override XmlSpace XmlSpace {
            get {
                return XmlSpace.None;
            }
        }

        internal XmlJsonReader() {
            this.BufferReader = new XmlBufferReader();
            this.rootElementNode = new XmlElementNode(this.BufferReader);
            this.atomicTextNode = new XmlAtomicTextNode(this.BufferReader);
            this.Node = (XmlNode)XmlJsonReader.closedNode;
        }

        public XmlJsonReader(Stream stream, XmlDictionaryReaderQuotas quotas)
          : this() {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            this.MoveToInitial(quotas);
            this.BufferReader.SetBuffer(stream);
            this.ResetState();
        }

        private void ResetState() {
            this.complexTextMode = JsonComplexTextMode.None;
            this.expectingFirstElementInNonPrimitiveChild = false;
            this.charactersToSkipOnNextRead = new byte[2];
            this.scopeDepth = 0;
            this.scopes.Clear();
        }

        protected void MoveToNode(XmlNode xmlNode) {
            this.Node = xmlNode;
            this.localName = (string)null;
            this.value = (string)null;
        }

        protected void MoveToInitial(XmlDictionaryReaderQuotas xmlQuotas) {
            if (xmlQuotas == null)
                throw new ArgumentNullException("quotas");
            this.quotas = xmlQuotas;
            this.maxBytesPerRead = xmlQuotas.MaxBytesPerRead;
            this.depth = 0;
            this.attributeCount = 0;
            this.attributeStart = -1;
            this.attributeIndex = -1;
            this.rootElement = false;
            this.readingElement = false;
            this.MoveToNode((XmlNode)XmlJsonReader.initialNode);
        }

        protected XmlCommentNode MoveToComment() {
            if (this.commentNode == null)
                this.commentNode = new XmlCommentNode(this.BufferReader);
            this.MoveToNode((XmlNode)this.commentNode);
            return this.commentNode;
        }

        protected XmlCDataNode MoveToCData() {
            if (this.cdataNode == null)
                this.cdataNode = new XmlCDataNode(this.BufferReader);
            this.MoveToNode((XmlNode)this.cdataNode);
            return this.cdataNode;
        }

        public void MoveToStartElement() {
            if (!base.IsStartElement())
                throw new XmlException("StartElementExpected(this)");
        }

        public void MoveToStartElement(string name) {
            if (!base.IsStartElement(name))
                throw new XmlException("StartElementExpected(this)");
        }

        protected XmlAtomicTextNode MoveToAtomicText() {
            XmlAtomicTextNode atomicTextNode = this.atomicTextNode;
            this.MoveToNode((XmlNode)atomicTextNode);
            return atomicTextNode;
        }

        protected XmlComplexTextNode MoveToComplexText() {
            if (this.complexTextNode == null)
                this.complexTextNode = new XmlComplexTextNode(this.BufferReader);
            this.MoveToNode((XmlNode)this.complexTextNode);
            return this.complexTextNode;
        }

        protected XmlTextNode MoveToWhitespaceText() {
            if (this.whitespaceTextNode == null)
                this.whitespaceTextNode = new XmlWhitespaceTextNode(this.BufferReader);
            this.whitespaceTextNode.NodeType = XmlNodeType.Whitespace;
            this.MoveToNode((XmlNode)this.whitespaceTextNode);
            return (XmlTextNode)this.whitespaceTextNode;
        }

        protected void MoveToEndElement() {
            int num = (int)this.ExitJsonScope();
            if (this.depth == 0)
                throw new XmlException("ThrowInvalidBinaryFormat");
            this.MoveToNode((XmlNode)this.elementNodes[this.depth].EndElement);
        }

        protected void MoveToEndOfFile() {
            if ((uint)this.depth > 0U)
                throw new XmlException("ThrowUnexpectedEndOfFile");
            this.MoveToNode((XmlNode)XmlJsonReader.endOfFileNode);
        }

        protected XmlElementNode EnterScope() {
            if (this.depth == 0) {
                if (this.rootElement)
                    throw new XmlException("ThrowMultipleRootElements");
                this.rootElement = true;
            }
            ++this.depth;
            if (this.depth > this.quotas.MaxDepth)
                throw new XmlException("ThrowMaxDepthExceeded");
            XmlElementNode xmlElementNode;
            if (!this.elementNodes.TryGetValue(this.depth, out xmlElementNode)) {
                xmlElementNode = new XmlElementNode(this.BufferReader);
                this.elementNodes[this.depth] = xmlElementNode;
            }
            this.attributeCount = 0;
            this.attributeStart = -1;
            this.attributeIndex = -1;
            this.MoveToNode((XmlNode)xmlElementNode);
            return xmlElementNode;
        }

        protected void ExitScope() {
            if (this.depth == 0)
                throw new XmlException("ThrowUnexpectedEndElement");
            --this.depth;
        }

        private XmlAttributeNode AddAttribute(bool isAtomicValue) {
            XmlAttributeNode xmlAttributeNode;
            if (!this.attributeNodes.TryGetValue(this.attributeCount, out xmlAttributeNode)) {
                xmlAttributeNode = new XmlAttributeNode(this.BufferReader);
                this.attributeNodes[this.attributeCount] = xmlAttributeNode;
            }
            xmlAttributeNode.IsAtomicValue = isAtomicValue;
            xmlAttributeNode.AttributeText.IsAtomicValue = isAtomicValue;
            ++this.attributeCount;
            return xmlAttributeNode;
        }

        protected XmlAttributeNode AddAttribute() {
            return this.AddAttribute(true);
        }

        protected XmlAttributeNode AddXmlAttribute() {
            return this.AddAttribute(true);
        }

        public override void Close() {
            this.MoveToNode((XmlNode)XmlJsonReader.closedNode);
            this.nameTable = (XmlNameTable)null;
            this.attributeNodes.Clear();
            this.attributeNodes = (Dictionary<int, XmlAttributeNode>)null;
            this.elementNodes.Clear();
            this.elementNodes = (Dictionary<int, XmlElementNode>)null;
            this.BufferReader.Close();
            this.ResetState();
        }

        private XmlAttributeNode GetAttributeNode(int index) {
            if (!this.Node.CanGetAttribute)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index < this.attributeCount)
                return this.attributeNodes[index];
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        private XmlAttributeNode GetAttributeNode(string name) {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (!this.Node.CanGetAttribute)
                return (XmlAttributeNode)null;
            int length = name.IndexOf(':');
            string prefix;
            string slocalName;
            if (length == -1) {
                if (name == "xmlns") {
                    prefix = "xmlns";
                    slocalName = string.Empty;
                } else {
                    prefix = string.Empty;
                    slocalName = name;
                }
            } else {
                prefix = name.Substring(0, length);
                slocalName = name.Substring(length + 1);
            }
            Dictionary<int, XmlAttributeNode> attributeNodes = this.attributeNodes;
            int index1 = this.attributeStart;
            for (int index2 = 0; index2 < this.attributeCount; ++index2) {
                if (++index1 >= this.attributeCount)
                    index1 = 0;
                XmlAttributeNode xmlAttributeNode = attributeNodes[index1];
                if (xmlAttributeNode.IsPrefixAndLocalName(prefix, slocalName)) {
                    this.attributeStart = index1;
                    return xmlAttributeNode;
                }
            }
            return (XmlAttributeNode)null;
        }

        private XmlAttributeNode GetAttributeNode(
          string iLocalName,
          string namespaceUri) {
            if (iLocalName == null)
                throw new ArgumentNullException("localName");
            if (namespaceUri == null)
                namespaceUri = string.Empty;
            if (!this.Node.CanGetAttribute)
                return (XmlAttributeNode)null;
            Dictionary<int, XmlAttributeNode> attributeNodes = this.attributeNodes;
            int index1 = this.attributeStart;
            for (int index2 = 0; index2 < this.attributeCount; ++index2) {
                ++index1;
                if (index1 >= this.attributeCount)
                    index1 = 0;
                XmlAttributeNode xmlAttributeNode = attributeNodes[index1];
                if (xmlAttributeNode.IsLocalNameAndNamespaceUri(iLocalName, namespaceUri)) {
                    this.attributeStart = index1;
                    return xmlAttributeNode;
                }
            }
            return (XmlAttributeNode)null;
        }

        public override string GetAttribute(int index) {
            return this.UnescapeJsonString(this.GetAttributeNode(index).ValueAsString);
        }

        public override string GetAttribute(string name) {
            XmlAttributeNode attributeNode = this.GetAttributeNode(name);
            if (attributeNode == null)
                return (string)null;
            if (name != "type")
                return this.UnescapeJsonString(attributeNode.ValueAsString);
            return attributeNode.ValueAsString;
        }

        public override string GetAttribute(string iLocalName, string namespaceUri) {
            XmlAttributeNode attributeNode = this.GetAttributeNode(iLocalName, namespaceUri);
            if (attributeNode == null)
                return (string)null;
            if (iLocalName != "type")
                return this.UnescapeJsonString(attributeNode.ValueAsString);
            return attributeNode.ValueAsString;
        }

        public override string LookupNamespace(string iPrefix) {
            return "http://www.w3.org/2000/xmlns/";
        }

        private static int BreakText(byte[] buffer, int offset, int length) {
            if (length > 0 && ((int)buffer[offset + length - 1] & 128) == 128) {
                int num1 = length;
                do {
                    --length;
                }
                while (length > 0 && ((int)buffer[offset + length] & 192) != 192);
                if (length == 0)
                    return num1;
                byte num2 = (byte)((uint)buffer[offset + length] << 2);
                int num3 = 2;
                while (((int)num2 & 128) == 128) {
                    num2 <<= 1;
                    ++num3;
                    if (num3 > 4)
                        return num1;
                }
                if (length + num3 == num1 || length == 0)
                    return num1;
            }
            return length;
        }

        private static int ComputeNumericalTextLength(byte[] buffer, int offset, int offsetMax) {
            int num1 = offset;
            for (; offset < offsetMax; ++offset) {
                byte ch = buffer[offset];
                int num2;
                switch (ch) {
                    case 44:
                    case 93:
                    case 125:
                        num2 = 1;
                        break;
                    default:
                        num2 = XmlJsonReader.IsWhitespace(ch) ? 1 : 0;
                        break;
                }
                if (num2 != 0)
                    return offset - num1;
            }
            return offset - num1;
        }

        private static int ComputeQuotedTextLengthUntilEndQuote(
          byte[] iBuffer,
          int iOffset,
          int iOffsetMax,
          out bool oEscaped) {
            oEscaped = false;
            int index;
            for (index = iOffset; index < iOffsetMax; ++index) {
                byte num = iBuffer[index];
                if (num < (byte)32)
                    throw new FormatException("InvalidCharacterEncountered");
                if (num == (byte)92 || num == (byte)239) {
                    oEscaped = true;
                    break;
                }
                if (num == (byte)34)
                    break;
            }
            return index - iOffset;
        }

        public override void MoveToAttribute(int index) {
            this.MoveToNode((XmlNode)this.GetAttributeNode(index));
        }

        public override bool MoveToAttribute(string name) {
            XmlNode attributeNode = (XmlNode)this.GetAttributeNode(name);
            if (attributeNode == null)
                return false;
            this.MoveToNode(attributeNode);
            return true;
        }

        public override bool MoveToAttribute(string slocalName, string namespaceUri) {
            XmlNode attributeNode = (XmlNode)this.GetAttributeNode(slocalName, namespaceUri);
            if (attributeNode == null)
                return false;
            this.MoveToNode(attributeNode);
            return true;
        }

        public override bool MoveToElement() {
            if (!this.Node.CanMoveToElement)
                return false;
            this.MoveToNode((XmlNode)this.elementNodes[this.depth]);
            this.attributeIndex = -1;
            return true;
        }

        public override XmlNodeType MoveToContent() {
            do {
                if (this.Node.HasContent) {
                    if (this.Node.NodeType == XmlNodeType.Text || this.Node.NodeType == XmlNodeType.CDATA) {
                        if (this.value == null) {
                            if (!this.Node.Value.IsWhitespace())
                                break;
                        } else if (!XmlConverter.IsWhitespace(this.value))
                            break;
                    } else
                        break;
                } else if (this.Node.NodeType == XmlNodeType.Attribute) {
                    this.MoveToElement();
                    break;
                }
            }
            while (this.Read());
            return this.Node.NodeType;
        }

        public override bool MoveToFirstAttribute() {
            if (!this.Node.CanGetAttribute || this.attributeCount == 0)
                return false;
            this.MoveToNode((XmlNode)this.GetAttributeNode(0));
            this.attributeIndex = 0;
            return true;
        }

        public override bool MoveToNextAttribute() {
            if (!this.Node.CanGetAttribute)
                return false;
            int index = this.attributeIndex + 1;
            if (index >= this.attributeCount)
                return false;
            this.MoveToNode((XmlNode)this.GetAttributeNode(index));
            this.attributeIndex = index;
            return true;
        }

        public override sealed bool IsStartElement() {
            switch (this.Node.NodeType) {
                case XmlNodeType.None:
                    this.Read();
                    if (this.Node.NodeType == XmlNodeType.Element)
                        return true;
                    break;
                case XmlNodeType.Element:
                    return true;
                case XmlNodeType.EndElement:
                    return false;
            }
            return this.MoveToContent() == XmlNodeType.Element;
        }

        public override bool IsStartElement(string name) {
            if (name == null)
                return false;
            int num = name.IndexOf(':');
            string str = num == -1 ? name : name.Substring(num + 1);
            if (this.Node.NodeType == XmlNodeType.Element || this.IsStartElement())
                return this.Node.LocalName == str;
            return false;
        }

        public override bool IsStartElement(string slocalName, string namespaceUri) {
            if (slocalName == null || namespaceUri == null || this.Node.NodeType != XmlNodeType.Element && !this.IsStartElement() || !(this.Node.LocalName == slocalName))
                return false;
            return this.Node.IsNamespaceUri(namespaceUri);
        }

        private void BufferElement() {
            int offset = this.BufferReader.Offset;
            bool flag = false;
            byte num1 = 0;
            while (!flag) {
                int outOffset;
                int outOffsetMax;
                byte[] buffer = this.BufferReader.GetBuffer(128, out outOffset, out outOffsetMax);
                if (outOffset + 128 == outOffsetMax) {
                    for (int index = outOffset; index < outOffsetMax && !flag; ++index) {
                        byte num2 = buffer[index];
                        if (num2 == (byte)92) {
                            ++index;
                            if (index >= outOffsetMax)
                                break;
                        } else if (num1 == (byte)0) {
                            if (num2 == (byte)39 || num2 == (byte)34)
                                num1 = num2;
                            if (num2 == (byte)58)
                                flag = true;
                        } else if ((int)num2 == (int)num1)
                            num1 = (byte)0;
                    }
                    this.BufferReader.Advance(128);
                } else
                    break;
            }
            this.BufferReader.Offset = offset;
        }

        private void EnterJsonScope(JsonNodeType currentNodeType) {
            ++this.scopeDepth;
            this.scopes[this.scopeDepth] = currentNodeType;
        }

        private JsonNodeType ExitJsonScope() {
            JsonNodeType scope = this.scopes[this.scopeDepth];
            this.scopes[this.scopeDepth] = JsonNodeType.None;
            --this.scopeDepth;
            return scope;
        }

        private static bool IsWhitespace(byte ch) {
            return ch == (byte)32 || ch == (byte)9 || ch == (byte)10 || ch == (byte)13;
        }

        private static char ParseChar(string value, NumberStyles style) {
            return Convert.ToChar((object)XmlJsonReader.ParseInt(value, style), (IFormatProvider)CultureInfo.InvariantCulture);
        }

        private static int ParseInt(string value, NumberStyles style) {
            return int.Parse(value, style, (IFormatProvider)NumberFormatInfo.InvariantInfo);
        }

        private void ParseAndSetLocalName() {
            XmlElementNode xmlElementNode = this.EnterScope();
            xmlElementNode.NameOffset = this.BufferReader.Offset;
            do {
                if (this.BufferReader.GetByte() == (byte)92)
                    this.ReadEscapedCharacter(false);
                else
                    this.ReadQuotedText(false);
            }
            while (this.complexTextMode == JsonComplexTextMode.QuotedText);
            int num = this.BufferReader.Offset - 1;
            xmlElementNode.LocalName.SetValue(xmlElementNode.NameOffset, num - xmlElementNode.NameOffset);
            xmlElementNode.NameLength = num - xmlElementNode.NameOffset;
            xmlElementNode.IsEmptyElement = false;
            xmlElementNode.ExitScope = false;
            xmlElementNode.BufferOffset = num;
        }

        private void ParseStartElement() {
            this.BufferElement();
            this.expectingFirstElementInNonPrimitiveChild = false;
            if (this.BufferReader.GetByte() != (byte)34)
                throw new Exception("TokenExpected");
            this.BufferReader.SkipByte();
            this.ParseAndSetLocalName();
            this.SkipWhitespaceInBufferReader();
            this.SkipExpectedByteInBufferReader((byte)58);
            this.SkipWhitespaceInBufferReader();
            if (this.BufferReader.GetByte() == (byte)123) {
                this.BufferReader.SkipByte();
                this.expectingFirstElementInNonPrimitiveChild = true;
            }
            this.ReadAttributes();
        }

        public override int ReadValueChunk(char[] iChars, int offset, int count) {
            if (iChars == null)
                throw new ArgumentNullException("chars");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset > iChars.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count > iChars.Length - offset)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (this.IsAttributeValue) {
                int actual;
                if (this.value == null && this.Node.Value.TryReadChars(iChars, offset, count, out actual))
                    return actual;
                string str = this.Value;
                actual = Math.Min(count, str.Length);
                str.CopyTo(0, iChars, offset, actual);
                this.value = str.Substring(actual);
                return actual;
            }
            string str1 = this.UnescapeJsonString(this.Node.ValueAsString);
            int count1 = Math.Min(count, str1.Length);
            if (count1 > 0) {
                str1.CopyTo(0, iChars, offset, count1);
                this.Node.Value.SetValue(ValueHandleType.UTF8, 0, 0);
            }
            return count1;
        }

        public override string ReadElementContentAsString() {
            if (this.Node.NodeType != XmlNodeType.Element)
                this.MoveToStartElement();
            if (this.Node.IsEmptyElement) {
                this.Read();
                return string.Empty;
            }
            this.Read();
            string str = this.ReadContentAsString();
            this.ReadEndElement();
            return str;
        }

        public new string ReadElementString() {
            this.MoveToStartElement();
            if (this.IsEmptyElement) {
                this.Read();
                return string.Empty;
            }
            this.Read();
            string str = this.ReadElementContentAsString();
            this.ReadEndElement();
            return str;
        }

        public new string ReadElementString(string name) {
            this.MoveToStartElement(name);
            return this.ReadElementString();
        }

        public override void ReadStartElement() {
            if (this.Node.NodeType != XmlNodeType.Element)
                this.MoveToStartElement();
            this.Read();
        }

        public override void ReadStartElement(string name) {
            this.MoveToStartElement(name);
            this.Read();
        }

        public override void ReadStartElement(string slocalName, string namespaceUri) {
            throw new NotSupportedException("namespace URI is not supported in this version");
        }

        public override void ReadEndElement() {
            if (this.Node.NodeType != XmlNodeType.EndElement && this.MoveToContent() != XmlNodeType.EndElement) {
                if ((this.Node.NodeType == XmlNodeType.Element ? this.depth - 1 : this.depth) == 0)
                    throw new InvalidOperationException("XmlEndElementNoOpenNodes");
                throw new XmlException("EndElementExpected");
            }
            this.Read();
        }

        public override bool Read() {
            try {


                if (this.Node.CanMoveToElement)
                    this.MoveToElement();
                if (this.Node.ReadState == ReadState.Closed)
                    return false;
                if (this.Node.ExitScope)
                    this.ExitScope();
                this.BufferReader.SetWindow(this.ElementNode.BufferOffset, this.maxBytesPerRead);
                byte ch;
                if (!this.IsReadingComplexText) {
                    this.SkipWhitespaceInBufferReader();
                    if (this.TryGetByte(out ch) && ((int)this.charactersToSkipOnNextRead[0] == (int)ch || (int)this.charactersToSkipOnNextRead[1] == (int)ch)) {
                        this.BufferReader.SkipByte();
                        this.charactersToSkipOnNextRead[0] = (byte)0;
                        this.charactersToSkipOnNextRead[1] = (byte)0;
                    }
                    this.SkipWhitespaceInBufferReader();
                    if (this.TryGetByte(out ch) && ch == (byte)93 && this.IsReadingCollection) {
                        this.BufferReader.SkipByte();
                        this.SkipWhitespaceInBufferReader();
                        int num = (int)this.ExitJsonScope();
                    }
                    if (this.BufferReader.EndOfFile) {
                        if (this.scopeDepth > 0) {
                            this.MoveToEndElement();
                            return true;
                        }
                        this.MoveToEndOfFile();
                        return false;
                    }
                }
                ch = this.BufferReader.GetByte();
                if (this.scopeDepth == 0)
                    this.ReadNonExistentElementName(StringHandleConstStringType.Root);
                else if (this.IsReadingComplexText) {
                    switch (this.complexTextMode) {
                        case JsonComplexTextMode.QuotedText:
                            if (ch == (byte)92) {
                                this.ReadEscapedCharacter(true);
                                break;
                            }
                            this.ReadQuotedText(true);
                            break;
                        case JsonComplexTextMode.NumericalText:
                            this.ReadNumericalText();
                            break;
                        case JsonComplexTextMode.None:
                            throw new XmlException("JsonEncounteredUnexpectedCharacter");
                    }
                } else if (this.IsReadingCollection) {
                    this.ReadNonExistentElementName(StringHandleConstStringType.Item);
                } else {
                    switch (ch) {
                        case 34:
                            if (this.Node.NodeType == XmlNodeType.Element) {
                                if (this.expectingFirstElementInNonPrimitiveChild) {
                                    this.EnterJsonScope(JsonNodeType.Object);
                                    this.ParseStartElement();
                                    break;
                                }
                                this.BufferReader.SkipByte();
                                this.ReadQuotedText(true);
                                break;
                            }
                            if (this.Node.NodeType != XmlNodeType.EndElement)
                                throw new XmlException("JsonEncounteredUnexpectedCharacter");
                            this.EnterJsonScope(JsonNodeType.Element);
                            this.ParseStartElement();
                            break;
                        case 44:
                            this.BufferReader.SkipByte();
                            this.MoveToEndElement();
                            break;
                        case 93:
                            this.BufferReader.SkipByte();
                            this.MoveToEndElement();
                            int num1 = (int)this.ExitJsonScope();
                            break;
                        case 102:
                            int outOffset1;
                            byte[] buffer1 = this.BufferReader.GetBuffer(5, out outOffset1);
                            if (buffer1[outOffset1 + 1] != (byte)97 || buffer1[outOffset1 + 2] != (byte)108 || buffer1[outOffset1 + 3] != (byte)115 || buffer1[outOffset1 + 4] != (byte)101)
                                throw new Exception("Expected False");
                            this.BufferReader.Advance(5);
                            if (this.TryGetByte(out ch) && !XmlJsonReader.IsWhitespace(ch) && (ch != (byte)44 && ch != (byte)125) && ch != (byte)93)
                                throw new Exception("TokenExpected");
                            this.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, outOffset1, 5);
                            break;
                        case 110:
                            int outOffset2;
                            byte[] buffer2 = this.BufferReader.GetBuffer(4, out outOffset2);
                            if (buffer2[outOffset2 + 1] != (byte)117 || buffer2[outOffset2 + 2] != (byte)108 || buffer2[outOffset2 + 3] != (byte)108)
                                throw new Exception("Expected null");
                            this.BufferReader.Advance(4);
                            this.SkipWhitespaceInBufferReader();
                            if (this.TryGetByte(out ch)) {
                                if (ch == (byte)44 || ch == (byte)125)
                                    this.BufferReader.SkipByte();
                                else if (ch != (byte)93)
                                    throw new Exception("TokenExpected");
                            } else {
                                this.charactersToSkipOnNextRead[0] = (byte)44;
                                this.charactersToSkipOnNextRead[1] = (byte)125;
                            }
                            this.MoveToEndElement();
                            break;
                        case 116:
                            int outOffset3;
                            byte[] buffer3 = this.BufferReader.GetBuffer(4, out outOffset3);
                            if (buffer3[outOffset3 + 1] != (byte)114 || buffer3[outOffset3 + 2] != (byte)117 || buffer3[outOffset3 + 3] != (byte)101)
                                throw new Exception("expected true");
                            this.BufferReader.Advance(4);
                            if (this.TryGetByte(out ch) && !XmlJsonReader.IsWhitespace(ch) && (ch != (byte)44 && ch != (byte)125) && ch != (byte)93)
                                throw new Exception("TokenExpected");
                            this.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, outOffset3, 4);
                            break;
                        case 123:
                            this.BufferReader.SkipByte();
                            this.SkipWhitespaceInBufferReader();
                            ch = this.BufferReader.GetByte();
                            if (ch == (byte)125) {
                                this.BufferReader.SkipByte();
                                this.SkipWhitespaceInBufferReader();
                                if (this.TryGetByte(out ch)) {
                                    if (ch == (byte)44)
                                        this.BufferReader.SkipByte();
                                } else
                                    this.charactersToSkipOnNextRead[0] = (byte)44;
                                this.MoveToEndElement();
                                break;
                            }
                            this.EnterJsonScope(JsonNodeType.Object);
                            this.ParseStartElement();
                            break;
                        case 125:
                            this.BufferReader.SkipByte();
                            if (this.expectingFirstElementInNonPrimitiveChild) {
                                this.SkipWhitespaceInBufferReader();
                                ch = this.BufferReader.GetByte();
                                switch (ch) {
                                    case 44:
                                    case 125:
                                        this.BufferReader.SkipByte();
                                        this.expectingFirstElementInNonPrimitiveChild = false;
                                        break;
                                    default:
                                        throw new XmlException("JsonEncounteredUnexpectedCharacter");
                                }
                            }
                            this.MoveToEndElement();
                            break;
                        default:
                            if (ch != (byte)45 && ((byte)48 > ch || ch > (byte)57) && ch != (byte)73 && ch != (byte)78)
                                throw new XmlException("JsonEncounteredUnexpectedCharacter");
                            this.ReadNumericalText();
                            break;
                    }
                }
                return true;
            } catch (Exception ex) {

                throw ex;
            }
        }

        private void ReadAttributes() {
            XmlAttributeNode xmlAttributeNode = this.AddAttribute();
            xmlAttributeNode.LocalName.SetConstantValue(StringHandleConstStringType.Type);
            this.SkipWhitespaceInBufferReader();
            byte num = this.BufferReader.GetByte();
            switch (num) {
                case 34:
                    if (!this.expectingFirstElementInNonPrimitiveChild) {
                        xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.String);
                        break;
                    }
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Object);
                    break;
                case 91:
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Array);
                    this.BufferReader.SkipByte();
                    this.EnterJsonScope(JsonNodeType.Collection);
                    break;
                case 102:
                case 116:
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Boolean);
                    break;
                case 110:
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Null);
                    break;
                case 123:
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Object);
                    break;
                case 125:
                    if (!this.expectingFirstElementInNonPrimitiveChild)
                        throw new XmlException("JsonEncounteredUnexpectedCharacter");
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Object);
                    break;
                default:
                    if (num != (byte)45 && (num > (byte)57 || num < (byte)48) && num != (byte)78 && num != (byte)73)
                        throw new XmlException("JsonEncounteredUnexpectedCharacter");
                    xmlAttributeNode.Value.SetConstantValue(ValueHandleConstStringType.Number);
                    break;
            }
        }

        private void ReadEscapedCharacter(bool moveToText) {
            this.BufferReader.SkipByte();
            char ch1 = (char)this.BufferReader.GetByte();
            if (ch1 == 'u') {
                this.BufferReader.SkipByte();
                int outOffset;
                byte[] buffer = this.BufferReader.GetBuffer(5, out outOffset);
                string str1 = Encoding.UTF8.GetString(buffer, outOffset, 4);
                this.BufferReader.Advance(4);
                int ch2 = (int)XmlJsonReader.ParseChar(str1, NumberStyles.HexNumber);
                if (char.IsHighSurrogate((char)ch2) && this.BufferReader.GetByte() == (byte)92) {
                    this.BufferReader.SkipByte();
                    this.SkipExpectedByteInBufferReader((byte)117);
                    buffer = this.BufferReader.GetBuffer(5, out outOffset);
                    string str2 = Encoding.UTF8.GetString(buffer, outOffset, 4);
                    this.BufferReader.Advance(4);
                    if (!char.IsLowSurrogate(XmlJsonReader.ParseChar(str2, NumberStyles.HexNumber)))
                        throw new XmlException("XmlInvalidLowSurrogate");
                }
                if (buffer[outOffset + 4] == (byte)34) {
                    this.BufferReader.SkipByte();
                    if (moveToText)
                        this.MoveToAtomicText().Value.SetCharValue(ch2);
                    this.complexTextMode = JsonComplexTextMode.None;
                } else {
                    if (moveToText)
                        this.MoveToComplexText().Value.SetCharValue(ch2);
                    this.complexTextMode = JsonComplexTextMode.QuotedText;
                }
            } else {
                switch (ch1) {
                    case '"':
                    case '/':
                    case '\\':
                    case 'f':
                        this.BufferReader.SkipByte();
                        if (this.BufferReader.GetByte() == (byte)34) {
                            this.BufferReader.SkipByte();
                            if (moveToText)
                                this.MoveToAtomicText().Value.SetCharValue((int)ch1);
                            this.complexTextMode = JsonComplexTextMode.None;
                        } else {
                            if (moveToText)
                                this.MoveToComplexText().Value.SetCharValue((int)ch1);
                            this.complexTextMode = JsonComplexTextMode.QuotedText;
                        }
                        break;
                    case 'b':
                        ch1 = '\b';
                        goto case '"';
                    case 'n':
                        ch1 = '\n';
                        goto case '"';
                    case 'r':
                        ch1 = '\r';
                        goto case '"';
                    case 't':
                        ch1 = '\t';
                        goto case '"';
                    default:
                        throw new XmlException("JsonEncounteredUnexpectedCharacter");
                }
            }
        }

        private void ReadNonExistentElementName(StringHandleConstStringType elementName) {
            this.EnterJsonScope(JsonNodeType.Object);
            XmlElementNode xmlElementNode = this.EnterScope();
            xmlElementNode.LocalName.SetConstantValue(elementName);
            xmlElementNode.BufferOffset = this.BufferReader.Offset;
            xmlElementNode.IsEmptyElement = false;
            xmlElementNode.ExitScope = false;
            this.ReadAttributes();
        }

        private int ReadNonFFFE() {
            int outOffset;
            byte[] buffer = this.BufferReader.GetBuffer(3, out outOffset);
            if (buffer[outOffset + 1] == (byte)191 && (buffer[outOffset + 2] == (byte)190 || buffer[outOffset + 2] == (byte)191))
                throw new XmlException("JsonInvalidFFFE");
            return 3;
        }

        private void ReadNumericalText() {
            int outOffset;
            int outOffsetMax;
            byte[] buffer = this.BufferReader.GetBuffer(2048, out outOffset, out outOffsetMax);
            int numericalTextLength = XmlJsonReader.ComputeNumericalTextLength(buffer, outOffset, outOffsetMax);
            int num = XmlJsonReader.BreakText(buffer, outOffset, numericalTextLength);
            this.BufferReader.Advance(num);
            if (outOffset <= outOffsetMax - num) {
                this.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, outOffset, num);
                this.complexTextMode = JsonComplexTextMode.None;
            } else {
                this.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, outOffset, num);
                this.complexTextMode = JsonComplexTextMode.NumericalText;
            }
        }

        private void ReadQuotedText(bool moveToText) {
            int outOffset;
            int outOffsetMax;
            byte[] buffer = this.BufferReader.GetBuffer(2048, out outOffset, out outOffsetMax);
            bool oEscaped;
            int lengthUntilEndQuote = XmlJsonReader.ComputeQuotedTextLengthUntilEndQuote(buffer, outOffset, outOffsetMax, out oEscaped);
            int num = XmlJsonReader.BreakText(buffer, outOffset, lengthUntilEndQuote);
            if (oEscaped && this.BufferReader.GetByte() == (byte)239) {
                outOffset = this.BufferReader.Offset;
                num = this.ReadNonFFFE();
            }
            this.BufferReader.Advance(num);
            if (!oEscaped && outOffset < outOffsetMax - num) {
                if (moveToText)
                    this.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, outOffset, num);
                this.SkipExpectedByteInBufferReader((byte)34);
                this.complexTextMode = JsonComplexTextMode.None;
            } else if (num == 0 & oEscaped) {
                this.ReadEscapedCharacter(moveToText);
            } else {
                if (moveToText)
                    this.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, outOffset, num);
                this.complexTextMode = JsonComplexTextMode.QuotedText;
            }
        }

        private void SkipExpectedByteInBufferReader(byte characterToSkip) {
            if ((int)this.BufferReader.GetByte() != (int)characterToSkip)
                throw new Exception("TokenExpected");
            this.BufferReader.SkipByte();
        }

        private void SkipWhitespaceInBufferReader() {
            byte ch;
            while (this.TryGetByte(out ch) && XmlJsonReader.IsWhitespace(ch))
                this.BufferReader.SkipByte();
        }

        private bool TryGetByte(out byte ch) {
            int outOffset;
            int outOffsetMax;
            byte[] buffer = this.BufferReader.GetBuffer(1, out outOffset, out outOffsetMax);
            if (outOffset < outOffsetMax) {
                ch = buffer[outOffset];
                return true;
            }
            ch = (byte)0;
            return false;
        }

        private string UnescapeJsonString(string val) {
            if (val == null)
                return (string)null;
            StringBuilder stringBuilder = new StringBuilder();
            int startIndex = 0;
            int count = 0;
            for (int index = 0; index < val.Length; ++index) {
                if (val[index] == '\\') {
                    stringBuilder.Append(val, startIndex, count);
                    ++index;
                    if (index >= val.Length)
                        throw new XmlException("JsonEncounteredUnexpectedCharacter");
                    switch (val[index]) {
                        case '"':
                        case '\'':
                        case '/':
                        case '\\':
                            stringBuilder.Append(val[index]);
                            break;
                        case 'b':
                            stringBuilder.Append('\b');
                            break;
                        case 'f':
                            stringBuilder.Append('\f');
                            break;
                        case 'n':
                            stringBuilder.Append('\n');
                            break;
                        case 'r':
                            stringBuilder.Append('\r');
                            break;
                        case 't':
                            stringBuilder.Append('\t');
                            break;
                        case 'u':
                            if (index + 3 >= val.Length)
                                throw new XmlException("JsonEncounteredUnexpectedCharacter");
                            stringBuilder.Append(XmlJsonReader.ParseChar(val.Substring(index + 1, 4), NumberStyles.HexNumber));
                            index += 4;
                            break;
                    }
                    startIndex = index + 1;
                    count = 0;
                } else
                    ++count;
            }
            if (stringBuilder.Length == 0)
                return val;
            if (count > 0)
                stringBuilder.Append(val, startIndex, count);
            return stringBuilder.ToString();
        }

        public override bool ReadAttributeValue() {
            XmlAttributeTextNode attributeText = this.Node.AttributeText;
            if (attributeText == null)
                return false;
            this.MoveToNode((XmlNode)attributeText);
            return true;
        }

        private void SkipValue(XmlNode xmlNode) {
            if (!xmlNode.SkipValue)
                return;
            this.Read();
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count) {
            if (!this.readingElement) {
                if (this.IsEmptyElement) {
                    this.Read();
                    return 0;
                }
                this.ReadStartElement();
                this.readingElement = true;
            }
            int num = this.ReadContentAsBase64(buffer, offset, count);
            if (num == 0) {
                this.ReadEndElement();
                this.readingElement = false;
            }
            return num;
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count) {
            throw new NotSupportedException("ReadContentAsBase64(byte[] buffer, int offset, int count)");
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count) {
            throw new NotSupportedException("ReadElementContentAsBinHex(byte[] buffer, int offset, int count)");
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count) {
            throw new NotSupportedException("ReadContentAsBinHex(byte[] buffer, int offset, int count)");
        }

        public override string ReadContentAsString() {
            XmlNode node = this.Node;
            if (!node.IsAtomicValue)
                return base.ReadContentAsString();
            string str;
            if (this.value != null) {
                str = this.value;
                if (node.AttributeText == null)
                    this.value = string.Empty;
            } else {
                str = node.Value.GetString();
                this.SkipValue(node);
                if (str.Length > this.quotas.MaxStringContentLength)
                    throw new XmlException("MaxStringContentLengthExceeded");
            }
            return str;
        }

        public override bool ReadContentAsBoolean() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToBoolean(this.ReadContentAsString());
            bool boolean = node.Value.ToBoolean();
            this.SkipValue(node);
            return boolean;
        }

        public override long ReadContentAsLong() {
            return long.Parse(this.ReadContentAsString(), NumberStyles.Float, (IFormatProvider)NumberFormatInfo.InvariantInfo);
        }

        public override int ReadContentAsInt() {
            return XmlJsonReader.ParseInt(this.ReadContentAsString(), NumberStyles.Float);
        }

        public new DateTime ReadContentAsDateTime() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToDateTime(this.ReadContentAsString());
            DateTime dateTime = node.Value.ToDateTime();
            this.SkipValue(node);
            return dateTime;
        }

        public override double ReadContentAsDouble() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToDouble(this.ReadContentAsString());
            double num = node.Value.ToDouble();
            this.SkipValue(node);
            return num;
        }

        public override float ReadContentAsFloat() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToSingle(this.ReadContentAsString());
            float single = node.Value.ToSingle();
            this.SkipValue(node);
            return single;
        }

        public override Decimal ReadContentAsDecimal() {
            return Decimal.Parse(this.ReadContentAsString(), NumberStyles.Float, (IFormatProvider)NumberFormatInfo.InvariantInfo);
        }

        public UniqueId ReadContentAsUniqueId() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToUniqueId(this.ReadContentAsString());
            UniqueId uniqueId = node.Value.ToUniqueId();
            this.SkipValue(node);
            return uniqueId;
        }

        public TimeSpan ReadContentAsTimeSpan() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToTimeSpan(this.ReadContentAsString());
            TimeSpan timeSpan = node.Value.ToTimeSpan();
            this.SkipValue(node);
            return timeSpan;
        }

        public Guid ReadContentAsGuid() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return XmlConverter.ToGuid(this.ReadContentAsString());
            Guid guid = node.Value.ToGuid();
            this.SkipValue(node);
            return guid;
        }

        public override object ReadContentAsObject() {
            XmlNode node = this.Node;
            if (this.value != null || !node.IsAtomicValue)
                return (object)this.ReadContentAsString();
            object obj = node.Value.ToObject();
            this.SkipValue(node);
            return obj;
        }

        public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver) {
            if (type == typeof(ulong)) {
                if (this.value != null || !this.Node.IsAtomicValue)
                    return (object)XmlConverter.ToUInt64(this.ReadContentAsString());
                ulong num = this.Node.Value.ToULong();
                this.SkipValue(this.Node);
                return (object)num;
            }
            if (type == typeof(bool))
                return (object)this.ReadContentAsBoolean();
            if (type == typeof(int))
                return (object)this.ReadContentAsInt();
            if (type == typeof(long))
                return (object)this.ReadContentAsLong();
            if (type == typeof(float))
                return (object)this.ReadContentAsFloat();
            if (type == typeof(double))
                return (object)this.ReadContentAsDouble();
            if (type == typeof(Decimal))
                return (object)this.ReadContentAsDecimal();
            if (type == typeof(DateTime))
                return (object)this.ReadContentAsDateTime();
            if (type == typeof(UniqueId))
                return (object)this.ReadContentAsUniqueId();
            if (type == typeof(Guid))
                return (object)this.ReadContentAsGuid();
            if (type == typeof(TimeSpan))
                return (object)this.ReadContentAsTimeSpan();
            if (type == typeof(object))
                return this.ReadContentAsObject();
            if (type == typeof(string))
                return (object)this.ReadContentAsString();
            return base.ReadContentAs(type, namespaceResolver);
        }

        public override void ResolveEntity() {
            throw new InvalidOperationException("XmlInvalidOperation");
        }

        public override void Skip() {
            if (this.Node.ReadState != ReadState.Interactive)
                return;
            if ((this.Node.NodeType == XmlNodeType.Element || this.MoveToElement()) && !this.IsEmptyElement) {
                int depth = this.Depth;
                do
#pragma warning disable CS0642
                    ; // Intentionally empty!
#pragma warning restore CS0642
                while (this.Read() && depth < this.Depth);
                if (this.Node.NodeType != XmlNodeType.EndElement)
                    return;
                this.Read();
            } else
                this.Read();
        }
    }
}
