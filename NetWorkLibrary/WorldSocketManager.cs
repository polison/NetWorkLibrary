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
        private Dictionary<int, BaseWorldSocket> worldSockets;

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

        public bool OpenConnection(int port)
        {
            LogHead = "server";
            try
            {
                worldSockets = new Dictionary<int, BaseWorldSocket>();
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

        public void CloseSocket(BaseWorldSocket worldSocket)
        {
            if (worldSockets == null)
                return;

            lock (worldSockets)
            {
                worldSockets.Remove(worldSocket.ID);
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

        public bool OpenConnection(IPEndPoint serverEndPoint)
        {
            LogHead = "client";
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

            return true;
        }

        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
            var socket = e.ConnectSocket;
            if (socket.Connected)
            {
                client = Activator.CreateInstance(worldSocketType, worldPacketType, socket, this) as BaseWorldSocket;
                client.Open();
                Log(LogType.Message, "网络服务成功启动并已连接！");
            }
            else
            {
                OpenConnection(ServerEndPoint);
                Log(LogType.Warning, "网络服务成功启动但未能连接，正在重连……");
            }
        }

        #endregion

        public string LogHead = "客户端";

        /// <summary>
        /// 日志输出工具
        /// </summary>
        private ILog logger = null;

        private Socket connSocket;

        private Type worldSocketType;

        private Type worldPacketType;

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
