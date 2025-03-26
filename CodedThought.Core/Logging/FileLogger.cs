

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

            string fullFilePath = $"{_provider.Options.FolderPath}\\{_provider.Options.FileName.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd"))}";
            string logEntryContent;

            int currentLogFileCount = CountLogFiles(fullFilePath);
            if (_provider.Options.MaxLogFileCount == 1)
            {
            }
            else
            {
                // Delete the oldest file to stay under the max log file count.
                if (currentLogFileCount > _provider.Options.MaxLogFileCount)
                    DeleteOldestFile(_provider.Options.FolderPath, _provider.Options.FileName.Replace("{date}", "*"));

                string filePattern = _provider.Options.FileName.Replace("{date}", "*");
                string currentFile = GetCurrentFile(_provider.Options.FolderPath, filePattern) ?? fullFilePath;
                if (String.IsNullOrEmpty(currentFile))
                {
                    // Create a new log file since there isn't one already
                    string ext = Path.GetExtension(fullFilePath);
                    fullFilePath = $"{_provider.Options.FolderPath}\\{Path.GetFileNameWithoutExtension(fullFilePath)}_{String.Format("{0:D2}", ++currentLogFileCount)}{ext}";
                }
                else
                {
                    // If the current file is greater than or equal to the max file size create a new file.
                    if (GetFileSize(currentFile) >= _provider.Options.MaxLogFileSize)
                    {
                        // Create a new log file if max count is met.
                        string ext = Path.GetExtension(fullFilePath);
                        fullFilePath = $"{_provider.Options.FolderPath}\\{Path.GetFileNameWithoutExtension(fullFilePath)}_{String.Format("{0:D2}", ++currentLogFileCount)}{ext}";
                    }
                    else
                    {
                        fullFilePath = currentFile;
                    }
                }
            }
            // Write out the content
            if (!String.IsNullOrEmpty(_provider.Options.LeadingDateFormat) == null)
                logEntryContent = $"[{DateTimeOffset.UtcNow.ToString(DATEFORMAT)}] {logLevel} {formatter(state, exception)} {(exception != null ? exception.StackTrace : "")}";
            else
                logEntryContent = $"{logLevel} {formatter(state, exception)} {(exception != null ? exception.StackTrace : "")}";

            using StreamWriter streamWriter = new(fullFilePath, true);
            streamWriter.WriteLine(logEntryContent);

        }

        private int CountLogFiles(string filePath) => Directory.GetFiles(Path.GetDirectoryName(filePath), $"*.{Path.GetExtension(filePath)}").Length;

        private static double GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
                return 0;

            FileInfo fileInfo = new FileInfo(filePath);
            return Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2);
        }
        private bool CreateNewFile(string filePath)
        {
            if (!File.Exists(filePath))
                return true;
            if (GetFileSize(filePath) > Convert.ToDouble(_provider.Options.MaxLogFileSize))
                return true;

            return false;
        }

        private static string GetCurrentFile(string directoryPath, string searchPattern)
        {
            if (!Directory.Exists(directoryPath))
                return string.Empty;

            string[] files = Directory.GetFiles(directoryPath, searchPattern);

            FileInfo? newestFile = files
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .FirstOrDefault();
            if(newestFile == null) return string.Empty;

            if (newestFile.LastWriteTime.ToString("MM/dd/yyyy") == DateTime.Now.ToString("MM/dd/yyyy"))
                return newestFile != null ? newestFile.FullName : string.Empty;
            else
                return string.Empty;
        }
        private static void DeleteOldestFile(string directoryPath, string searchPattern)
        {
            if (!Directory.Exists(directoryPath))
                return;

            string[] files = Directory.GetFiles(directoryPath, searchPattern);

            FileInfo? oldestFile = files
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.CreationTime)
                .FirstOrDefault();

            if (oldestFile != null)
                File.Delete(oldestFile.FullName);
            return;
        }
    }
}