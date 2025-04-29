// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.SyncWriter
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Xml;

namespace Microsoft.Synchronization.Services.Formatters {
    internal abstract class SyncWriter
  {
    private Uri _baseUri;

    protected Uri BaseUri
    {
      get
      {
        return this._baseUri;
      }
    }

    protected SyncWriter(Uri serviceUri)
    {
      if (serviceUri == (Uri) null)
        throw new ArgumentNullException(nameof (serviceUri));
      this._baseUri = serviceUri;
    }

    public virtual void StartFeed(bool isLastBatch, byte[] serverBlob)
    {
    }

    public virtual void AddItem(IOfflineEntity entry, string tempId)
    {
      this.AddItem(entry, tempId, false);
    }

    public virtual void AddItem(IOfflineEntity entry, string tempId, bool emitMetadataOnly)
    {
      if (entry == null)
        throw new ArgumentNullException(nameof (entry));
      if (string.IsNullOrEmpty(entry.ServiceMetadata.Id) && entry.ServiceMetadata.IsTombstone)
        return;
      this.WriteItemInternal(entry, tempId, (IOfflineEntity) null, (string) null, (string) null, false, emitMetadataOnly);
    }

    public abstract void WriteItemInternal(
      IOfflineEntity live,
      string liveTempId,
      IOfflineEntity conflicting,
      string conflictingTempId,
      string desc,
      bool isConflict,
      bool emitMetadataOnly);

    public abstract void WriteFeed(XmlWriter writer);
  }
}
