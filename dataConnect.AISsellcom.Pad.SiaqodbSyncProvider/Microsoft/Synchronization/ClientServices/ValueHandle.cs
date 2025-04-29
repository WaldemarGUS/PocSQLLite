// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.ClientServices.ValueHandle
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using System;
using System.Text;
using System.Xml;

namespace Microsoft.Synchronization.ClientServices {
    public class ValueHandle {
        private static string[] constStrings = new string[6]
        {
      "string",
      "number",
      "array",
      "object",
      "boolean",
      "null"
        };
        private XmlBufferReader bufferReader;
        private ValueHandleType type;
        private int offset;
        private int length;

        public ValueHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
            this.type = ValueHandleType.Empty;
        }

        public void SetConstantValue(ValueHandleConstStringType constStringType)
        {
            this.type = ValueHandleType.ConstString;
            this.offset = (int)constStringType;
        }

        public void SetValue(ValueHandleType valueHandleType)
        {
            this.type = valueHandleType;
        }

        public void SetDictionaryValue(int key)
        {
            this.SetValue(ValueHandleType.Dictionary, key, 0);
        }

        public void SetCharValue(int ch)
        {
            this.SetValue(ValueHandleType.Char, ch, 0);
        }

        public void SetQNameValue(int prefix, int key)
        {
            this.SetValue(ValueHandleType.QName, key, prefix);
        }

        public void SetValue(ValueHandleType iType, int iOffset, int iLength)
        {
            this.type = iType;
            this.offset = iOffset;
            this.length = iLength;
        }

        public bool IsWhitespace()
        {
            switch (this.type)
            {
                case ValueHandleType.True:
                case ValueHandleType.False:
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                    return false;
                case ValueHandleType.UTF8:
                    throw new NotSupportedException();
                case ValueHandleType.EscapedUTF8:
                    throw new NotSupportedException();
                case ValueHandleType.Dictionary:
                    throw new NotSupportedException();
                case ValueHandleType.Char:
                    int num = this.GetChar();
                    if (num > (int)ushort.MaxValue)
                        return false;
                    return XmlConverter.IsWhitespace((char)num);
                case ValueHandleType.Unicode:
                    throw new NotSupportedException();
                case ValueHandleType.ConstString:
                    return ValueHandle.constStrings[this.offset].Length == 0;
                default:
                    return this.length == 0;
            }
        }

        public Type ToType()
        {
            switch (this.type)
            {
                case ValueHandleType.Empty:
                case ValueHandleType.UTF8:
                case ValueHandleType.EscapedUTF8:
                case ValueHandleType.Dictionary:
                case ValueHandleType.Char:
                case ValueHandleType.Unicode:
                case ValueHandleType.QName:
                case ValueHandleType.ConstString:
                    return typeof(string);
                case ValueHandleType.True:
                case ValueHandleType.False:
                    return typeof(bool);
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return typeof(int);
                case ValueHandleType.Int64:
                    return typeof(long);
                case ValueHandleType.UInt64:
                    return typeof(ulong);
                case ValueHandleType.Single:
                    return typeof(float);
                case ValueHandleType.Double:
                    return typeof(double);
                case ValueHandleType.Decimal:
                    return typeof(Decimal);
                case ValueHandleType.DateTime:
                    return typeof(DateTime);
                case ValueHandleType.TimeSpan:
                    return typeof(TimeSpan);
                case ValueHandleType.Guid:
                    return typeof(Guid);
                case ValueHandleType.UniqueId:
                    return typeof(UniqueId);
                case ValueHandleType.Base64:
                    return typeof(byte[]);
                case ValueHandleType.List:
                    return typeof(object[]);
                default:
                    throw new InvalidOperationException();
            }
        }

        public bool ToBoolean()
        {
            switch (this.type)
            {
                case ValueHandleType.True:
                    return true;
                case ValueHandleType.False:
                    return false;
                case ValueHandleType.Int8:
                    switch (this.GetInt8())
                    {
                        case 0:
                            return false;
                        case 1:
                            return true;
                    }
                    break;
                case ValueHandleType.UTF8:
                    return XmlConverter.ToBoolean(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToBoolean(this.GetString());
        }

        public int ToInt()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type == ValueHandleType.Int8)
                return this.GetInt8();
            if (type == ValueHandleType.Int16)
                return this.GetInt16();
            if (type == ValueHandleType.Int32)
                return this.GetInt32();
            if (type == ValueHandleType.Int64)
            {
                long int64 = this.GetInt64();
                if (int64 >= (long)int.MinValue && int64 <= (long)int.MaxValue)
                    return (int)int64;
            }
            if (type == ValueHandleType.UInt64)
            {
                ulong uint64 = this.GetUInt64();
                if (uint64 <= (ulong)int.MaxValue)
                    return (int)uint64;
            }
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToInt32(this.bufferReader.Buffer, this.offset, this.length);
            return XmlConverter.ToInt32(this.GetString());
        }

