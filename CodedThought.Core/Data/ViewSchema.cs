using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data
{
    public class ViewSchema : TableSchema
    {
        public ViewSchema() : base()
        {
            SqlDefinition = string.Empty;
        }
        public string? SqlDefinition {  get; set; }
    }
}
