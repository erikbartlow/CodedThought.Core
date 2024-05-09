using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodedThought.Core.Data.Interfaces
{
    public interface IDbSchema
    {
        string GetTableListQuery();
        string GetViewListQuery();
        string GetTableDefinitionQuery(string tableName);
        string GetViewDefinitionQuery(string viewName);
        List<TableColumn> GetTableDefinition(string tableName);
        List<TableColumn> GetViewDefinition(string viewName);
        List<TableSchema> GetTableDefinitions();
        List<ViewSchema> GetViewDefinitions();
    }

}
