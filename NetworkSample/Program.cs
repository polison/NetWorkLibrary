using System;
using System.Net;
using NetWorkLibrary;
using NetWorkLibrary.Algorithm;

namespace NetworkSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Log logger = new Log();
            WorldSocketManager serverManager = new WorldSocketManager(typeof(WorldSocket), typeof(WorldPacket), logger);
            serverManager.OpenConnection(2000);

            WorldSocketManager clientManager = new WorldSocketManager(typeof(WorldSocket), typeof(WorldPacket), logger);
            IPEndPoint server = new IPEndPoint(IPAddress.Loopback, 2000);
            clientManager.OpenConnection(server);

            while (Console.Read() != 'q')
            {

            }

            serverManager.CloseConnection();
            clientManager.CloseConnection();
        }
    }

    public class Log : ILog
    {
        public void Error(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, args);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Message(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(format, args);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Warning(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(format, args);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
