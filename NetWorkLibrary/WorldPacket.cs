﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkLibrary
{
    public abstract class WorldPacket
    {
        protected ByteBuffer ByteBuffer;

        public WorldPacket(ByteBuffer buffer)
        {
            ByteBuffer = buffer;
        }

        public abstract byte[] Pack();

        public abstract int ReadPacketID();

        public abstract int ReadPacketLength();


    }
}
