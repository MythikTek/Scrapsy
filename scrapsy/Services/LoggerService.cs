using System;
using System.Collections.Generic;
using System.IO;
using scrapsy.Enums;
using scrapsy.Interfaces;
using Spectre.Console;

namespace scrapsy.Services
{
    public class LoggerService : ILoggerService
    {
        private List<string> _cachedLogs;
        
        public LoggerService()
        {
            var logsDirectory = Directory.GetCurrentDirectory() + @"\logs";
            
            
            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            _cachedLogs = new List<string>();
        }

        public LoggerLevel LogLevel { get; set; }

        public void LogTrace(object message)
        {
            var consoleMessage = $"[[{DateTime.Now}]] [[Trace]]: {message}";
            SaveMessage(consoleMessage);
            if (!LogLevel.HasFlag(LoggerLevel.All)) return;
            AnsiConsole.MarkupLine("[silver]" + consoleMessage + "[/]");
        }

        public void LogDebug(object message)
        {
            var consoleMessage = $"[[{DateTime.Now}]] [[Debug]]: {message}";
            SaveMessage(consoleMessage);
            if (!LogLevel.HasFlag(LoggerLevel.Debug)) return;
            AnsiConsole.MarkupLine("[mediumpurple1]" + consoleMessage + "[/]");
        }

        public void LogInfo(object message)
        {
            var consoleMessage = $"[[{DateTime.Now}]] [[Info]]: {message}";
            SaveMessage(consoleMessage);
            if (!LogLevel.HasFlag(LoggerLevel.Info)) return;
            AnsiConsole.MarkupLine("[teal]" + consoleMessage + "[/]");
        }

        public void LogWarning(object message)
        {
            var consoleMessage = $"[[{DateTime.Now}]] [[Warning]]: {message}";
            SaveMessage(consoleMessage);
            if (!LogLevel.HasFlag(LoggerLevel.Warning)) return;
            AnsiConsole.MarkupLine("[yellow1]" + consoleMessage + "[/]");
        }

        public void LogSevere(object message)
        {
            var consoleMessage = $"[[{DateTime.Now}]] [[Error]]: {message}";
            SaveMessage(consoleMessage);
            if (!LogLevel.HasFlag(LoggerLevel.Severe)) return;
            AnsiConsole.MarkupLine("[red]" + consoleMessage + "[/]");
            
        }

        public void LogException(Exception ex)
        {
            
            LogSevere("EXCEPTION OCCURED");
            
            SaveMessage(ex.GetType().FullName);
            SaveMessage("Source: " + ex.Source);
            SaveMessage("Message: " + ex.Message);
            SaveMessage("StackTrace: " + ex.StackTrace);
            SaveMessage("InnerException: " + ex.InnerException);
        }

        public void SaveLogs()
        {
            //no need to saved if no logs were written
            if (_cachedLogs.Count == 0)
                return;
            
            //create the log file
            var logsDirectory = Directory.GetCurrentDirectory() + @"\logs";
            var currentTime = DateTime.Now.ToString("yyyy-dd-M HH-mm-ss");
            var logFile = logsDirectory + @"\" + currentTime + @".txt";
            
            //write the logs to the log file
            using var writer = File.AppendText(logFile);
            foreach (var log in _cachedLogs)
            {
                writer.WriteLine(log);
            }
        }

        public void StatusMessage(string message, Spinner spinner, Action<StatusContext> action)
        {
            AnsiConsole.Status()
                .Spinner(spinner)
                .Start($"[yellow]{message}[/]", ctx => action);
        }

        private void SaveMessage(string message)
        {
            _cachedLogs.Add(message);
        }
    }
}