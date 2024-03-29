﻿using System;
using System.Net;
using NetWorkLibrary;
using NetWorkLibrary.Algorithm;

namespace ServerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Log logger = new Log();
            WorldSocketManager serverManager = new WorldSocketManager(typeof(WorldSocket), typeof(WorldPacket), logger);
            serverManager.OpenConnection(2000);

            int code;
            while ((code = Console.Read())!='q')
            {
                if(code == 'p')
                {
                    foreach (var item in serverManager.GetWorldSockets())
                    {
                        logger.Message("{0}---{1}", item.Key, item.Value.Socket.RemoteEndPoint);
                    }
                }
            }

            serverManager.CloseConnection();
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
