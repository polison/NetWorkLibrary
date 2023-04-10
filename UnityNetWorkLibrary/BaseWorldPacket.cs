namespace NetWorkLibrary
{
    public abstract class BaseWorldPacket
    {
        protected ByteBuffer ByteBuffer;

        public BaseWorldPacket(ByteBuffer buffer)
        {
            ByteBuffer = buffer;
            ByteBuffer.ResetRead();
        }

        public abstract byte[] Pack();

        public abstract int ReadPacketID();

        public abstract int ReadPacketLength();


    }
}
