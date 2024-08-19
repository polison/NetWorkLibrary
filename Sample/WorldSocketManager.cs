using NetWorkLibrary.Network;
using NetWorkLibrary.Utility;
using System;
using System.Threading.Tasks;

namespace NetWorkLibrary.Sample
{
    public class WorldSocketManager : SocketManager<WorldSocket>
    {
        public WorldSocketManager() : base()
        {
            LogManager.Instance.AddLogger(new ConsoleLog());
            LogManager.Instance.AddLogger(new FileLog("WorldServer.log"));
        }

        protected override void SocketAdded(WorldSocket sock)
        {
            base.SocketAdded(sock);
            LogManager.Instance.Log(LogType.Message, $"[{sock.LinkIP}] connected.");
        }

        protected override void SocketRemoved(WorldSocket sock)
        {
            base.SocketRemoved(sock);
            LogManager.Instance.Log(LogType.Message, $"[{sock.LinkIP}] shut down.");
        }

        public static void ServerLoop()
        {
            WorldSocketManager socketManager = new WorldSocketManager();
            socketManager.Open("localhost", 20001);
            Console.Write("请输入操作(q-退出):");
            Console.WriteLine();

            while (socketManager.IsRunning())
            {
                var code = Console.ReadLine().Trim();
                switch (code)
                {
                    case "q":
                        socketManager.Close();
                        Task.Delay(5000).Wait();
                        return;
                    default:
                        break;
                }
            }
        }
    }
}
