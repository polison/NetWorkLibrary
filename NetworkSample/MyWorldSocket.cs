using NetWorkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSample
{
    public class MyWorldSocket : NetWorkLibrary.WorldSocket
    {
        public MyWorldSocket(Socket linkSocket, WorldSocketManager socketManager)
            : base(linkSocket, socketManager)
        {

        }

        protected override void BeforeRead()
        {
            ReadBuffer.Write(ReadArgs);
        }

        protected override byte[] BeforeSend(WorldPacket packet)
        {
            return packet.Pack();
        }

        protected override void Initialize()
        {
            WorldPacketType = typeof(MyWorldPacket);
        }
    }
}
