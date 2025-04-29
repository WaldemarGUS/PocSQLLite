// Decompiled with JetBrains decompiler
// Type: System.Reflection.ObfuscateAssemblyAttribute
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Runtime.InteropServices;

namespace System.Reflection {
    [ComVisible(true)]
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
  internal sealed class ObfuscateAssemblyAttribute : Attribute
  {
    private bool m_assemblyIsPrivate;
    private bool m_stripAfterObfuscation;

    public ObfuscateAssemblyAttribute(bool assemblyIsPrivate)
    {
      this.m_assemblyIsPrivate = assemblyIsPrivate;
      this.m_stripAfterObfuscation = true;
    }

    public bool AssemblyIsPrivate
    {
      get
      {
        return this.m_assemblyIsPrivate;
      }
    }

    public bool StripAfterObfuscation
    {
      get
      {
        return this.m_stripAfterObfuscation;
      }
      set
      {
        this.m_stripAfterObfuscation = value;
      }
    }
  }
}
