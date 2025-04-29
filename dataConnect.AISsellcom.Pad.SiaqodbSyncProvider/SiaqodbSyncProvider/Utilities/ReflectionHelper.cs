// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.Utilities.ReflectionHelper
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace SiaqodbSyncProvider.Utilities {
    internal class ReflectionHelper
  {
    public static string GetDiscoveringTypeName(Type type)
    {
      return type.Namespace + "." + type.Name + ", " + type.Assembly.GetName().Name;
    }

    public static Type GetTypeByDiscoveringName(string typeName)
    {
      return Type.GetType(typeName);
    }
  }
}
