using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkLibrary
{
    /// <summary>
    /// 消息处理函数
    /// </summary>
    public delegate void PacketHandler(byte[] packetData);

    public abstract class WorldSocket
    {
        /// <summary>
        /// 当前连接所用id
        /// </summary>
        private readonly int id;
        public int ID => id;

        /// <summary>
        /// 消息处理列表
        /// </summary>
        protected Dictionary<int, PacketHandler> PacketHandlers = new Dictionary<int, PacketHandler>();
        public void RegisterHandler(int id, PacketHandler handler)
        {
            if (!PacketHandlers.ContainsKey(id))
                PacketHandlers.Add(id, handler);
        }

        /// <summary>
        /// 连接用Socket
        /// </summary>
        protected Socket connSocket;

        /// <summary>
        /// socket管理器
        /// </summary>
        protected WorldSocketManager worldSocketManager;

        public WorldSocket(Type packetType, Socket linkSocket, WorldSocketManager socketManager)
        {
            if (!packetType.IsSubclassOf(typeof(WorldPacket)))
                throw new Exception("you must post a subclass of WorldPacket as argument for packetType.");

            connSocket = linkSocket;
            worldSocketManager = socketManager;
            WorldPacketType = packetType;

            id = connSocket.Handle.ToInt32();
        }


        private Type WorldPacketType;
        private byte[] readData;
        protected SocketAsyncEventArgs ReadArgs, WriteArgs;
        protected ByteBuffer ReadBuffer;

        /// <summary>
        /// 开始连接
        /// </summary>
        public void Open()
        {
            worldSocketManager.Log(LogType.Message, "客户端[{0}]{1}连接……", ID, connSocket.RemoteEndPoint);
            readData = new byte[0x2000];
            ReadBuffer = new ByteBuffer();
            Packets = new Queue<WorldPacket>();

            ReadArgs = new SocketAsyncEventArgs();
            ReadArgs.UserToken = this;
            ReadArgs.SetBuffer(readData, 0, readData.Length);
            ReadArgs.AcceptSocket = connSocket;

            WriteArgs = new SocketAsyncEventArgs();
            WriteArgs.UserToken = this;
            WriteArgs.AcceptSocket = connSocket;

            ReadArgs.Completed += IO_Completed;
            WriteArgs.Completed += IO_Completed;
            if (!connSocket.ReceiveAsync(ReadArgs))
                ProcessRead();

            Initialize();
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs args)
        {
            WorldSocket client = args.UserToken as WorldSocket;
            lock (client)
            {
                switch (args.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        client.ProcessRead();
                        break;
                    case SocketAsyncOperation.Send:
                        client.ProcessSend();
                        break;
                    default:
                        client.Close();
                        break;
                }
            }
        }

        private void ProcessRead()
        {
            if (ReadArgs.BytesTransferred > 0 && ReadArgs.SocketError == SocketError.Success)
            {
                ReadPacket();
                if (connSocket != null)
                {
                    if (!connSocket.ReceiveAsync(ReadArgs))
                        ProcessRead();

                    return;
                }
            }

            Close();
        }

        protected Queue<WorldPacket> Packets;
        protected bool IsSending = false;

        private void ProcessSend()
        {
            if (WriteArgs.SocketError == SocketError.Success)
            {
                IsSending = false;
                lock (Packets)
                {
                    if (Packets.Count > 0)
                        SendPacket(Packets.Dequeue());
                }

                return;
            }

            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// sample code like <code>ReadBuffer.Write(ReadArgs);</code>
        /// </summary>
        protected abstract void BeforeRead();

        /// <summary>
        /// sample code like <code>return packet.Pack();</code>
        /// </summary>
        protected abstract void HandleUnRegister(int cmdId, byte[] packetData);

        /// <summary>
        /// 
        /// </summary>
        protected abstract byte[] BeforeSend(WorldPacket packet);

        private void ReadPacket()
        {
            BeforeRead();

            while (ReadBuffer.GetLength() > sizeof(ushort))
            {
                WorldPacket worldPacket = Activator.CreateInstance(WorldPacketType, ReadBuffer) as WorldPacket;
                var cmdId = worldPacket.ReadPacketID();
                var packetLength = worldPacket.ReadPacketLength();
                var packetData = ReadBuffer.ReadBytes(packetLength);
                if (PacketHandlers.ContainsKey(cmdId))
                    PacketHandlers[cmdId].Invoke(packetData);
                else
                    HandleUnRegister(cmdId, packetData);

                ReadBuffer.ClearReaded();
            }
        }

        /// <summary>
        /// May call from multi threads, so we lock self
        /// 发送包
        /// </summary>
        public void SendPacket(WorldPacket packet)
        {
            lock (this)
            {
                if (IsSending)
                {
                    lock (Packets)
                        Packets.Enqueue(packet);
                    return;
                }

                IsSending = true;
                var bytes = BeforeSend(packet);
                WriteArgs.SetBuffer(bytes, 0, bytes.Length);
                if (!connSocket.SendAsync(WriteArgs))
                    ProcessSend();
            }
        }

        public void Close()
        {
            if (connSocket == null)
                return;

            worldSocketManager.Log(LogType.Message, "客户端[{0}]{1}断开……", ID, connSocket.RemoteEndPoint);
            worldSocketManager.CloseSocket(this);

            try
            {
                connSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                connSocket.Close();
            }

            connSocket = null;
        }
    }
}
