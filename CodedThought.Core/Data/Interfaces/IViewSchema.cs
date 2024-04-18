using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodedThought.Core.Data.Interfaces
{
    public interface IViewSchema : ITableSchema
    {
        /// <summary>
        /// Gets or sets the view definition.
        /// </summary>
        string SqlDefinition {  get; set; }
    }
}
