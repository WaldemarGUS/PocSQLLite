// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.SiaqodbOfflineEntity
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using Newtonsoft.Json;
using Sqo;
using Sqo.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SiaqodbSyncProvider {
    [Obfuscation(Exclude = true)]
  public class SiaqodbOfflineEntity : SqoDataObject, IOfflineEntity
  {
    private bool isTombstone;
    private bool isDirty;
    private ulong tickCount;
    private string _etag;
    [Text]
    [JsonProperty]
    internal string _idMeta;
    [Index]
    internal int _idMetaHash;
    [Ignore]
    private OfflineEntityMetadata _entityMetadata;

    [JsonProperty]
    internal bool IsTombstone
    {
      get
      {
        return this.isTombstone;
      }
      set
      {
        this.isTombstone = value;
      }
    }

    public bool IsDirty
    {
      get
      {
        return this.isDirty;
      }
      set
      {
        this.isDirty = value;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public OfflineEntityMetadata ServiceMetadata
    {
      get
      {
        if (this._entityMetadata == null)
        {
          this._entityMetadata = new OfflineEntityMetadata();
          this._entityMetadata.ETag = this._etag;
          this._entityMetadata.Id = this._idMeta;
          this._entityMetadata.IsTombstone = this.isTombstone;
        }
        return this._entityMetadata;
      }
      set
      {
        this._entityMetadata = value;
        this._etag = this._entityMetadata.ETag;
        this._idMeta = this._entityMetadata.Id;
        this._idMetaHash = this._idMeta == null ? 0 : this._idMeta.GetHashCode();
        this.isTombstone = this._entityMetadata.IsTombstone;
      }
    }

    public SiaqodbOfflineEntity(): base()
    {
    }
  }
}