        public long ToLong()
        {
            switch (this.type)
            {
                case ValueHandleType.Zero:
                    return 0;
                case ValueHandleType.One:
                    return 1;
                case ValueHandleType.Int8:
                    return (long)this.GetInt8();
                case ValueHandleType.Int16:
                    return (long)this.GetInt16();
                case ValueHandleType.Int32:
                    return (long)this.GetInt32();
                case ValueHandleType.Int64:
                    return this.GetInt64();
                case ValueHandleType.UInt64:
                    return (long)this.GetUInt64();
                case ValueHandleType.UTF8:
                    return XmlConverter.ToInt64(this.bufferReader.Buffer, this.offset, this.length);
                default:
                    return XmlConverter.ToInt64(this.GetString());
            }
        }

        public ulong ToULong()
        {
            ValueHandleType type = this.type;
            switch (type)
            {
                case ValueHandleType.Zero:
                    return 0;
                case ValueHandleType.One:
                    return 1;
                default:
                    if (type >= ValueHandleType.Int8 && type <= ValueHandleType.Int64)
                    {
                        long num = this.ToLong();
                        if (num >= 0L)
                            return (ulong)num;
                    }
                    if (type == ValueHandleType.UInt64)
                        return this.GetUInt64();
                    if (type == ValueHandleType.UTF8)
                        return XmlConverter.ToUInt64(this.bufferReader.Buffer, this.offset, this.length);
                    return XmlConverter.ToUInt64(this.GetString());
            }
        }

