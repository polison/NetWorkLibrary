using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkLibrary
{
    public abstract class BaseWorldPacket
    {
        protected ByteBuffer ByteBuffer;

        public BaseWorldPacket(ByteBuffer buffer)
        {
            ByteBuffer = buffer;
        }

        public abstract byte[] Pack();

        public abstract int ReadPacketID();

        public abstract int ReadPacketLength();


    }
}
