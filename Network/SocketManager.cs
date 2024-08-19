using NetWorkLibrary.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetWorkLibrary.Network
{
    public enum ManagerType : byte
    {
        Server,

        Client
    }

    public class SocketManager<TSocketType> : BackgroundService where TSocketType : ISocket
    {
        private readonly ManagerType managerType;

        private Socket linkSocket;

        private SocketAsyncEventArgs acceptArgs;
        private SocketAsyncEventArgs connectArgs;

        private List<TSocketType> sockets = new List<TSocketType>();
        private List<TSocketType> newSockets = new List<TSocketType>();

        private int clientNum = 0;

        public SocketManager(ManagerType inType = ManagerType.Server)
        {
            managerType = inType;

            acceptArgs = new SocketAsyncEventArgs();
            acceptArgs.Completed += (sender, args) => { ProcessAcceptAsync(args); };

            connectArgs = new SocketAsyncEventArgs();
            connectArgs.Completed += (sender, args) => { ProcessConnectAsync(args); };
        }

        public bool Open(string ip, int port)
        {
            StartAsync();

            switch (managerType)
            {
                case ManagerType.Server:
                    return Open(port);
                case ManagerType.Client:
                    {
                        IPAddress iPAddress = Dns.GetHostAddresses(ip).First((i) => i.AddressFamily == AddressFamily.InterNetwork);

                        IPEndPoint server = new IPEndPoint(iPAddress, port);
                        return Open(server);
                    }
            }

            return false;
        }

        private bool Open(int port)
        {
            try
            {
                IPEndPoint local = new IPEndPoint(IPAddress.Any, port);
                linkSocket = new Socket(local.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                linkSocket.Bind(local);
                linkSocket.Listen(1);
                LogManager.Instance.Log(LogType.Message, "Start Listen In {0}...", port);
            }
            catch (Exception e)
            {
                LogManager.Instance.Log(LogType.Error, "Listen Failed In {0} With {1}...", port, e.Message);
                return false;
            }

            ProcessAccept();
            return true;
        }

        public void Close()
        {
            if (linkSocket != null)
            {
                linkSocket.Close();
                linkSocket = null;
            }

            StopAsync().Wait();
            LogManager.Instance.Log(LogType.Message, "Network Service End...");
        }

        protected virtual void OnSocketOpen(Socket socket)
        {
            try
            {
                TSocketType newSocket = (TSocketType)Activator.CreateInstance(typeof(TSocketType), socket);
                newSocket.Open();

                Interlocked.Increment(ref clientNum);
                newSockets.Add(newSocket);
                SocketAdded(newSocket);
            }
            catch (Exception e)
            {
                LogManager.Instance.Log(LogType.Error, $"SocketMananger.OnSocketOpen :{e.Message}");
            }
        }

        private void ProcessAccept()
        {
            if (linkSocket != null)
            {
                acceptArgs.AcceptSocket = null;
                if (!linkSocket.AcceptAsync(acceptArgs))
                    ProcessAcceptAsync(acceptArgs);
            }
        }

        private void ProcessAcceptAsync(SocketAsyncEventArgs args)
        {
            var socket = args.AcceptSocket;
            if (socket != null && socket.IsBound)
            {
                OnSocketOpen(socket);
                ProcessAccept();
            }
        }

        private void AddNewSockets()
        {
            if (newSockets.Count == 0)
                return;

            foreach (var socket in newSockets.ToArray())
            {
                if (!socket.IsOpen())
                {
                    SocketRemoved(socket);
                    Interlocked.Decrement(ref clientNum);
                }
                else
                    sockets.Add(socket);
            }

            newSockets.Clear();
        }

        private bool Open(IPEndPoint serverEndPoint)
        {
            linkSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectArgs.RemoteEndPoint = serverEndPoint;
            ProcessConnect();

            return true;
        }

        private void ProcessConnect()
        {
            if (linkSocket != null)
            {
                if (!linkSocket.ConnectAsync(connectArgs))
                    ProcessConnectAsync(connectArgs);
            }
        }

        private void ProcessConnectAsync(SocketAsyncEventArgs args)
        {
            var socket = args.ConnectSocket;
            if (socket != null && socket.Connected)
            {
                OnSocketOpen(socket);
                LogManager.Instance.Log(LogType.Message, "Connected To {0}...", args.RemoteEndPoint);
            }
            else
            {
                ProcessConnect();
                LogManager.Instance.Log(LogType.Warning, "Failed Connect To {0},Retry...", args.RemoteEndPoint);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int sleepTime = 10;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(sleepTime);

                uint tickStart = Time.GetMSTime();

                AddNewSockets();

                for (var i = 0; i < sockets.Count; ++i)
                {
                    TSocketType socket = sockets[i];
                    if (!socket.Update())
                    {
                        if (socket.IsOpen())
                            socket.Close();

                        SocketRemoved(socket);
                        Interlocked.Decrement(ref clientNum);
                        sockets.Remove(socket);

                        if (managerType == ManagerType.Client)
                            return;
                    }
                }

                uint diff = Time.GetMSTimeDiffToNow(tickStart);
                sleepTime = (int)(diff > 10 ? 0 : 10 - diff);
            }

            AddNewSockets();
            for (var i = 0; i < sockets.Count; ++i)
            {
                TSocketType socket = sockets[i];
                if (socket.IsOpen())
                    socket.Close();

                SocketRemoved(socket);
                Interlocked.Decrement(ref clientNum);
                sockets.Remove(socket);
            }

            sockets.Clear();
        }

        protected virtual void SocketAdded(TSocketType sock) { }

        protected virtual void SocketRemoved(TSocketType sock) { }
    }
}
