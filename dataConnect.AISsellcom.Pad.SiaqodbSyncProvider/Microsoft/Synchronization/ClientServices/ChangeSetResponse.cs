// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.ChangeSetResponse
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Synchronization.ClientServices {
    public class ChangeSetResponse
  {
    private List<Conflict> _conflicts;
    private List<IOfflineEntity> _updatedItems;

    public byte[] ServerBlob { get; set; }

    public Exception Error { get; set; }

    public ReadOnlyCollection<Conflict> Conflicts
    {
      get
      {
        return new ReadOnlyCollection<Conflict>((IList<Conflict>) this._conflicts);
      }
    }

    public ReadOnlyCollection<IOfflineEntity> UpdatedItems
    {
      get
      {
        return new ReadOnlyCollection<IOfflineEntity>((IList<IOfflineEntity>) this._updatedItems);
      }
    }

    internal ChangeSetResponse()
    {
      this._conflicts = new List<Conflict>();
      this._updatedItems = new List<IOfflineEntity>();
    }

    internal void AddConflict(Conflict conflict)
    {
      this._conflicts.Add(conflict);
    }

    internal void AddUpdatedItem(IOfflineEntity item)
    {
      this._updatedItems.Add(item);
    }

    internal List<Conflict> ConflictsInternal
    {
      get
      {
        return this._conflicts;
      }
    }
  }
}
