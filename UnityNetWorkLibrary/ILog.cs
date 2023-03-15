namespace NetWorkLibrary
{
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
