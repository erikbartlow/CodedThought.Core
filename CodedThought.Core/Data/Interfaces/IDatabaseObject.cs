using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace CodedThought.Core.Data.Interfaces {

	public interface IDatabaseObject {


		#region Properties

		string ConnectionName { get; }

		ServiceLifetime ServiceLifetime { get; }

		string ColumnDelimiter { get; set; }

		int CommandTimeout { get; set; }

		IDbConnection Connection { get; }

		string ConnectionString { get; set; }

		string DefaultSchemaName { get; set; }

		CommandBehavior DataReaderBehavior { get; set; }

		string ParameterConnector { get; set; }

		DBSupported SupportedDatabase { get; }

		string WildCardCharacter { get; set; }

		#endregion Properties

		#region Methods and Events

		event SqlRowsCopiedEventHandler BulkCopySqlRowsCopied;

		void Add(string tableName, object obj, List<TableColumn> columns, IDBStore store);

		DataSet GetDataSet(string tableName, string schemaName, List<string> selectColumns, ParameterCollection parameters);

		IDbTransaction BeginTransaction();

		void CloseConnection();

		void Commit();

		void CommitTransaction();

		string ConvertToChar(string columnName);


		IDataParameter CreateBooleanParameter(string srcTableColumnName, bool parameterValue);

		IDataParameter CreateCharParameter(string srcTableColumnName, string parameterValue, int size);

		IDataParameter CreateDateTimeParameter(string srcTableColumnName, DateTime parameterValue);

		IDataParameter CreateDoubleParameter(string srcTableColumnName, double parameterValue);

		IDataParameter CreateEmptyParameter();

		IDataParameter CreateGuidParameter(string srcTableColumnName, Guid parameterValue);

		IDataParameter CreateInt32Parameter(string srcTableColumnName, int parameterValue);

		IDataParameter CreateOutputParameter(string parameterName, DbTypeSupported returnType);

		IDataParameter CreateParameter(object obj, TableColumn col, IDBStore store);

		IDataParameter CreateReturnParameter(string parameterName, DbTypeSupported returnType);

		IDataParameter CreateStringParameter(string srcTableColumnName, string parameterValue);

		string CreateWhere(ParameterCollection parameters, string tableColumnName, int parameterValue);

		string CreateWhere(ParameterCollection parameters, string tableColumnName, string parameterValue);

		string CreateWhere(ParameterCollection parameters, string tableColumnName, double parameterValue);

		string CreateWhere(ParameterCollection parameters, string tableColumnName, DateTime parameterValue);

		bool ExecuteBulkCopy(DataTable records, int notificationRecordInterval, string destinationTable = null);

		void ExecuteBulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e);

		DataSet ExecuteDataSet(string commandText);

		DataSet ExecuteDataSet(string commandText, ParameterCollection parameters);

		DataSet ExecuteDataSet(string commandText, CommandType type);

		DataSet ExecuteDataSet(string commandText, CommandType type, ParameterCollection parameters);

		DataTable ExecuteDataTable(string commandText);

		int ExecuteNonQuery(string commandText);

		int ExecuteNonQuery(string commandText, ParameterCollection parameters);

		int ExecuteNonQuery(string commandText, CommandType type);

		int ExecuteNonQuery(string commandText, CommandType type, ParameterCollection parameters);

		IDataReader ExecuteReader(string commandText);

		IDataReader ExecuteReader(string commandText, ParameterCollection parameters);

		IDataReader ExecuteReader(string commandText, CommandType type);

		IDataReader ExecuteReader(string commandText, CommandType type, ParameterCollection parameters);

		IDataReader ExecuteReader(string commandText, CommandType type, ParameterCollection parameters, CommandBehavior commandBehavior);

		Task<ApiDataReader> ExecuteReader(string controller, string action, ParameterCollection parameters);

		object ExecuteScalar(string commandText, CommandType type, ParameterCollection parameters);

		IDataParameter CreateXMLParameter(string srcTaleColumnName, string parameterValue);

		string GenerateColumnList(List<string> columnList);

		string GenerateColumnList(List<string> columnList, string tableAlias);

		string GenerateOrderByClause(List<string> columnList);

		string GenerateUpdateList(ParameterCollection parameters);

		bool GetBitValue(IDataReader reader, string columnName);

		string GetTableName(string defaultSchema, string tableName);

		string GetSchemaName();

		IDataReader Get(string sourceName, List<string> selectColumns, ParameterCollection parameters);

		IDataReader Get(string sql, CommandType commandType, ParameterCollection parameters);

		IDataReader Get(string sourceName, List<string> selectColumns, ParameterCollection parameters, List<string> orderByColumns);

		IDataReader Get(string sourceName, string schemaName, List<string> selectColumns, ParameterCollection parameters);

		IDataReader Get(string sourceName, string schemaName, List<string> selectColumns, ParameterCollection parameters, List<string> orderByColumns);

		Task<ApiDataReader> Get(string controller, string action, ParameterCollection parameters);

		string GetCaseDecode(string columnName, string equalValue, string trueValue, string falseValue, string alias);

		string GetCharValue(IDataReader reader, string columnName);

		string GetCurrentDateFunction();

		string GetDateOnlySqlSyntax(string dateColumn);

		string GetDatePart(string datestring, DateFormat dateFormat, DatePart datePart);

		DateTime GetDateTimeValue(IDataReader reader, string columnName);

		string GetDateToStringForColumn(string columnName, DateFormat dateFormat);

		string GetDateToStringForValue(string value, DateFormat dateFormat);

		double GetDoubleValue(IDataReader reader, string columnName);

		string GetFunctionName(FunctionName functionName);

		string GetIfNullFunction();

		string GetIfNullFunction(string validateColumnName, string optionColumnName);

		int GetInt32Value(IDataReader reader, string columnName);

		string GetStringFromBlob(IDataReader reader, string columnName);

		string GetStringToDateSqlSyntax(string dateString);

		string GetStringToDateSqlSyntax(DateTime dateSQL);

		List<TableColumn> GetTableDefinition(string tableName);

		string GetVarCharValue(IDataReader reader, string columnName);

		string GetYearSQLSyntax(string dateString);

		void Remove(string tableName, ParameterCollection parameters);

		void RollbackTransaction();

		bool TestConnection();

		string ToDate(string datestring, DateFormat dateFormat);

		Type ToSystemType(string dbTypeName);

		void Update(string tableName, ParameterCollection parameters, ParameterCollection whereParamCollection);

		#endregion Methods and Events
	}
}