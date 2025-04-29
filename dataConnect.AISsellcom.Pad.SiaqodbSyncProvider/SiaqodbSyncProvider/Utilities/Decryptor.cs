// Decompiled with JetBrains decompiler
// Type: SiaqodbSyncProvider.Utilities.Decryptor
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SiaqodbSyncProvider.Utilities {
    internal class Decryptor
  {
    public static string DecryptRJ128(string prm_key, string prm_iv, string prm_text_to_decrypt)
    {
      string s = prm_text_to_decrypt;
      AesManaged aesManaged = new AesManaged();
      aesManaged.KeySize = 128;
      aesManaged.BlockSize = 128;
      byte[] bytes1 = Encoding.UTF8.GetBytes(prm_key);
      byte[] bytes2 = Encoding.UTF8.GetBytes(prm_iv);
      ICryptoTransform decryptor = aesManaged.CreateDecryptor(bytes1, bytes2);
      byte[] buffer = Convert.FromBase64String(s);
      byte[] numArray = new byte[buffer.Length];
      new CryptoStream((Stream) new MemoryStream(buffer), decryptor, CryptoStreamMode.Read).Read(numArray, 0, numArray.Length);
      return Encoding.UTF8.GetString(numArray, 0, numArray.Length).TrimEnd(new char[1]);
    }
  }
}
