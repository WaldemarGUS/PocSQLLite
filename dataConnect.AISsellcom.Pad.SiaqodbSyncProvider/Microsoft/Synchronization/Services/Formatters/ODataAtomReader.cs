// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.ODataAtomReader
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal class ODataAtomReader : SyncReader
  {
    public ODataAtomReader(Stream stream)
      : this(stream, (Type[]) null)
    {
    }

    public ODataAtomReader(Stream stream, Type[] knownTypes)
      : base(stream, knownTypes)
    {
      this._reader = XmlReader.Create(stream, new XmlReaderSettings()
      {
        CheckCharacters = false
      });
    }

    public override void Start()
    {
      int content = (int) this._reader.MoveToContent();
      if (!AtomHelper.IsAtomElement(this._reader, "feed"))
        throw new InvalidOperationException("Not a valid ATOM feed.");
    }

    public override ReaderItemType ItemType
    {
      get
      {
        return this._currentType;
      }
    }

    public override IOfflineEntity GetItem()
    {
      this.CheckItemType(ReaderItemType.Entry);
      this._currentEntryWrapper = (EntryInfoWrapper) new AtomEntryInfoWrapper((XElement) XNode.ReadFrom(this._reader));
      this._liveEntity = ReflectionUtility.GetObjectForType(this._currentEntryWrapper, this._knownTypes);
      return this._liveEntity;
    }

    public override bool GetHasMoreChangesValue()
    {
      this.CheckItemType(ReaderItemType.HasMoreChanges);
      return (bool) this._reader.ReadElementContentAs(FormatterConstants.BoolType, (IXmlNamespaceResolver) null);
    }

    public override byte[] GetServerBlob()
    {
      this.CheckItemType(ReaderItemType.SyncBlob);
      return Convert.FromBase64String((string) this._reader.ReadElementContentAs(FormatterConstants.StringType, (IXmlNamespaceResolver) null));
    }

    public override bool Next()
    {
      if (this._currentType != ReaderItemType.BOF && !this._currentNodeRead)
        this._reader.Skip();
      do
      {
        this._currentEntryWrapper = (EntryInfoWrapper) null;
        this._liveEntity = (IOfflineEntity) null;
        if (AtomHelper.IsAtomElement(this._reader, "entry") || AtomHelper.IsAtomTombstone(this._reader, "deleted-entry"))
        {
          this._currentType = ReaderItemType.Entry;
          this._currentNodeRead = false;
          return true;
        }
        if (AtomHelper.IsSyncElement(this._reader, "serverBlob"))
        {
          this._currentType = ReaderItemType.SyncBlob;
          this._currentNodeRead = false;
          return true;
        }
        if (AtomHelper.IsSyncElement(this._reader, "moreChangesAvailable"))
        {
          this._currentType = ReaderItemType.HasMoreChanges;
          this._currentNodeRead = false;
          return true;
        }
      }
      while (this._reader.Read());
      this._currentType = ReaderItemType.EOF;
      return false;
    }
  }
}
