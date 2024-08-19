using NetWorkLibrary.Network;
using NetWorkLibrary.Utility;

namespace NetWorkLibrary.Sample
{
    public class WorldPacket : IPacket
    {
        public virtual uint PacketOpcode { get; }

        protected ByteBuffer PacketBuffer => byteBuffer;
        private ByteBuffer byteBuffer;

        public WorldPacket()
        {
            byteBuffer = new ByteBuffer();
        }

        public virtual uint GetLength(ByteBuffer buffer)
        {
            return buffer.ReadUint32();
        }

        public virtual uint GetOpcode(ByteBuffer buffer)
        {
            return buffer.ReadUint32();
        }

        public virtual void Read(ByteBuffer buffer)
        {

        }

        public virtual void Write(ByteBuffer buffer)
        {
            buffer.WriteUint32(PacketOpcode);
            buffer.WriteUint32((uint)byteBuffer.Length);
            buffer.Write(byteBuffer.Data);
        }
    }
}
