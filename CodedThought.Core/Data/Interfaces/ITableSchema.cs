using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodedThought.Core.Data.Interfaces
{
    public interface ITableSchema
    {
        /// <summary>
        /// Gets or sets the owner of the table.  In the case of Sql Server this is schema.
        /// However, Oracle calls them owner since they can be more than just a schema.
        /// </summary>
        string Owner { get; set; }
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets if this object is a view.
        /// </summary>
        bool IsView {  get; set; }
        /// <summary>
        /// Gets or sets the associated columns.
        /// </summary>
        List<TableColumn> Columns { get; set; }

    }
}
