// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.XmlBufferReader
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.IO;
using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class XmlBufferReader
  {
    private static readonly byte[] EmptyByteArray = new byte[0];
    private static readonly XmlBufferReader EmptyBufferReader = new XmlBufferReader();
    private const int BufferAllocation = 128;
    private byte[] buffer;
    private char[] chars;
    private byte[] guid;
    private int offset;
    private int offsetMax;
    private Stream stream;
    private byte[] streamBuffer;
    private int windowOffsetMax;

    public static XmlBufferReader Empty
    {
      get
      {
        return XmlBufferReader.EmptyBufferReader;
      }
    }

    public byte[] Buffer
    {
      get
      {
        return this.buffer;
      }
    }

    public bool IsStreamed
    {
      get
      {
        return this.stream != null;
      }
    }

    public bool EndOfFile
    {
      get
      {
        if (this.offset == this.offsetMax)
          return !this.TryEnsureByte();
        return false;
      }
    }

    public int Offset
    {
      get
      {
        return this.offset;
      }
      set
      {
        this.offset = value;
      }
    }

    public void SetBuffer(Stream iStream)
    {
      if (this.streamBuffer == null)
        this.streamBuffer = new byte[128];
      this.SetBuffer(iStream, this.streamBuffer, 0, 0);
      this.windowOffsetMax = this.streamBuffer.Length;
    }

    private void SetBuffer(Stream iStream, byte[] iBuffer, int iOffset, int count)
    {
      this.stream = iStream;
      this.buffer = iBuffer;
      this.offset = iOffset;
      this.offsetMax = iOffset + count;
    }

    public void Close()
    {
      if (this.streamBuffer != null && this.streamBuffer.Length > 4096)
        this.streamBuffer = (byte[]) null;
      if (this.stream != null)
      {
        this.stream.Dispose();
        this.stream = (Stream) null;
      }
      this.buffer = XmlBufferReader.EmptyByteArray;
      this.offset = 0;
      this.offsetMax = 0;
      this.windowOffsetMax = 0;
    }

    public byte GetByte()
    {
      int offset = this.offset;
      if (offset < this.offsetMax)
        return this.buffer[offset];
      return this.GetByteHard();
    }

    public void SkipByte()
    {
      this.Advance(1);
    }

    private byte GetByteHard()
    {
      this.EnsureByte();
      return this.buffer[this.offset];
    }

    public byte[] GetBuffer(int count, out int outOffset)
    {
      outOffset = this.offset;
      if (outOffset <= this.offsetMax - count)
        return this.buffer;
      return this.GetBufferHard(count, out outOffset);
    }

    public byte[] GetBuffer(int count, out int outOffset, out int outOffsetMax)
    {
      outOffset = this.offset;
      if (outOffset <= this.offsetMax - count)
      {
        outOffsetMax = this.offset + count;
      }
      else
      {
        this.TryEnsureBytes(Math.Min(count, this.windowOffsetMax - outOffset));
        outOffsetMax = this.offsetMax;
      }
      return this.buffer;
    }

    public byte[] GetBuffer(out int outOffset, out int outOffsetMax)
    {
      outOffset = this.offset;
      outOffsetMax = this.offsetMax;
      return this.buffer;
    }

    private byte[] GetBufferHard(int count, out int outOffset)
    {
      outOffset = this.offset;
      this.EnsureBytes(count);
      return this.buffer;
    }

    private void EnsureByte()
    {
      if (!this.TryEnsureByte())
        throw new EndOfStreamException();
    }

    private void EnsureBytes(int count)
    {
      if (!this.TryEnsureBytes(count))
        throw new Exception("XmlExceptionHelper.ThrowUnexpectedEndOfFile(this.reader)");
    }

    private bool TryEnsureByte()
    {
      if (this.stream == null)
        return false;
      if (this.offsetMax >= this.windowOffsetMax)
        throw new Exception("ThrowMaxBytesPerReadExceeded");
      if (this.offsetMax >= this.buffer.Length)
        return this.TryEnsureBytes(1);
      int num = this.stream.ReadByte();
      if (num == -1)
        return false;
      this.buffer[this.offsetMax++] = (byte) num;
      return true;
    }

    private bool TryEnsureBytes(int count)
    {
      if (this.stream == null)
        return false;
      if (this.offset > int.MaxValue - count)
        throw new Exception("XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this.reader, this.windowOffsetMax - this.windowOffset);");
      int val1 = this.offset + count;
      if (val1 < this.offsetMax)
        return true;
      if (val1 > this.windowOffsetMax)
        throw new Exception("XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this.reader, this.windowOffsetMax - this.windowOffset);");
      if (val1 > this.buffer.Length)
      {
        byte[] numArray = new byte[Math.Max(val1, this.buffer.Length * 2)];
        System.Buffer.BlockCopy((Array) this.buffer, 0, (Array) numArray, 0, this.offsetMax);
        this.buffer = numArray;
        this.streamBuffer = numArray;
      }
      int num;
      for (int count1 = val1 - this.offsetMax; count1 > 0; count1 -= num)
      {
        num = this.stream.Read(this.buffer, this.offsetMax, count1);
        if (num == 0)
          return false;
        this.offsetMax += num;
      }
      return true;
    }

    public void Advance(int count)
    {
      this.offset += count;
    }

    public void SetWindow(int iWindowOffset, int iWindowLength)
    {
      if (iWindowOffset > int.MaxValue - iWindowLength)
        iWindowLength = int.MaxValue - iWindowOffset;
      if (this.offset != iWindowOffset)
      {
        System.Buffer.BlockCopy((Array) this.buffer, this.offset, (Array) this.buffer, iWindowOffset, this.offsetMax - this.offset);
        this.offsetMax = iWindowOffset + (this.offsetMax - this.offset);
        this.offset = iWindowOffset;
      }
      this.windowOffsetMax = Math.Max(iWindowOffset + iWindowLength, this.offsetMax);
    }

    public int ReadBytes(int count)
    {
      int offset = this.offset;
      if (offset > this.offsetMax - count)
        this.EnsureBytes(count);
      this.offset += count;
      return offset;
    }

    public int ReadMultiByteUInt31()
    {
      int num1 = (int) this.GetByte();
      this.Advance(1);
      if ((num1 & 128) == 0)
        return num1;
      int num2 = num1 & (int) sbyte.MaxValue;
      int num3 = (int) this.GetByte();
      this.Advance(1);
      int num4 = num2 | (num3 & (int) sbyte.MaxValue) << 7;
      if ((num3 & 128) == 0)
        return num4;
      int num5 = (int) this.GetByte();
      this.Advance(1);
      int num6 = num4 | (num5 & (int) sbyte.MaxValue) << 14;
      if ((num5 & 128) == 0)
        return num6;
      int num7 = (int) this.GetByte();
      this.Advance(1);
      int num8 = num6 | (num7 & (int) sbyte.MaxValue) << 21;
      if ((num7 & 128) == 0)
        return num8;
      int num9 = (int) this.GetByte();
      this.Advance(1);
      int num10 = num8 | num9 << 28;
      if ((uint) (num9 & 248) > 0U)
        throw new Exception("XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);");
      return num10;
    }

    public int ReadUInt8()
    {
      byte num = this.GetByte();
      this.Advance(1);
      return (int) num;
    }

    public int ReadInt8()
    {
      return (int) (sbyte) this.ReadUInt8();
    }

    public int ReadUInt16()
    {
      int outOffset;
      byte[] buffer = this.GetBuffer(2, out outOffset);
      int num = (int) buffer[outOffset] + ((int) buffer[outOffset + 1] << 8);
      this.Advance(2);
      return num;
    }

    public int ReadInt16()
    {
      return (int) (short) this.ReadUInt16();
    }

    public int ReadInt32()
    {
      int outOffset;
      byte[] buffer = this.GetBuffer(4, out outOffset);
      byte num1 = buffer[outOffset];
      byte num2 = buffer[outOffset + 1];
      byte num3 = buffer[outOffset + 2];
      byte num4 = buffer[outOffset + 3];
      this.Advance(4);
      return ((((int) num4 << 8) + (int) num3 << 8) + (int) num2 << 8) + (int) num1;
    }

    public int ReadUInt31()
    {
      int num = this.ReadInt32();
      if (num < 0)
        throw new Exception("XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);");
      return num;
    }

    public long ReadInt64()
    {
      return ((long) (uint) this.ReadInt32() << 32) + (long) (uint) this.ReadInt32();
    }

    public UniqueId ReadUniqueId()
    {
      int outOffset;
      UniqueId uniqueId = new UniqueId(this.GetBuffer(16, out outOffset), outOffset);
      this.Advance(16);
      return uniqueId;
    }

    public DateTime ReadDateTime()
    {
      return DateTime.FromBinary(this.ReadInt64());
    }

    public TimeSpan ReadTimeSpan()
    {
      return TimeSpan.FromTicks(this.ReadInt64());
    }

    public Guid ReadGuid()
    {
      int outOffset;
      this.GetBuffer(16, out outOffset);
      Guid guid = this.GetGuid(outOffset);
      this.Advance(16);
      return guid;
    }

    public string ReadUTF8String(int length)
    {
      int outOffset;
      this.GetBuffer(length, out outOffset);
      char[] charBuffer = this.GetCharBuffer(length);
      int chars = this.GetChars(outOffset, length, charBuffer);
      string str = new string(charBuffer, 0, chars);
      this.Advance(length);
      return str;
    }

    private char[] GetCharBuffer(int count)
    {
      if (count > 1024)
        return new char[count];
      if (this.chars == null || this.chars.Length < count)
        this.chars = new char[count];
      return this.chars;
    }

    private int GetChars(int iOffset, int iLength, char[] iChars)
    {
      byte[] buffer = this.buffer;
      for (int charOffset = 0; charOffset < iLength; ++charOffset)
      {
        byte num = buffer[iOffset + charOffset];
        if (num >= (byte) 128)
          return charOffset + XmlConverter.ToChars(buffer, iOffset + charOffset, iLength - charOffset, iChars, charOffset);
        iChars[charOffset] = (char) num;
      }
      return iLength;
    }

    private int GetChars(int iOffset, int iLength, char[] iChars, int iCharOffset)
    {
      byte[] buffer = this.buffer;
      for (int index = 0; index < iLength; ++index)
      {
        byte num = buffer[this.offset + index];
        if (num >= (byte) 128)
          return index + XmlConverter.ToChars(buffer, this.offset + index, iLength - index, iChars, iCharOffset + index);
        iChars[iCharOffset + index] = (char) num;
      }
      return iLength;
    }

    public string GetString(int iOffset, int iLength)
    {
      char[] charBuffer = this.GetCharBuffer(iLength);
      int chars = this.GetChars(iOffset, iLength, charBuffer);
      return new string(charBuffer, 0, chars);
    }

    public string GetUnicodeString(int iOffset, int iLength)
    {
      return XmlConverter.ToStringUnicode(this.buffer, iOffset, iLength);
    }

    public string GetString(int iOffset, int iLength, XmlNameTable nameTable)
    {
      char[] charBuffer = this.GetCharBuffer(iLength);
      int chars = this.GetChars(iOffset, iLength, charBuffer);
      return nameTable.Add(charBuffer, 0, chars);
    }

    public int GetEscapedChars(int iOffset, int iLength, char[] iChars)
    {
      byte[] buffer = this.buffer;
      int iCharOffset = 0;
      int iOffset1 = iOffset;
      int num1 = iOffset + iLength;
      int num2;
      while (true)
      {
        while (iOffset < num1 && this.IsAttrChar((int) this.buffer[this.offset]))
          ++iOffset;
        num2 = iCharOffset + this.GetChars(iOffset1, iOffset - iOffset1, iChars, iCharOffset);
        if (iOffset != num1)
        {
          int iOffset2 = iOffset;
          switch (buffer[iOffset])
          {
            case 9:
            case 10:
              char[] chArray1 = iChars;
              int index1 = num2;
              iCharOffset = index1 + 1;
              chArray1[index1] = ' ';
              ++iOffset;
              iOffset1 = iOffset;
              break;
            case 38:
              while (iOffset < num1 && buffer[iOffset] != (byte) 59)
                ++iOffset;
              ++iOffset;
              int charEntity = this.GetCharEntity(iOffset2, iOffset - iOffset2);
              iOffset1 = iOffset;
              if (charEntity <= (int) ushort.MaxValue)
              {
                char[] chArray2 = iChars;
                int index2 = num2;
                iCharOffset = index2 + 1;
                int num3 = (int) (ushort) charEntity;
                chArray2[index2] = (char) num3;
                break;
              }
              goto label_8;
            default:
              char[] chArray3 = iChars;
              int index3 = num2;
              iCharOffset = index3 + 1;
              chArray3[index3] = ' ';
              ++iOffset;
              if (iOffset < num1 && buffer[this.offset] == (byte) 10)
                ++iOffset;
              iOffset1 = iOffset;
              break;
          }
        }
        else
          goto label_16;
      }
label_8:
      throw new Exception("Not a good Character");
label_16:
      return num2;
    }

    private bool IsAttrChar(int ch)
    {
      switch (ch)
      {
        case 9:
        case 10:
        case 13:
        case 38:
          return false;
        default:
          return true;
      }
    }

    public string GetEscapedString(int iOffset, int iLength)
    {
      char[] charBuffer = this.GetCharBuffer(iLength);
      int escapedChars = this.GetEscapedChars(iOffset, iLength, charBuffer);
      return new string(charBuffer, 0, escapedChars);
    }

    public string GetEscapedString(int iOffset, int length, XmlNameTable nameTable)
    {
      char[] charBuffer = this.GetCharBuffer(length);
      int escapedChars = this.GetEscapedChars(iOffset, length, charBuffer);
      return nameTable.Add(charBuffer, 0, escapedChars);
    }

    public int GetCharEntity(int iOffset, int length)
    {
      if (length < 3)
        throw new Exception(" XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      byte[] buffer = this.buffer;
      switch (this.buffer[this.offset + 1])
      {
        case 35:
          if (buffer[iOffset + 2] == (byte) 120)
            return this.GetHexCharEntity(iOffset, length);
          return this.GetDecimalCharEntity(iOffset, length);
        case 97:
          if (buffer[iOffset + 2] == (byte) 109)
            return this.GetAmpersandCharEntity(iOffset, length);
          return this.GetApostropheCharEntity(iOffset, length);
        case 103:
          return this.GetGreaterThanCharEntity(length);
        case 108:
          return this.GetLessThanCharEntity(iOffset, length);
        case 113:
          return this.GetQuoteCharEntity(iOffset, length);
        default:
          throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      }
    }

    private int GetLessThanCharEntity(int iOffset, int length)
    {
      byte[] buffer = this.buffer;
      if (length != 4 || buffer[this.offset + 1] != (byte) 108 || buffer[this.offset + 2] != (byte) 116)
        throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      return 60;
    }

    private int GetGreaterThanCharEntity(int length)
    {
      byte[] buffer = this.buffer;
      if (length != 4 || buffer[this.offset + 1] != (byte) 103 || buffer[this.offset + 2] != (byte) 116)
        throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      return 62;
    }

    private int GetQuoteCharEntity(int iOffset, int length)
    {
      byte[] buffer = this.buffer;
      if (length != 6 || buffer[this.offset + 1] != (byte) 113 || (buffer[this.offset + 2] != (byte) 117 || buffer[this.offset + 3] != (byte) 111) || buffer[this.offset + 4] != (byte) 116)
        throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      return 34;
    }

    private int GetAmpersandCharEntity(int iOffset, int length)
    {
      byte[] buffer = this.buffer;
      if (length != 5 || buffer[this.offset + 1] != (byte) 97 || buffer[this.offset + 2] != (byte) 109 || buffer[this.offset + 3] != (byte) 112)
        throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      return 38;
    }

    private int GetApostropheCharEntity(int iOffset, int length)
    {
      byte[] buffer = this.buffer;
      if (length != 6 || buffer[this.offset + 1] != (byte) 97 || (buffer[this.offset + 2] != (byte) 112 || buffer[this.offset + 3] != (byte) 111) || buffer[this.offset + 4] != (byte) 115)
        throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      return 39;
    }

    private int GetDecimalCharEntity(int iOffset, int length)
    {
      byte[] buffer = this.buffer;
      int num1 = 0;
      for (int index = 2; index < length - 1; ++index)
      {
        byte num2 = buffer[this.offset + index];
        if (num2 < (byte) 48 || num2 > (byte) 57)
          throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
        num1 = num1 * 10 + ((int) num2 - 48);
        if (num1 > 1114111)
          throw new Exception("XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      }
      return num1;
    }

    public double GetDouble(int iOffset)
    {
      return BitConverter.ToDouble(this.Buffer, iOffset);
    }

    public float GetSingle(int iOffset)
    {
      return BitConverter.ToSingle(this.buffer, iOffset);
    }

    public Decimal GetDecimal(int iOffset)
    {
      byte[] buffer = this.buffer;
      return new Decimal(new int[4]
      {
        (int) buffer[iOffset] | (int) buffer[1 + iOffset] << 8 | (int) buffer[2 + iOffset] << 16 | (int) buffer[3 + iOffset] << 24,
        (int) buffer[4 + iOffset] | (int) buffer[5 + iOffset] << 8 | (int) buffer[6 + iOffset] << 16 | (int) buffer[7 + iOffset] << 24,
        (int) buffer[8 + iOffset] | (int) buffer[9 + iOffset] << 8 | (int) buffer[10 + iOffset] << 16 | (int) buffer[11 + iOffset] << 24,
        (int) buffer[12 + iOffset] | (int) buffer[13 + iOffset] << 8 | (int) buffer[14 + iOffset] << 16 | (int) buffer[15 + iOffset] << 24
      });
    }

    private int GetHexCharEntity(int iOffset, int length)
    {
      byte[] buffer = this.buffer;
      int num1 = 0;
      for (int index = 3; index < length - 1; ++index)
      {
        byte num2 = buffer[this.offset + index];
        int num3;
        if (num2 >= (byte) 48 && num2 <= (byte) 57)
          num3 = (int) num2 - 48;
        else if (num2 >= (byte) 97 && num2 <= (byte) 102)
        {
          num3 = 10 + ((int) num2 - 97);
        }
        else
        {
          if (num2 < (byte) 65 || num2 > (byte) 70)
            throw new Exception(" XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
          num3 = 10 + ((int) num2 - 65);
        }
        num1 = num1 * 16 + num3;
        if (num1 > 1114111)
          throw new Exception(" XmlExceptionHelper.ThrowInvalidCharRef(this.reader);");
      }
      return num1;
    }

    public static bool IsWhitespace(char ch)
    {
      if (ch > ' ')
        return false;
      return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n';
    }

    public bool Equals2(int offset1, int length1, byte[] buffer2)
    {
      int length = buffer2.Length;
      if (length1 != length)
        return false;
      byte[] buffer = this.buffer;
      for (int index = 0; index < length1; ++index)
      {
        if ((int) buffer[offset1 + index] != (int) buffer2[index])
          return false;
      }
      return true;
    }

    public bool Equals2(
      int offset1,
      int length1,
      XmlBufferReader bufferReader2,
      int offset2,
      int length2)
    {
      if (length1 != length2)
        return false;
      byte[] buffer1 = this.buffer;
      byte[] buffer2 = bufferReader2.buffer;
      for (int index = 0; index < length1; ++index)
      {
        if ((int) buffer1[offset1 + index] != (int) buffer2[offset2 + index])
          return false;
      }
      return true;
    }

    public bool Equals2(int offset1, int length1, int offset2, int length2)
    {
      if (length1 != length2)
        return false;
      if (offset1 == offset2)
        return true;
      byte[] buffer = this.buffer;
      for (int index = 0; index < length1; ++index)
      {
        if ((int) buffer[offset1 + index] != (int) buffer[offset2 + index])
          return false;
      }
      return true;
    }

    public int Compare(int offset1, int length1, int offset2, int length2)
    {
      byte[] buffer = this.buffer;
      int num1 = Math.Min(length1, length2);
      for (int index = 0; index < num1; ++index)
      {
        int num2 = (int) buffer[offset1 + index] - (int) buffer[offset2 + index];
        if ((uint) num2 > 0U)
          return num2;
      }
      return length1 - length2;
    }

    public byte GetByte(int iOffset)
    {
      return this.buffer[iOffset];
    }

    public int GetInt8(int iOffset)
    {
      return (int) (sbyte) this.GetByte(iOffset);
    }

    public int GetInt16(int iOffset)
    {
      byte[] buffer = this.buffer;
      return (int) (short) ((int) buffer[iOffset] + ((int) buffer[iOffset + 1] << 8));
    }

    public int GetInt32(int iOffset)
    {
      return BitConverter.ToInt32(this.buffer, iOffset);
    }

    public long GetInt64(int iOffset)
    {
      return BitConverter.ToInt64(this.buffer, iOffset);
    }

    public ulong GetUInt64(int iOffset)
    {
      return (ulong) this.GetInt64(iOffset);
    }

    public UniqueId GetUniqueId(int iOffset)
    {
      return new UniqueId(this.buffer, iOffset);
    }

    public Guid GetGuid(int currentOffset)
    {
      if (this.guid == null)
        this.guid = new byte[16];
      System.Buffer.BlockCopy((Array) this.buffer, currentOffset, (Array) this.guid, 0, this.guid.Length);
      return new Guid(this.guid);
    }

    public void GetBase64(int srcOffset, byte[] outBuffer, int dstOffset, int count)
    {
      System.Buffer.BlockCopy((Array) this.buffer, srcOffset, (Array) outBuffer, dstOffset, count);
    }

    public void SkipNodeType()
    {
      this.SkipByte();
    }
  }
}
