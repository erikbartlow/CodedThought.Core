

using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

using Microsoft.Extensions.Logging;

namespace CodedThought.Core.Logging
{
    public class FileLogger : ILogger
    {
        private static readonly object _lock = new();
        protected readonly FileLoggerProvider _provider;
        protected const string DATEFORMAT = "yyyy-MM-dd HH:mm:ss+00:00";
        public FileLogger([NotNull] FileLoggerProvider provider)
        {
            _provider = provider;
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string fullFilePath = $"{_provider.Options.FolderPath}/{_provider.Options.FileName.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd"))}";
            string logEntryContent;
            if (String.IsNullOrEmpty(_provider.Options.LeadingDateFormat) == null)
                logEntryContent = $"[{DateTimeOffset.UtcNow.ToString(DATEFORMAT)}] {logLevel} {formatter(state, exception)} {(exception != null ? exception.StackTrace : "")}";
            else
                logEntryContent = $"{logLevel} {formatter(state, exception)} {(exception != null ? exception.StackTrace : "")}";

            using (StreamWriter streamWriter = new(fullFilePath, true))
                streamWriter.WriteLine(logEntryContent);

        }
    }
}
