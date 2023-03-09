using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetWorkLibrary
{
    public class WorldSocketManager
    {
        #region 服务端配置
        /// <summary>
        /// 客户端列表，如果是服务端使用这个参数
        /// </summary>
        private Dictionary<long, BaseWorldSocket> worldSockets;

        /// <summary>
        /// 最大连接数
        /// </summary>
        private readonly int maxConnectionNumber = 2000;

        /// <summary>
        /// 同一时间最大连接数
        /// </summary>
        private readonly int maxConnectionAtOnce = 10;

        /// <summary>
        /// 信号量，锁定最大连接
        /// </summary>
        private Semaphore maxConnection;

        /// <summary>
        /// 获取客户端列表
        /// </summary>
        public Dictionary<long, BaseWorldSocket> GetWorldSockets()
        {
            return worldSockets;
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        public void BroadCast(BaseWorldPacket packet)
        {
            lock(worldSockets)
            {
                foreach (var socket in worldSockets.Values)
                {
                    socket.SendPacket(packet);
                }
            }
        }

        /// <summary>
        /// 开启服务(服务端)
        /// </summary>
        /// <param name="port">服务端口</param>
        /// <returns>是否成功</returns>
        public bool OpenConnection(int port)
        {
            LogHead = "服务端(server)";
            TargetHead = "客户端(client)";
            try
            {
                worldSockets = new Dictionary<long, BaseWorldSocket>();
                maxConnection = new Semaphore(maxConnectionNumber, maxConnectionNumber);

                IPEndPoint local = new IPEndPoint(IPAddress.Any, port);
                connSocket = new Socket(local.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                connSocket.Bind(local);
                connSocket.Listen(maxConnectionAtOnce);
            }
            catch (Exception e)
            {
                Log(LogType.Error, "网络服务启动失败，{0}", e.Message);
                return false;
            }

            Log(LogType.Message, "网络服务成功启动！");
            DoAccept(null);
            return true;
        }

        private void DoAccept(SocketAsyncEventArgs args)
        {
            if (args == null)
            {
                args = new SocketAsyncEventArgs();
                args.Completed += ProcessAccept;
            }
            else
                args.AcceptSocket = null;

            maxConnection.WaitOne();
            if (connSocket != null)
            {
                if (connSocket.AcceptAsync(args))
                    ProcessAccept(null, args);
            }
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs eventArgs)
        {
            var socket = eventArgs.AcceptSocket;
            if (socket.Connected)
            {
                OpenSocket(socket);
                DoAccept(eventArgs); //把当前异步事件释放，等待下次连接
            }
        }

        private void OpenSocket(Socket linkSocket)
        {
            var worldSocket = Activator.CreateInstance(worldSocketType, worldPacketType, linkSocket, this) as BaseWorldSocket;
            lock (worldSockets)
            {
                worldSockets.Add(worldSocket.ID, worldSocket);
            }

            worldSocket.Open();
        }

        internal void CloseSocket(long clientID)
        {
            if (worldSockets == null)
                return;

            lock (worldSockets)
            {
                worldSockets.Remove(clientID);
                maxConnection.Release();
            }
        }

        #endregion


        #region 客户端配置
        /// <summary>
        /// 客户端，如果是客户端使用这个
        /// </summary>
        private BaseWorldSocket client = null;

        private IPEndPoint ServerEndPoint;

        /// <summary>
        /// 客户都，客户端连接用
        /// </summary>
        private SocketAsyncEventArgs connEventArgs = null;

        /// <summary>
        /// 开启服务(客户端)
        /// </summary>
        /// <param name="serverEndPoint">服务器地址</param>
        public void OpenConnection(IPEndPoint serverEndPoint)
        {
            LogHead = "客户端(client)";
            TargetHead = "服务端(server)";
            ServerEndPoint = serverEndPoint;
            if (connEventArgs == null)
            {
                connSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                connEventArgs = new SocketAsyncEventArgs();
                connEventArgs.RemoteEndPoint = serverEndPoint;

                connEventArgs.Completed += ProcessConnect;
            }

            if (!connSocket.ConnectAsync(connEventArgs))
                ProcessConnect(null, connEventArgs);
        }

        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
            switch (e.SocketError)
            {
                case SocketError.Success:
                    {
                        var socket = e.ConnectSocket;
                        if (socket.Connected)
                        {
                            Log(LogType.Message, "网络服务成功启动并已连接！");
                            client = Activator.CreateInstance(worldSocketType, worldPacketType, socket, this) as BaseWorldSocket;
                            client.Open();
                        }
                        else
                        {
                            OpenConnection(ServerEndPoint);
                            Log(LogType.Warning, "网络服务成功启动但未能连接，正在重连……");
                        }
                    }
                    break;
                case SocketError.AccessDenied:
                case SocketError.OperationAborted:
                    Log(LogType.Warning, "网络服务已经关闭……");
                    break;
                default:
                    {
                        OpenConnection(ServerEndPoint);
                        Log(LogType.Error, "网络服务未能连接，正在重连……");
                    }
                    break;
            }
        }

        #endregion

        public string LogHead = "客户端";
        public string TargetHead = "客户端";

        /// <summary>
        /// 日志输出工具
        /// </summary>
        private ILog logger = null;

        private Socket connSocket;

        private Type worldSocketType;

        private Type worldPacketType;

        public Dictionary<long, BaseWorldSocket> WorldSockets { get => worldSockets; set => worldSockets = value; }
        public Dictionary<long, BaseWorldSocket> WorldSockets1 { get => worldSockets; set => worldSockets = value; }

        public WorldSocketManager(Type socketType, Type packetType, ILog log = null)
        {
            if (!socketType.IsSubclassOf(typeof(BaseWorldSocket)))
                throw new Exception("you must post a subclass of WorldSocket as argument for socketType.");

            if (!packetType.IsSubclassOf(typeof(BaseWorldPacket)))
                throw new Exception("you must post a subclass of WorldPacket as argument for packetType.");

            worldSocketType = socketType;
            worldPacketType = packetType;
            logger = log;
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        public void Log(LogType type, string format, params object[] args)
        {
            if (logger == null)
                return;

            lock (logger)
            {
                switch (type)
                {
                    case LogType.Message:
                        logger.Message($"{LogHead}-{format}", args);
                        break;
                    case LogType.Warning:
                        logger.Warning($"{LogHead}-{format}", args);
                        break;
                    case LogType.Error:
                        logger.Error($"{LogHead}-{format}", args);
                        break;
                }
            }
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public void CloseConnection()
        {
            if (connSocket != null)
            {
                if (client != null)
                    client.Close();
                else
                    connSocket.Close();

                client = null;
                connSocket = null;
            }

            if (worldSockets != null)
            {
                while (worldSockets.Count > 0)
                {
                    var socket = worldSockets.Values.First();
                    socket.Close();
                }

                worldSockets.Clear();
                worldSockets = null;
            }

            if (maxConnection != null)
            {
                maxConnection.Close();
                maxConnection = null;
            }

            Log(LogType.Message, "网络服务已终止！");
        }
    }
}
