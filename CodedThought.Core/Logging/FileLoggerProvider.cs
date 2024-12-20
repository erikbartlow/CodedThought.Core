using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodedThought.Core.Logging
{
    [ProviderAlias("FileLogger")]
    public class FileLoggerProvider : ILoggerProvider
    {
        public readonly FileLoggerOptions Options;
        public FileLoggerProvider(IOptions<FileLoggerOptions> options)
        {
            Options = options.Value;
            if( !Directory.Exists(Options.FolderPath)) 
                Directory.CreateDirectory(Options.FolderPath);
        }
        public ILogger CreateLogger(string categoryName) => new FileLogger(this);
        public void Dispose() => GC.SuppressFinalize(this);
    }
}