        public float ToSingle()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Single)
                return this.GetSingle();
            if (type == ValueHandleType.Double)
            {
                double d = this.GetDouble();
                if (d >= -3.40282346638529E+38 && d <= 3.40282346638529E+38 || double.IsInfinity(d) || double.IsNaN(d))
                    return (float)d;
            }
            if (type == ValueHandleType.Zero)
                return 0.0f;
            if (type == ValueHandleType.One)
                return 1f;
            if (type == ValueHandleType.Int8)
                return (float)this.GetInt8();
            if (type == ValueHandleType.Int16)
                return (float)this.GetInt16();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToSingle(this.bufferReader.Buffer, this.offset, this.length);
            return XmlConverter.ToSingle(this.GetString());
        }

        public double ToDouble()
        {
            switch (this.type)
            {
                case ValueHandleType.Zero:
                    return 0.0;
                case ValueHandleType.One:
                    return 1.0;
                case ValueHandleType.Int8:
                    return (double)this.GetInt8();
                case ValueHandleType.Int16:
                    return (double)this.GetInt16();
                case ValueHandleType.Int32:
                    return (double)this.GetInt32();
                case ValueHandleType.Single:
                    return (double)this.GetSingle();
                case ValueHandleType.Double:
                    return this.GetDouble();
                case ValueHandleType.UTF8:
                    return XmlConverter.ToDouble(this.bufferReader.Buffer, this.offset, this.length);
                default:
                    return XmlConverter.ToDouble(this.GetString());
            }
        }

        public Decimal ToDecimal()
        {
            ValueHandleType type = this.type;
            switch (type)
            {
                case ValueHandleType.Zero:
                    return new Decimal(0);
                case ValueHandleType.One:
                    return new Decimal(1);
                case ValueHandleType.Decimal:
                    return this.GetDecimal();
                default:
                    if (type >= ValueHandleType.Int8 && type <= ValueHandleType.Int64)
                        return (Decimal)this.ToLong();
                    if (type == ValueHandleType.UInt64)
                        return (Decimal)this.GetUInt64();
                    if (type == ValueHandleType.UTF8)
                        return XmlConverter.ToDecimal(this.bufferReader.Buffer, this.offset, this.length);
                    return XmlConverter.ToDecimal(this.GetString());
            }
        }

        public DateTime ToDateTime()
        {
            if (this.type == ValueHandleType.DateTime)
                return XmlConverter.ToDateTime(this.GetInt64());
            if (this.type == ValueHandleType.UTF8)
                return XmlConverter.ToDateTime(this.bufferReader.Buffer, this.offset, this.length);
            return XmlConverter.ToDateTime(this.GetString());
        }

        public UniqueId ToUniqueId()
        {
            if (this.type == ValueHandleType.UniqueId)
                return this.GetUniqueId();
            if (this.type == ValueHandleType.UTF8)
                return XmlConverter.ToUniqueId(this.bufferReader.Buffer, this.offset, this.length);
            return XmlConverter.ToUniqueId(this.GetString());
        }

        public TimeSpan ToTimeSpan()
        {
            if (this.type == ValueHandleType.TimeSpan)
                return new TimeSpan(this.GetInt64());
            if (this.type == ValueHandleType.UTF8)
                return XmlConverter.ToTimeSpan(this.bufferReader.Buffer, this.offset, this.length);
            return XmlConverter.ToTimeSpan(this.GetString());
        }

        public Guid ToGuid()
        {
            if (this.type == ValueHandleType.Guid)
                return this.GetGuid();
            if (this.type == ValueHandleType.UTF8)
                return XmlConverter.ToGuid(this.bufferReader.Buffer, this.offset, this.length);
            return XmlConverter.ToGuid(this.GetString());
        }

        public override string ToString()
        {
            return this.GetString();
        }

        public byte[] ToByteArray()
        {
            if (this.type != ValueHandleType.Base64)
                throw new NotSupportedException();
            byte[] buffer = new byte[this.length];
            this.GetBase64(buffer, 0, this.length);
            return buffer;
        }

        public string GetString()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.UTF8)
                return this.GetCharsText();
            switch (type)
            {
                case ValueHandleType.Empty:
                    return string.Empty;
                case ValueHandleType.True:
                    return "true";
                case ValueHandleType.False:
                    return "false";
                case ValueHandleType.Zero:
                    return "0";
                case ValueHandleType.One:
                    return "1";
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return XmlConverter.ToString(this.ToInt());
                case ValueHandleType.Int64:
                    return XmlConverter.ToString(this.GetInt64());
                case ValueHandleType.UInt64:
                    return XmlConverter.ToString(this.GetUInt64());
                case ValueHandleType.Single:
                    return XmlConverter.ToString(this.GetSingle());
                case ValueHandleType.Double:
                    return XmlConverter.ToString(this.GetDouble());
                case ValueHandleType.Decimal:
                    return XmlConverter.ToString(this.GetDecimal());
                case ValueHandleType.DateTime:
                    return XmlConverter.ToString(this.ToDateTime());
                case ValueHandleType.TimeSpan:
                    return XmlConverter.ToString(this.ToTimeSpan());
                case ValueHandleType.Guid:
                    return XmlConverter.ToString(this.ToGuid());
                case ValueHandleType.UniqueId:
                    return XmlConverter.ToString(this.ToUniqueId());
                case ValueHandleType.UTF8:
                    return this.GetCharsText();
                case ValueHandleType.EscapedUTF8:
                    return this.GetEscapedCharsText();
                case ValueHandleType.Base64:
                    throw new NotSupportedException();
                case ValueHandleType.List:
                    throw new NotSupportedException();
                case ValueHandleType.Char:
                    return this.GetCharText();
                case ValueHandleType.Unicode:
                    return this.GetUnicodeCharsText();
                case ValueHandleType.ConstString:
                    return ValueHandle.constStrings[this.offset];
                default:
                    throw new InvalidOperationException();
            }
        }

        public bool Equals2(string str, bool checkLower)
        {
            if (this.type != ValueHandleType.UTF8)
                return this.GetString() == str;
            if (this.length != str.Length)
                return false;
            byte[] buffer = this.bufferReader.Buffer;
            for (int index = 0; index < this.length; ++index)
            {
                byte num = buffer[index + this.offset];
                if ((int)num != (int)str[index] && (!checkLower || (int)char.ToLowerInvariant((char)num) != (int)str[index]))
                    return false;
            }
            return true;
        }

        public object ToObject()
        {
            switch (this.type)
            {
                case ValueHandleType.Empty:
                case ValueHandleType.UTF8:
                case ValueHandleType.EscapedUTF8:
                case ValueHandleType.Dictionary:
                case ValueHandleType.Char:
                case ValueHandleType.Unicode:
                case ValueHandleType.ConstString:
                    return (object)this.ToString();
                case ValueHandleType.True:
                case ValueHandleType.False:
                    return (object)(this.ToBoolean() ? 1 : 0);
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return (object)this.ToInt();
                case ValueHandleType.Int64:
                    return (object)this.ToLong();
                case ValueHandleType.UInt64:
                    return (object)this.GetUInt64();
                case ValueHandleType.Single:
                    return (object)this.ToSingle();
                case ValueHandleType.Double:
                    return (object)this.ToDouble();
                case ValueHandleType.Decimal:
                    return (object)this.ToDecimal();
                case ValueHandleType.DateTime:
                    return (object)this.ToDateTime();
                case ValueHandleType.TimeSpan:
                    return (object)this.ToTimeSpan();
                case ValueHandleType.Guid:
                    return (object)this.ToGuid();
                case ValueHandleType.UniqueId:
                    return (object)this.ToUniqueId();
                case ValueHandleType.Base64:
                    return (object)this.ToByteArray();
                case ValueHandleType.List:
                    throw new NotSupportedException("ToObject List");
                default:
                    throw new InvalidOperationException();
            }
        }

        public bool TryReadBase64(byte[] buffer, int iOffset, int count, out int actual)
        {
            if (this.type == ValueHandleType.Base64)
            {
                actual = Math.Min(this.length, count);
                this.GetBase64(buffer, iOffset, actual);
                this.offset += actual;
                this.length -= actual;
                return true;
            }
            actual = 0;
            return false;
        }

        public bool TryReadChars(char[] chars, int iOffset, int count, out int actual)
        {
            if (this.type == ValueHandleType.Unicode)
                return this.TryReadUnicodeChars(chars, iOffset, count, out actual);
            if (this.type != ValueHandleType.UTF8)
            {
                actual = 0;
                return false;
            }
            int offset1 = this.offset;
            int val1 = count;
            byte[] buffer = this.bufferReader.Buffer;
            int offset2 = this.offset;
            int length = this.length;
            while (true)
            {
                for (; val1 > 0 && length > 0; --val1)
                {
                    byte num = buffer[offset2];
                    if (num < (byte)128)
                    {
                        chars[offset1] = (char)num;
                        ++offset2;
                        --length;
                        ++offset1;
                    }
                    else
                        break;
                }
                if (val1 != 0 && length != 0)
                {
                    UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                    int chars1;
                    int num;
                    if (val1 >= utF8Encoding.GetMaxCharCount(length) || val1 >= utF8Encoding.GetCharCount(buffer, offset2, length))
                    {
                        chars1 = utF8Encoding.GetChars(buffer, offset2, length, chars, offset1);
                        num = length;
                    }
                    else
                    {
                        Decoder decoder = utF8Encoding.GetDecoder();
                        int byteCount = Math.Min(val1, length);
                        chars1 = decoder.GetChars(buffer, offset2, byteCount, chars, offset1);
                        while (chars1 == 0)
                        {
                            chars1 = decoder.GetChars(buffer, offset2 + byteCount, 1, chars, offset1);
                            ++byteCount;
                        }
                        num = utF8Encoding.GetByteCount(chars, offset1, chars1);
                    }
                    offset2 += num;
                    length -= num;
                    offset1 += chars1;
                    val1 -= chars1;
                }
                else
                    break;
            }
            this.offset = offset2;
            this.length = length;
            actual = count - val1;
            return true;
        }

        private bool TryReadUnicodeChars(char[] chars, int iOffset, int count, out int actual)
        {
            int num = Math.Min(count, this.length / 2);
            for (int index = 0; index < num; ++index)
                chars[iOffset + index] = (char)this.bufferReader.GetInt16(this.offset + index * 2);
            this.offset += num * 2;
            this.length -= num * 2;
            actual = num;
            return true;
        }

        public bool TryGetByteArrayLength(out int oLength)
        {
            if (this.type == ValueHandleType.Base64)
            {
                oLength = this.length;
                return true;
            }
            oLength = 0;
            return false;
        }

        private string GetCharsText()
        {
            return this.bufferReader.GetString(this.offset, this.length);
        }

        private string GetUnicodeCharsText()
        {
            return this.bufferReader.GetUnicodeString(this.offset, this.length);
        }

        private string GetEscapedCharsText()
        {
            return this.bufferReader.GetEscapedString(this.offset, this.length);
        }

        private string GetCharText()
        {
            int num = this.GetChar();
            return num <= (int)ushort.MaxValue ? ((char)num).ToString() : (string)null;
        }

        private int GetChar()
        {
            return this.offset;
        }

        private int GetInt8()
        {
            return this.bufferReader.GetInt8(this.offset);
        }

        private int GetInt16()
        {
            return this.bufferReader.GetInt16(this.offset);
        }

        private int GetInt32()
        {
            return this.bufferReader.GetInt32(this.offset);
        }

        private long GetInt64()
        {
            return this.bufferReader.GetInt64(this.offset);
        }

        private ulong GetUInt64()
        {
            return this.bufferReader.GetUInt64(this.offset);
        }

        private float GetSingle()
        {
            return this.bufferReader.GetSingle(this.offset);
        }

        private double GetDouble()
        {
            return this.bufferReader.GetDouble(this.offset);
        }

        private Decimal GetDecimal()
        {
            return this.bufferReader.GetDecimal(this.offset);
        }

        private UniqueId GetUniqueId()
        {
            return this.bufferReader.GetUniqueId(this.offset);
        }

        private Guid GetGuid()
        {
            return this.bufferReader.GetGuid(this.offset);
        }

        private void GetBase64(byte[] buffer, int iOffset, int count)
        {
            this.bufferReader.GetBase64(this.offset, buffer, iOffset, count);
        }
    }
}
