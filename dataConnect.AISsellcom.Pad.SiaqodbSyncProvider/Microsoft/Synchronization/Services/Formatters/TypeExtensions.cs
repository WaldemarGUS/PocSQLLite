// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.TypeExtensions
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;

namespace Microsoft.Synchronization.Services.Formatters {
    internal static class TypeExtensions
  {
    public static bool IsGenericType(this Type type)
    {
      return type.IsGenericType;
    }

    public static bool IsEnum(this Type type)
    {
      return type.IsEnum;
    }

    public static bool IsClass(this Type type)
    {
      return type.IsClass;
    }

    public static bool IsPrimitive(this Type type)
    {
      return type.IsPrimitive;
    }
  }
}
