// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.JsonEntryInfoWrapper
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal class JsonEntryInfoWrapper : EntryInfoWrapper
  {
    public JsonEntryInfoWrapper(XElement reader)
      : base(reader)
    {
    }

    protected override void LoadConflictEntry(XElement entry)
    {
      XElement xelement1 = entry.Element((XName) "__syncConflict");
      if (xelement1 != null)
      {
        this.IsConflict = true;
        XElement xelement2 = xelement1.Element((XName) "conflictResolution");
        if (xelement2 == null)
          throw new InvalidOperationException("Conflict resolution not specified for Json object " + this.TypeName);
        this.ConflictDesc = xelement2.Value;
        XElement reader = xelement1.Element((XName) "conflictingChange");
        if (reader == null)
          throw new InvalidOperationException("conflictingChange not specified for Json syncConflict object " + this.TypeName);
        this.ConflictWrapper = (EntryInfoWrapper) new JsonEntryInfoWrapper(reader);
      }
      else
      {
        XElement xelement2 = entry.Element((XName) "__syncError");
        if (xelement2 == null)
          return;
        this.IsConflict = false;
        XElement xelement3 = xelement2.Element((XName) "errorDescription");
        if (xelement3 != null)
          this.ConflictDesc = xelement3.Value;
        XElement reader = xelement2.Element((XName) "changeInError");
        if (reader == null)
          throw new InvalidOperationException("errorInChange not specified for Json syncError object " + this.TypeName);
        this.ConflictWrapper = (EntryInfoWrapper) new JsonEntryInfoWrapper(reader);
      }
    }

    protected override void LoadEntryProperties(XElement entry)
    {
      foreach (XElement element in entry.Elements())
      {
        if (!element.Name.LocalName.Equals("__metadata", StringComparison.CurrentCulture) && !element.Name.LocalName.Equals("isDeleted", StringComparison.CurrentCulture))
        {
          XAttribute xattribute = element.Attribute((XName) "type");
          if (xattribute != null && xattribute.Value.Equals("null", StringComparison.OrdinalIgnoreCase))
            this.PropertyBag[element.Name.LocalName] = (string) null;
          else
            this.PropertyBag[element.Name.LocalName] = element.Value;
        }
      }
    }

    protected override void LoadTypeName(XElement entry)
    {
      foreach (XElement element in entry.Elements((XName) "__metadata"))
      {
        this.TypeName = element.Element((XName) "type").Value;
        if (element.Element((XName) "uri") != null)
        {
          this.Id = element.Element((XName) "uri").Value;
          this.EditUri = new Uri(this.Id, UriKind.RelativeOrAbsolute);
        }
        if (element.Element((XName) "tempId") != null)
          this.TempId = element.Element((XName) "tempId").Value;
        if (string.IsNullOrEmpty(this.Id) && this.TempId == null)
          throw new InvalidOperationException("A uri or a tempId key must be present in the __metadata object. Entity in error: " + entry.ToString(SaveOptions.None));
        if (element.Element((XName) "etag") != null)
          this.ETag = element.Element((XName) "etag").Value;
        if (element.Element((XName) "edituri") != null)
          this.EditUri = new Uri(element.Element((XName) "edituri").Value, UriKind.RelativeOrAbsolute);
        if (element.Element((XName) "isDeleted") != null)
          this.IsTombstone = bool.Parse(element.Element((XName) "isDeleted").Value);
      }
      if (string.IsNullOrEmpty(this.TypeName))
        throw new InvalidOperationException("Json object does not have a _metadata tag containing the type information.");
    }
  }
}
