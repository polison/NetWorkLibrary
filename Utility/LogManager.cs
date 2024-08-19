using System;
using System.Collections.Generic;

namespace NetWorkLibrary.Utility
{
    public class LogManager
    {
        private static LogManager instance;
        public static LogManager Instance => instance ?? (instance = new LogManager());

        private List<ILog> logs = new List<ILog>();

        public void Log(LogType type, string format, params object[] args)
        {
            foreach (var log in logs)
            {
                lock (log)
                {
                    switch (type)
                    {
                        case LogType.Message:
                            log.Message($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-{format}", args);
                            break;
                        case LogType.Warning:
                            log.Warning($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-{format}", args);
                            break;
                        case LogType.Error:
                            log.Error($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-{format}", args);
                            break;
                    }
                }
            }
        }

        public void AddLogger(ILog log)
        {
            logs.Add(log);
        }
    }

    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// 打印消息
        /// </summary>
        void Message(string format, params object[] args);

        /// <summary>
        /// 打印警告
        /// </summary>
        void Warning(string format, params object[] args);

        /// <summary>
        /// 打印错误
        /// </summary>
        void Error(string format, params object[] args);
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        Message,

        /// <summary>
        /// 警告类型
        /// </summary>
        Warning,

        /// <summary>
        /// 错误类型
        /// </summary>
        Error
    }
}
