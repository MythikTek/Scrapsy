using scrapsy.Enums;
using Spectre.Console;
using System;

namespace scrapsy.Interfaces
{
    public interface ILoggerService
    {
        LoggerLevel LogLevel { get; set; }

        public void LogTrace(object message);

        public void LogDebug(object message);

        public void LogInfo(object message);

        public void LogWarning(object message);

        public void LogSevere(object message);

        public void LogException(Exception ex);

        public void SaveLogs();

        public void StatusMessage(string message, Spinner spinner, Action<StatusContext> action);
    }
}