﻿using ClientSample;
using NetWorkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample
{
    public class WorldSocket : BaseWorldSocket
    {
        public WorldSocket(Type packetType, Socket linkSocket, WorldSocketManager socketManager)
            : base(packetType, linkSocket, socketManager)
        {

        }

        private void PrintHello(BaseWorldSocket socket, ByteBuffer byteBuffer)
        {
            int stringLenth = byteBuffer.ReadInt32();
            var bytes = byteBuffer.ReadBytes(stringLenth);
            worldSocketManager.Log(LogType.Message, "{0}[{1}]收到信息:{2}", worldSocketManager.TargetHead, socket.ID, Encoding.UTF8.GetString(bytes));
        }

        protected override void Initialize()
        {
            RegisterHandler(1, PrintHello);

            ByteBuffer byteBuffer = new ByteBuffer();
            var packet = new WorldPacket(byteBuffer);
            packet.ID = 1;
            var bytes = Encoding.UTF8.GetBytes(string.Format("{0}[{1}]这里是{2}.", worldSocketManager.TargetHead, ID, worldSocketManager.LogHead));
            byteBuffer.WriteInt32(bytes.Length);
            byteBuffer.Write(bytes);

            SendPacket(packet);
        }

        protected override void BeforeRead()
        {
            ReadBuffer.Write(ReadArgs);
        }

        protected override void HandleUnRegister(int cmdId, byte[] packetData)
        {
            worldSocketManager.Log(LogType.Warning, "cmd:0x{0:X2},data:{1}", cmdId, BitConverter.ToString(packetData));
        }

        protected override byte[] BeforeSend(BaseWorldPacket packet)
        {
            return packet.Pack();
        }

        protected override void BeforeClose()
        {

        }
    }
}
