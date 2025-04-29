// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.ODataAtomWriter
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal class ODataAtomWriter : SyncWriter
  {
    private XDocument _document;
    private XElement _root;

    public ODataAtomWriter(Uri serviceUri)
      : base(serviceUri)
    {
    }

    public override void StartFeed(bool isLastBatch, byte[] serverBlob)
    {
      base.StartFeed(isLastBatch, serverBlob);
      this._document = new XDocument();
      XNamespace xnamespace = (XNamespace) this.BaseUri.ToString();
      XNamespace atomXmlNamespace = FormatterConstants.AtomXmlNamespace;
      this._root = new XElement(FormatterConstants.AtomXmlNamespace + "feed", new object[6]
      {
        (object) new XAttribute(XNamespace.Xmlns + "base", (object) xnamespace),
        (object) new XAttribute((XName) "xmlns", (object) FormatterConstants.AtomXmlNamespace),
        (object) new XAttribute(XNamespace.Xmlns + "d", (object) FormatterConstants.ODataDataNamespace),
        (object) new XAttribute(XNamespace.Xmlns + "m", (object) FormatterConstants.ODataMetadataNamespace),
        (object) new XAttribute(XNamespace.Xmlns + "at", (object) FormatterConstants.AtomDeletedEntryNamespace),
        (object) new XAttribute(XNamespace.Xmlns + "sync", (object) FormatterConstants.SyncNamespace)
      });
      this._root.Add((object) new XElement(atomXmlNamespace + "title", (object) string.Empty));
      this._root.Add((object) new XElement(atomXmlNamespace + "id", (object) Guid.NewGuid().ToString("B")));
      this._root.Add((object) new XElement(atomXmlNamespace + "updated", (object) XmlConvert.ToString(DateTime.Now)));
      this._root.Add((object) new XElement(atomXmlNamespace + "link", new object[2]
      {
        (object) new XAttribute((XName) "rel", (object) "self"),
        (object) new XAttribute((XName) "href", (object) string.Empty)
      }));
      this._root.Add((object) new XElement(FormatterConstants.SyncNamespace + "moreChangesAvailable", (object) !isLastBatch));
      this._root.Add((object) new XElement(FormatterConstants.SyncNamespace + nameof (serverBlob), serverBlob != null ? (object) Convert.ToBase64String(serverBlob) : (object) "null"));
    }

    public override void WriteFeed(XmlWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof (writer));
      this._document.Add((object) this._root);
      writer.WriteNode(this._document.CreateReader(), true);
      writer.Flush();
    }

    public override void WriteItemInternal(
      IOfflineEntity live,
      string liveTempId,
      IOfflineEntity conflicting,
      string conflictingTempId,
      string desc,
      bool isConflict,
      bool emitMetadataOnly)
    {
      XElement xelement1 = this.WriteEntry(live, liveTempId, emitMetadataOnly);
      if (conflicting != null)
      {
        XElement xelement2 = new XElement(FormatterConstants.SyncNamespace + (isConflict ? "syncConflict" : "syncError"));
        xelement2.Add((object) new XElement(FormatterConstants.SyncNamespace + (isConflict ? "conflictResolution" : "errorDescription"), (object) desc));
        XElement xelement3 = new XElement(FormatterConstants.SyncNamespace + (isConflict ? "conflictingChange" : "changeInError"));
        xelement3.Add((object) this.WriteEntry(conflicting, conflictingTempId, false));
        xelement2.Add((object) xelement3);
        xelement1.Add((object) xelement2);
      }
      this._root.Add((object) xelement1);
    }

    private XElement WriteEntry(IOfflineEntity live, string tempId, bool emitPartial)
    {
      string fullName = live.GetType().FullName;
      if (!live.ServiceMetadata.IsTombstone)
      {
        XElement xelement1 = new XElement(FormatterConstants.AtomXmlNamespace + "entry");
        if (!string.IsNullOrEmpty(live.ServiceMetadata.ETag))
          xelement1.Add((object) new XAttribute(FormatterConstants.ODataMetadataNamespace + "etag", (object) live.ServiceMetadata.ETag));
        if (!string.IsNullOrEmpty(tempId))
          xelement1.Add((object) new XElement(FormatterConstants.SyncNamespace + nameof (tempId), (object) tempId));
        xelement1.Add((object) new XElement(FormatterConstants.AtomXmlNamespace + "id", string.IsNullOrEmpty(live.ServiceMetadata.Id) ? (object) string.Empty : (object) live.ServiceMetadata.Id));
        xelement1.Add((object) new XElement(FormatterConstants.AtomXmlNamespace + "title", (object) new XAttribute((XName) "type", (object) "text")));
        xelement1.Add((object) new XElement(FormatterConstants.AtomXmlNamespace + "updated", (object) XmlConvert.ToString(DateTime.Now)));
        xelement1.Add((object) new XElement(FormatterConstants.AtomXmlNamespace + "author", (object) new XElement(FormatterConstants.AtomXmlNamespace + "name")));
        xelement1.Add((object) new XElement(FormatterConstants.AtomXmlNamespace + "link", new object[3]
        {
          (object) new XAttribute((XName) "rel", (object) "edit"),
          (object) new XAttribute((XName) "title", (object) fullName),
          (object) new XAttribute((XName) "href", live.ServiceMetadata.EditUri != (Uri) null ? (object) live.ServiceMetadata.EditUri.ToString() : (object) string.Empty)
        }));
        xelement1.Add((object) new XElement(FormatterConstants.AtomXmlNamespace + "category", new object[2]
        {
          (object) new XAttribute((XName) "term", (object) live.GetType().FullName),
          (object) new XAttribute((XName) "schema", (object) FormatterConstants.ODataSchemaNamespace)
        }));
        XElement xelement2 = new XElement(FormatterConstants.AtomXmlNamespace + "content");
        if (!emitPartial)
          xelement2.Add((object) this.WriteEntityContents(live));
        xelement1.Add((object) xelement2);
        return xelement1;
      }
      XElement xelement = new XElement(FormatterConstants.AtomDeletedEntryNamespace + "deleted-entry");
      xelement.Add((object) new XElement(FormatterConstants.AtomNamespaceUri + "ref", (object) live.ServiceMetadata.Id));
      xelement.Add((object) new XElement(FormatterConstants.SyncNamespace + "category", (object) fullName));
      return xelement;
    }

    private XElement WriteEntityContents(IOfflineEntity entity)
    {
      XElement xelement = new XElement(FormatterConstants.ODataMetadataNamespace + "properties");
      foreach (PropertyInfo propertyInfo in ReflectionUtility.GetPropertyInfoMapping(entity.GetType()))
      {
        string edmType = FormatterUtilities.GetEdmType(propertyInfo.PropertyType);
        object objValue = propertyInfo.GetValue((object) entity, (object[]) null);
        string text = objValue as string;
        if (text != null)
          objValue = (object) AtomHelper.CleanInvalidXmlChars(text);
        Type type = propertyInfo.PropertyType;
        if (propertyInfo.PropertyType.IsGenericType() && propertyInfo.PropertyType.Name.Equals("Nullable`1", StringComparison.CurrentCulture))
          type = propertyInfo.PropertyType.GetGenericArguments()[0];
        if (objValue == null)
          xelement.Add((object) new XElement(FormatterConstants.ODataDataNamespace + propertyInfo.Name, new object[2]
          {
            (object) new XAttribute(FormatterConstants.ODataMetadataNamespace + "type", (object) edmType),
            (object) new XAttribute(FormatterConstants.ODataMetadataNamespace + "null", (object) true)
          }));
        else if (type == FormatterConstants.DateTimeType || type == FormatterConstants.TimeSpanType || type == FormatterConstants.DateTimeOffsetType)
          xelement.Add((object) new XElement(FormatterConstants.ODataDataNamespace + propertyInfo.Name, new object[2]
          {
            (object) new XAttribute(FormatterConstants.ODataMetadataNamespace + "type", (object) edmType),
            FormatterUtilities.ConvertDateTimeForType_Atom(objValue, type)
          }));
        else if (type != FormatterConstants.ByteArrayType)
        {
          xelement.Add((object) new XElement(FormatterConstants.ODataDataNamespace + propertyInfo.Name, new object[2]
          {
            (object) new XAttribute(FormatterConstants.ODataMetadataNamespace + "type", (object) edmType),
            objValue
          }));
        }
        else
        {
          byte[] inArray = (byte[]) objValue;
          xelement.Add((object) new XElement(FormatterConstants.ODataDataNamespace + propertyInfo.Name, new object[2]
          {
            (object) new XAttribute(FormatterConstants.ODataMetadataNamespace + "type", (object) edmType),
            (object) Convert.ToBase64String(inArray)
          }));
        }
      }
      return xelement;
    }
  }
}
