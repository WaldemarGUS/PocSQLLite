// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.Utilities.JSerializer
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Newtonsoft.Json;
using System;
using System.Text;

namespace SiaqodbSyncProvider.Utilities {
    internal class JSerializer
    {
        public static byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }

        public static object Deserialize(Type type, byte[] objectBytes)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(objectBytes).TrimEnd(new char[1]), type);
        }

        public static object Deserialize(Type type, string jsonString)
        {
            return JsonConvert.DeserializeObject(jsonString.TrimEnd(new char[1]), type);
        }
    }
}
