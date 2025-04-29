// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.ODataJsonWriter
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal class ODataJsonWriter : SyncWriter
  {
    private XDocument _doc;
    private XElement _root;
    private XElement _results;

    public ODataJsonWriter(Uri serviceUri)
      : base(serviceUri)
    {
    }

    public override void StartFeed(bool isLastBatch, byte[] serverBlob)
    {
      base.StartFeed(isLastBatch, serverBlob);
      this._doc = new XDocument();
      XElement xelement1 = new XElement((XName) "root", (object) new XAttribute((XName) "type", (object) "object"));
      this._doc.Add((object) xelement1);
      this._root = new XElement((XName) "d", (object) new XAttribute((XName) "type", (object) "object"));
      xelement1.Add((object) this._root);
      XElement xelement2 = new XElement((XName) "__sync", (object) new XAttribute((XName) "type", (object) "object"));
      xelement2.Add((object) new XElement((XName) "moreChangesAvailable", new object[2]
      {
        (object) new XAttribute((XName) "type", (object) "boolean"),
        (object) !isLastBatch
      }));
      xelement2.Add((object) new XElement((XName) nameof (serverBlob), new object[2]
      {
        (object) new XAttribute((XName) "type", serverBlob != null ? (object) "string" : (object) "object"),
        serverBlob != null ? (object) Convert.ToBase64String(serverBlob) : (object) "null"
      }));
      this._root.Add((object) xelement2);
      this._results = new XElement((XName) "results", (object) new XAttribute((XName) "type", (object) "array"));
      this._root.Add((object) this._results);
    }

    public override void WriteFeed(XmlWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException(nameof (writer));
      this._doc.WriteTo(writer);
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
      XElement xelement1 = this.WriteEntry(live, (XElement) null, liveTempId, emitMetadataOnly);
      if (conflicting != null)
      {
        XElement xelement2 = new XElement((XName) (isConflict ? "__syncConflict" : "__syncError"), (object) new XAttribute((XName) "type", (object) "object"));
        xelement2.Add((object) new XElement((XName) (isConflict ? "conflictResolution" : "errorDescription"), new object[2]
        {
          (object) new XAttribute((XName) "type", (object) "string"),
          (object) desc
        }));
        XElement entryElement = new XElement((XName) (isConflict ? "conflictingChange" : "changeInError"), (object) new XAttribute((XName) "type", (object) "object"));
        this.WriteEntry(conflicting, entryElement, conflictingTempId, false);
        xelement2.Add((object) entryElement);
        xelement1.Add((object) xelement2);
      }
      this._results.Add((object) xelement1);
    }

    private XElement WriteEntry(
      IOfflineEntity live,
      XElement entryElement,
      string tempId,
      bool emitPartial)
    {
      string fullName = live.GetType().FullName;
      if (entryElement == null)
        entryElement = new XElement((XName) "item", (object) new XAttribute((XName) "type", (object) "object"));
      XElement xelement = new XElement((XName) "__metadata", (object) new XAttribute((XName) "type", (object) "object"));
      if (!string.IsNullOrEmpty(tempId))
        xelement.Add((object) new XElement((XName) nameof (tempId), new object[2]
        {
          (object) new XAttribute((XName) "type", (object) "string"),
          (object) tempId
        }));
      xelement.Add((object) new XElement((XName) "uri", new object[2]
      {
        (object) new XAttribute((XName) "type", (object) "string"),
        string.IsNullOrEmpty(live.ServiceMetadata.Id) ? (object) string.Empty : (object) live.ServiceMetadata.Id
      }));
      if (!string.IsNullOrEmpty(live.ServiceMetadata.ETag))
        xelement.Add((object) new XElement((XName) "etag", new object[2]
        {
          (object) new XAttribute((XName) "type", (object) "string"),
          (object) live.ServiceMetadata.ETag
        }));
      if (live.ServiceMetadata.EditUri != (Uri) null)
        xelement.Add((object) new XElement((XName) "edituri", new object[2]
        {
          (object) new XAttribute((XName) "type", (object) "string"),
          (object) live.ServiceMetadata.EditUri
        }));
      xelement.Add((object) new XElement((XName) "type", new object[2]
      {
        (object) new XAttribute((XName) "type", (object) "string"),
        (object) fullName
      }));
      if (live.ServiceMetadata.IsTombstone)
        xelement.Add((object) new XElement((XName) "isDeleted", new object[2]
        {
          (object) new XAttribute((XName) "type", (object) "boolean"),
          (object) true
        }));
      else if (!emitPartial)
        this.WriteEntityContentsToElement(entryElement, live);
      entryElement.Add((object) xelement);
      return entryElement;
    }

    private void WriteEntityContentsToElement(XElement contentElement, IOfflineEntity entity)
    {
      foreach (PropertyInfo propertyInfo in ReflectionUtility.GetPropertyInfoMapping(entity.GetType()))
      {
        object objValue = propertyInfo.GetValue((object) entity, (object[]) null);
        if (objValue == null)
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) "null"),
            objValue
          }));
        else if (propertyInfo.PropertyType == FormatterConstants.CharType || propertyInfo.PropertyType == FormatterConstants.StringType || propertyInfo.PropertyType == FormatterConstants.GuidType)
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) "string"),
            objValue
          }));
        else if (propertyInfo.PropertyType == FormatterConstants.DateTimeType || propertyInfo.PropertyType == FormatterConstants.TimeSpanType || propertyInfo.PropertyType == FormatterConstants.DateTimeOffsetType)
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) "string"),
            FormatterUtilities.ConvertDateTimeForType_Json(objValue, propertyInfo.PropertyType)
          }));
        else if (propertyInfo.PropertyType == FormatterConstants.BoolType)
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) "boolean"),
            objValue
          }));
        else if (propertyInfo.PropertyType == FormatterConstants.ByteArrayType)
        {
          byte[] inArray = (byte[]) objValue;
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) "string"),
            (object) Convert.ToBase64String(inArray)
          }));
        }
        else if (propertyInfo.PropertyType.IsGenericType() && propertyInfo.PropertyType.Name.Equals("Nullable`1", StringComparison.CurrentCulture))
        {
          Type genericArgument = propertyInfo.PropertyType.GetGenericArguments()[0];
          string str = "number";
          if (genericArgument == FormatterConstants.BoolType)
          {
            str = "boolean";
          }
          else
          {
            if (genericArgument == FormatterConstants.DateTimeType || genericArgument == FormatterConstants.TimeSpanType || genericArgument == FormatterConstants.DateTimeOffsetType)
            {
              contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
              {
                (object) new XAttribute((XName) "type", (object) "string"),
                FormatterUtilities.ConvertDateTimeForType_Json(objValue, genericArgument)
              }));
              continue;
            }
            if (genericArgument == FormatterConstants.CharType || genericArgument == FormatterConstants.GuidType)
              str = "string";
          }
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) str),
            objValue
          }));
        }
        else
          contentElement.Add((object) new XElement((XName) propertyInfo.Name, new object[2]
          {
            (object) new XAttribute((XName) "type", (object) "number"),
            objValue
          }));
      }
    }
  }
}
