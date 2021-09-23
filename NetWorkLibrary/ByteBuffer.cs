using NetWorkLibrary.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkLibrary
{
    public class ByteBuffer
    {
        protected List<byte> buffer;
        protected int rpos, wpos;

        public ByteBuffer()
        {
            buffer = new List<byte>();
            wpos = 0;
            rpos = 0;
        }
        public ByteBuffer(byte[] data)
        {
            buffer = new List<byte>();
            wpos = 0;
            rpos = 0;
            Write(data);
        }

        public byte[] GetBytes()
        {
            return buffer.ToArray();
        }

        public int GetLength()
        {
            return buffer.Count;
        }

        public void ClearReaded()
        {
            buffer.RemoveRange(0, rpos);
            rpos = 0;
            wpos -= rpos;
        }

        public void Write(SocketAsyncEventArgs args, RC4 encrypt = null)
        {
            var bytes = new byte[args.BytesTransferred];
            Array.Copy(args.Buffer, args.Offset, bytes, 0, bytes.Length);
            if (encrypt != null)
                bytes = encrypt.Encrypt(bytes);
            Write(bytes);
        }

        public void Write(byte[] data, bool littleEndian = true)
        {
            if (!littleEndian)
                Array.Reverse(data);
            buffer.AddRange(data);
            wpos += data.Length;
        }

        public void Write(byte value)
        {
            buffer.Add(value);
            wpos++;
        }

        public void Write(bool value)
        {
            if (value)
                Write((byte)1);
            else
                Write((byte)0);
        }

        public void Write(ushort value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(uint value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(ulong value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(short value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(int value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(long value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(float value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public void Write(double value, bool littleEndian = true)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, littleEndian);
        }

        public byte[] ReadBytes(int length, bool littleEndian = true)
        {
            var result = new byte[length];
            buffer.CopyTo(rpos, result, 0, length);
            if (!littleEndian)
                Array.Reverse(result);
            rpos += length;
            return result;
        }

        public byte ReadByte()
        {
            return buffer[rpos++];
        }

        public bool ReadBool()
        {
            var result = ReadByte();
            return result == 1 ? true : false;
        }

        public ushort ReadUInt16(bool littleEndian = true)
        {
            var bytes = ReadBytes(2, littleEndian);
            return BitConverter.ToUInt16(bytes);
        }

        public uint ReadUInt32(bool littleEndian = true)
        {
            var bytes = ReadBytes(4, littleEndian);
            return BitConverter.ToUInt32(bytes);
        }

        public ulong ReadUInt64(bool littleEndian = true)
        {
            var bytes = ReadBytes(8, littleEndian);
            return BitConverter.ToUInt64(bytes);
        }

        public short ReadInt16(bool littleEndian = true)
        {
            var bytes = ReadBytes(2, littleEndian);
            return BitConverter.ToInt16(bytes);
        }

        public int ReadInt32(bool littleEndian = true)
        {
            var bytes = ReadBytes(4, littleEndian);
            return BitConverter.ToInt32(bytes);
        }

        public long ReadInt64(bool littleEndian = true)
        {
            var bytes = ReadBytes(8, littleEndian);
            return BitConverter.ToInt64(bytes);
        }

        public float ReadSingle(bool littleEndian = true)
        {
            var bytes = ReadBytes(4, littleEndian);
            return BitConverter.ToSingle(bytes);
        }

        public double ReadDouble(bool littleEndian = true)
        {
            var bytes = ReadBytes(8, littleEndian);
            return BitConverter.ToDouble(bytes);
        }
    }
}
