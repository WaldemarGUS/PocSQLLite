// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.OfflineEntityMetadata
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.ComponentModel;

namespace Microsoft.Synchronization.ClientServices {
    public class OfflineEntityMetadata : INotifyPropertyChanged
  {
    private bool _isTombstone;
    private string _id;
    private string _etag;
    private Uri _editUri;

    public OfflineEntityMetadata()
    {
      this._isTombstone = false;
      this._id = (string) null;
      this._etag = (string) null;
      this._editUri = (Uri) null;
    }

    public OfflineEntityMetadata(bool isTombstone, string id, string etag, Uri editUri)
    {
      this._isTombstone = isTombstone;
      this._id = id;
      this._etag = etag;
      this._editUri = editUri;
    }

    public bool IsTombstone
    {
      get
      {
        return this._isTombstone;
      }
      set
      {
        if (value == this._isTombstone)
          return;
        this._isTombstone = value;
        this.RaisePropertyChanged(nameof (IsTombstone));
      }
    }

    public string Id
    {
      get
      {
        return this._id;
      }
      set
      {
        if (!(value != this._id))
          return;
        this._id = value;
        this.RaisePropertyChanged(nameof (Id));
      }
    }

    public string ETag
    {
      get
      {
        return this._etag;
      }
      set
      {
        if (!(value != this._etag))
          return;
        this._etag = value;
        this.RaisePropertyChanged(nameof (ETag));
      }
    }

    public Uri EditUri
    {
      get
      {
        return this._editUri;
      }
      set
      {
        if (!(value != this._editUri))
          return;
        this._editUri = value;
        this.RaisePropertyChanged(nameof (EditUri));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    internal OfflineEntityMetadata Clone()
    {
      return new OfflineEntityMetadata(this._isTombstone, this._id, this._etag, this._editUri);
    }
  }
}
