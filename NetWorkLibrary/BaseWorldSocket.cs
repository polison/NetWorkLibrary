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
    public delegate void PacketHandler(BaseWorldSocket socket, ByteBuffer byteBuffer);

    public abstract class BaseWorldSocket
    {
        /// <summary>
        /// 当前连接所用id
        /// </summary>
        private readonly long id;
        public long ID => id;

        /// <summary>
        /// 消息处理列表
        /// </summary>
        protected static Dictionary<int, PacketHandler> PacketHandlers = new Dictionary<int, PacketHandler>();
        public static void RegisterHandler(int id, PacketHandler handler)
        {
            if (!PacketHandlers.ContainsKey(id))
                PacketHandlers.Add(id, handler);
        }

        /// <summary>
        /// 连接用Socket
        /// </summary>
        protected Socket connSocket;
        public Socket Socket => connSocket;

        /// <summary>
        /// socket管理器
        /// </summary>
        protected WorldSocketManager worldSocketManager;

        public BaseWorldSocket(Type packetType, Socket linkSocket, WorldSocketManager socketManager)
        {
            if (!packetType.IsSubclassOf(typeof(BaseWorldPacket)))
                throw new Exception("you must post a subclass of WorldPacket as argument for packetType.");

            connSocket = linkSocket;
            worldSocketManager = socketManager;
            WorldPacketType = packetType;

            id = connSocket.Handle.ToInt64();
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
            worldSocketManager.Log(LogType.Message, "{0}[{1}]{2}连接……", worldSocketManager.TargetHead, ID, connSocket.RemoteEndPoint);
            readData = new byte[0x2000];
            ReadBuffer = new ByteBuffer();
            Packets = new Queue<BaseWorldPacket>();

            ReadArgs = new SocketAsyncEventArgs();
            ReadArgs.UserToken = this;
            ReadArgs.SetBuffer(readData, 0, readData.Length);
            ReadArgs.AcceptSocket = connSocket;

            WriteArgs = new SocketAsyncEventArgs();
            WriteArgs.UserToken = this;
            WriteArgs.AcceptSocket = connSocket;

            ReadArgs.Completed += IO_Completed;
            WriteArgs.Completed += IO_Completed;

            Initialize();
            if (!connSocket.ReceiveAsync(ReadArgs))
                ProcessRead();
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs args)
        {
            BaseWorldSocket client = args.UserToken as BaseWorldSocket;
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
                try
                {
                    ReadPacket();

                    if (connSocket != null)
                    {
                        if (!connSocket.ReceiveAsync(ReadArgs))
                            ProcessRead();

                        return;
                    }
                }
                catch(Exception e)
                {
                    worldSocketManager.Log(LogType.Error, e.Message);
                    Close();
                }
            }

            Close();
        }

        protected Queue<BaseWorldPacket> Packets;
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
        /// 
        /// </summary>
        protected abstract void BeforeClose();

        /// <summary>
        /// sample code like <code>ReadBuffer.Write(ReadArgs);</code>
        /// </summary>
        protected abstract void BeforeRead();

        /// <summary>
        /// 
        /// </summary>
        protected abstract void HandleUnRegister(int cmdId, byte[] packetData);

        /// <summary>
        /// sample code like <code>return packet.Pack();</code>
        /// </summary>
        protected abstract byte[] BeforeSend(BaseWorldPacket packet);

        private void ReadPacket()
        {
            BeforeRead();

            while (ReadBuffer.GetLength() > sizeof(ushort))
            {
                BaseWorldPacket worldPacket = Activator.CreateInstance(WorldPacketType, ReadBuffer) as BaseWorldPacket;
                var cmdId = worldPacket.ReadPacketID();
                var packetLength = worldPacket.ReadPacketLength();
                var packetData = ReadBuffer.ReadBytes(packetLength);
                if (PacketHandlers.ContainsKey(cmdId))
                {
                    ByteBuffer byteBuffer = new ByteBuffer();
                    byteBuffer.Write(packetData);
                    PacketHandlers[cmdId].Invoke(this, byteBuffer);
                }
                else
                    HandleUnRegister(cmdId, packetData);

                ReadBuffer.ClearReaded();
            }
        }

        /// <summary>
        /// May call from multi threads, so we lock self
        /// 发送包
        /// </summary>
        public void SendPacket(BaseWorldPacket packet)
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

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            lock(this)
            {
                if (connSocket == null)
                    return;

                BeforeClose();

                try
                {
                    worldSocketManager.Log(LogType.Message, "{0}[{1}]{2}断开……", worldSocketManager.TargetHead, ID, connSocket.RemoteEndPoint);
                    worldSocketManager.CloseSocket(id);
               
                    connSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    if (connSocket != null)
                        connSocket.Close();
                }

                ReadArgs.Dispose();
                WriteArgs.Dispose();
                connSocket = null;
            }
        }
    }
}
