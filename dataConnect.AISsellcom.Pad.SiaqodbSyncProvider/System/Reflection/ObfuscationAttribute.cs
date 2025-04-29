// Decompiled with JetBrains decompiler
// Type: System.Reflection.ObfuscationAttribute
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System.Runtime.InteropServices;

namespace System.Reflection {
    [ComVisible(true)]
  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
  internal sealed class ObfuscationAttribute : Attribute
  {
    private bool m_applyToMembers;
    private bool m_exclude;
    private string m_feature;
    private bool m_stripAfterObfuscation;

    public ObfuscationAttribute()
    {
      this.m_applyToMembers = true;
      this.m_exclude = true;
      this.m_feature = "all";
      this.m_stripAfterObfuscation = true;
    }

    public bool ApplyToMembers
    {
      get
      {
        return this.m_applyToMembers;
      }
      set
      {
        this.m_applyToMembers = value;
      }
    }

    public bool Exclude
    {
      get
      {
        return this.m_exclude;
      }
      set
      {
        this.m_exclude = value;
      }
    }

    public string Feature
    {
      get
      {
        return this.m_feature;
      }
      set
      {
        this.m_feature = value;
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
