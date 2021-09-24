using NetWorkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSample
{
    class MyWorldPacket : WorldPacket
    {
        public int ID = -1;

        public MyWorldPacket(ByteBuffer byteBuffer)
            : base(byteBuffer)
        {
            
        }

        public override int ReadPacketID()
        {
            return ByteBuffer.ReadInt32();
        }

        public override int ReadPacketLength()
        {
            return ByteBuffer.ReadInt32();
        }

        public override byte[] Pack()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteInt32(ID);
            buffer.WriteInt32(ByteBuffer.GetLength());
            buffer.Write(ByteBuffer.GetBytes());
            return buffer.GetBytes();
        }
    }
}
