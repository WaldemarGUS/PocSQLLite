// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.ODataJsonReader
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal class ODataJsonReader : SyncReader {
        private bool _traversingResultsNode = false;

        public ODataJsonReader(Stream stream)
          : this(stream, (Type[])null) {
        }

        public ODataJsonReader(Stream stream, Type[] knownTypes)
          : base(stream, knownTypes) {
            this._reader = (XmlReader)new XmlJsonReader(stream, XmlDictionaryReaderQuotas.Max);
        }

        public override void Start() {
            int content = (int)this._reader.MoveToContent();
            if (this._reader.Name != "root")
                throw new InvalidOperationException("Not a valid Json Feed.");
        }

        public override ReaderItemType ItemType {
            get {
                return this._currentType;
            }
        }

        public override IOfflineEntity GetItem() {
            try {
                this.CheckItemType(ReaderItemType.Entry);
                var tmp = XNode.ReadFrom(this._reader);
                var tmp2 = tmp as XElement;
                this._currentEntryWrapper = (EntryInfoWrapper)new JsonEntryInfoWrapper(tmp2);
                this._liveEntity = ReflectionUtility.GetObjectForType(this._currentEntryWrapper, this._knownTypes);
                return this._liveEntity;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public override bool GetHasMoreChangesValue() {
            this.CheckItemType(ReaderItemType.HasMoreChanges);
            return (bool)this._reader.ReadElementContentAs(FormatterConstants.BoolType, (IXmlNamespaceResolver)null);
        }

        public override byte[] GetServerBlob() {
            this.CheckItemType(ReaderItemType.SyncBlob);
            return Convert.FromBase64String((string)this._reader.ReadElementContentAs(FormatterConstants.StringType, (IXmlNamespaceResolver)null));
        }

        public override bool Next() {
            if (this._currentType != ReaderItemType.BOF && !this._currentNodeRead)
                this._reader.Skip();
            do {
                this._currentEntryWrapper = (EntryInfoWrapper)null;
                this._liveEntity = (IOfflineEntity)null;
                if (JsonHelper.IsElement(this._reader, "results")) {
                    if (this._traversingResultsNode && this._reader.IsStartElement())
                        throw new InvalidOperationException("Json feed has more than one results entry. Invalid stream.");
                    this._traversingResultsNode = this._reader.IsStartElement();
                } else {
                    if (JsonHelper.IsElement(this._reader, "item") && this._traversingResultsNode) {
                        this._currentType = ReaderItemType.Entry;
                        this._currentNodeRead = false;
                        return true;
                    }
                    if (JsonHelper.IsElement(this._reader, "serverBlob")) {
                        this._currentType = ReaderItemType.SyncBlob;
                        this._currentNodeRead = false;
                        return true;
                    }
                    if (JsonHelper.IsElement(this._reader, "moreChangesAvailable")) {
                        this._currentType = ReaderItemType.HasMoreChanges;
                        this._currentNodeRead = false;
                        return true;
                    }
                }
            }
            while (this._reader.Read());
            this._currentType = ReaderItemType.EOF;
            return false;
        }
    }
}
