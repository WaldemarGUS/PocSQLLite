// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.SyncReader
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microsoft.Synchronization.Services.Formatters {
    internal abstract class SyncReader : IDisposable
  {
    protected bool _currentNodeRead = false;
    protected XmlReader _reader;
    protected Stream _inputStream;
    protected Type[] _knownTypes;
    protected EntryInfoWrapper _currentEntryWrapper;
    protected ReaderItemType _currentType;
    protected IOfflineEntity _liveEntity;

    protected SyncReader(Stream stream, Type[] knownTypes)
    {
      if (stream == null)
        throw new ArgumentNullException(nameof (stream));
      this._inputStream = stream;
      this._knownTypes = knownTypes;
    }

    public abstract void Start();

    public abstract ReaderItemType ItemType { get; }

    public abstract IOfflineEntity GetItem();

    public abstract byte[] GetServerBlob();

    public abstract bool GetHasMoreChangesValue();

    public abstract bool Next();

    public virtual bool HasConflict()
    {
      if (this._currentEntryWrapper != null)
        return this._currentEntryWrapper.ConflictWrapper != null;
      return false;
    }

    public virtual bool HasConflictTempId()
    {
      if (this._currentEntryWrapper != null && this._currentEntryWrapper.ConflictWrapper != null)
        return this._currentEntryWrapper.ConflictWrapper.TempId != null;
      return false;
    }

    public virtual bool HasTempId()
    {
      if (this._currentEntryWrapper != null)
        return this._currentEntryWrapper.TempId != null;
      return false;
    }

    public virtual string GetTempId()
    {
      if (!this.HasTempId())
        return (string) null;
      return this._currentEntryWrapper.TempId;
    }

    public virtual string GetConflictTempId()
    {
      if (!this.HasConflictTempId())
        return (string) null;
      return this._currentEntryWrapper.ConflictWrapper.TempId;
    }

    public virtual Conflict GetConflict()
    {
      if (!this.HasConflict())
        return (Conflict) null;
      Conflict conflict;
      if (this._currentEntryWrapper.IsConflict)
      {
        SyncConflict syncConflict = new SyncConflict();
        syncConflict.LiveEntity = this._liveEntity;
        syncConflict.LosingEntity = ReflectionUtility.GetObjectForType(this._currentEntryWrapper.ConflictWrapper, this._knownTypes);
        syncConflict.Resolution = (SyncConflictResolution) Enum.Parse(FormatterConstants.SyncConflictResolutionType, this._currentEntryWrapper.ConflictDesc, true);
        conflict = (Conflict) syncConflict;
      }
      else
      {
        SyncError syncError = new SyncError();
        syncError.LiveEntity = this._liveEntity;
        syncError.ErrorEntity = ReflectionUtility.GetObjectForType(this._currentEntryWrapper.ConflictWrapper, this._knownTypes);
        syncError.Description = this._currentEntryWrapper.ConflictDesc;
        conflict = (Conflict) syncError;
      }
      return conflict;
    }

    protected void CheckItemType(ReaderItemType type)
    {
      if (this._currentType != type)
        throw new InvalidOperationException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} is not a valid {1} element.", (object) this._reader.Name, (object) type));
      this._currentNodeRead = true;
    }

    public void Dispose()
    {
      if (this._inputStream != null)
      {
        using (this._inputStream)
          this._inputStream.Dispose();
      }
      this._inputStream = (Stream) null;
      this._knownTypes = (Type[]) null;
    }
  }
}
