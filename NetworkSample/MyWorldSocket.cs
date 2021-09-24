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

        private void PrintHello(byte[] packetData)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.Write(packetData);
            int stringLenth = byteBuffer.ReadInt32();
            var bytes = byteBuffer.ReadBytes(stringLenth);
            worldSocketManager.Log(LogType.Message, "收到信息:{0}", Encoding.UTF8.GetString(bytes));
        }

        protected override void Initialize()
        {
            WorldPacketType = typeof(MyWorldPacket);
            RegisterHandler(1, PrintHello);

            ByteBuffer byteBuffer = new ByteBuffer();
            var packet = new MyWorldPacket(byteBuffer);
            packet.ID = 1;
            var bytes = Encoding.UTF8.GetBytes(string.Format("这里是客户端{0}.", ID));
            byteBuffer.WriteInt32(bytes.Length);
            byteBuffer.Write(bytes);

            SendPacket(packet);
        }
    }
}
