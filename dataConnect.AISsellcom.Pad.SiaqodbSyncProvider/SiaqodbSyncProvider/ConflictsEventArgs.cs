// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.ConflictsEventArgs
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Collections.Generic;

namespace SiaqodbSyncProvider {
    public class ConflictsEventArgs : EventArgs
  {
    private IEnumerable<Conflict> conflicts;

    public ConflictsEventArgs(IEnumerable<Conflict> conflicts)
    {
      this.conflicts = conflicts;
    }

    public bool CancelResolvingConflicts { get; set; }
  }
}
