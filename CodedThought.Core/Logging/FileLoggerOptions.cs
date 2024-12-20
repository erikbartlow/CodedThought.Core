using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodedThought.Core.Logging
{
    public class FileLoggerOptions
    {
        public virtual string FileName {  get; set; }
        public virtual string FolderPath {  get; set; }
        public virtual string? LeadingDateFormat {  get; set; }
    }
}
