using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data
{
    public class TableSchema : ITableSchema
    {
        public TableSchema()
        {
            Owner = string.Empty;
            Name = string.Empty;
            IsView = false;
            Columns = [];
        }
        public string Owner { get; set; }
        public string Name { get; set; }
        public bool IsView {  get; set; }
        public List<TableColumn> Columns { get; set; }
    }
}
