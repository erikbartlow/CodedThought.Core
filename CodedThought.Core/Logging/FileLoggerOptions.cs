using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace CodedThought.Core.Logging
{
    public class FileLoggerOptions
    {
        public virtual string FileName {  get; set; }
        public virtual string FolderPath {  get; set; }
        public virtual string? LeadingDateFormat {  get; set; }
        public virtual double? MaxLogFileSize { get; set; }
        public virtual int? MaxLogFileCount {  get; set; }
        public virtual LogLevel ConfiguredLogLevel { get; set; }
    }
}
