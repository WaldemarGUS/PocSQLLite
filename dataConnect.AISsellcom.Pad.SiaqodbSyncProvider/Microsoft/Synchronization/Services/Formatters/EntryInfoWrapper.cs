// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.EntryInfoWrapper
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal abstract class EntryInfoWrapper
  {
    public Dictionary<string, string> PropertyBag = new Dictionary<string, string>();
    public string TypeName;
    public bool IsTombstone;
    public string ConflictDesc;
    public EntryInfoWrapper ConflictWrapper;
    public bool IsConflict;
    public string ETag;
    public string TempId;
    public Uri EditUri;
    public string Id;

    protected abstract void LoadConflictEntry(XElement entry);

    protected abstract void LoadEntryProperties(XElement entry);

    protected abstract void LoadTypeName(XElement entry);

    protected EntryInfoWrapper(XElement reader)
    {
      if (reader == null)
        throw new ArgumentNullException(nameof (reader));
      this.PropertyBag = new Dictionary<string, string>();
      this.LoadTypeName(reader);
      this.LoadEntryProperties(reader);
      this.LoadConflictEntry(reader);
    }
  }
}
