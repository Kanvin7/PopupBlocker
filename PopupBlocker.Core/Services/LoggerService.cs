namespace PopupBlocker.Core.Services
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class LoggerService : IService
    {
        #region IService
        public bool Switch { get; set; }
        #endregion

        public LoggerService() { }
        public event Action<string>? LogWritingEvent;

        private void LogWriting(LogLevel level, string message)
        {
            if (!Switch)
                return;

            // 目前日志仅需通过事件触发，不直接写入文件或控制台
            // 后续可根据需要扩展日志的输出方式，例如写入文件或控制台等
            LogWritingEvent?.Invoke($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}\n");
        }

        public void Log(LogLevel level, string message)
        {
#if DEBUG
            // 在 Debug 环境下，所有日志级别都输出
            LogWriting(level, message);
#else
            // 在 Release 环境下，Debug 级别的日志不输出
            if (level != LogLevel.Debug)
                LogWriting(level, message);
#endif
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message) => Log(LogLevel.Error, message);
    }
}
