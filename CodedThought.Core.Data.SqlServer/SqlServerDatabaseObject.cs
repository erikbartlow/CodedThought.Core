using CodedThought.Core.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CodedThought.Core.Data.SqlServer {

	/// <summary>SqlServerDatabaseObject provides all SQLServer specific functionality needed by DBStore and its family of classes..</summary>
	public class SqlServerDatabaseObject : DatabaseObject, IDatabaseObject {

		#region Constructor

		/// <summary>Constructor</summary>
		/// <param name="connectionString"></param>
		/// Connection string to the database
		/// <param name="auditUserName">   </param>
		/// Name of the user so an audit log can be made
		public SqlServerDatabaseObject(string connectionString)
		   : base(DBSupported.SqlServer, connectionString) {
		}

		#endregion Constructor

		#region Transaction and Connection Methods

		/// <summary>Commits updates and inserts. This is only for Oracle database operations.</summary>
		public override void Commit() {
			CommitTransaction();
		}


		/// <summary>Opens an SqlServer Connection</summary>
		/// <returns></returns>
		protected override IDbConnection OpenConnection() {
			SqlConnection? sqlCn;
			try {
				sqlCn = new SqlConnection(this.ConnectionString);
				sqlCn.Open();
				return sqlCn;
			} catch (SqlException ex) {
				throw new Exceptions.CodedThoughtApplicationException("Could not open Connection.  Check connection string" + "/r/n" + ex.Message + "/r/n" + ex.StackTrace, ex);
			}
		}

		#endregion Transaction and Connection Methods

		#region Other Override Methods

		/// <summary>
		/// Tests the connection to the database.
		/// </summary>
		/// <returns></returns>
		public override bool TestConnection() {
			try {
				this.OpenConnection();
				return this.Connection.State == ConnectionState.Open;
			} catch (CodedThoughtException ex) {
				throw ex;
			}
		}
		/// <summary>
		/// Creates a Sql Data Adapter object with the passed Command object.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		protected override IDataAdapter CreateDataAdapter(IDbCommand cmd) {
			return new SqlDataAdapter(cmd as SqlCommand);
		}

		/// <summary>Convert any data type to Char</summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public override string ConvertToChar(string columnName) {
			return "CONVERT(varchar, " + columnName + ")";
		}
		/// <summary>Creates the parameter collection.</summary>
		/// <returns></returns>
		public override ParameterCollection CreateParameterCollection() {
			return new ParameterCollection(this);
		}

		public override IDataParameter CreateApiParameter(string paraemterName, string parameterValue) {
			throw new NotImplementedException();
		}

		#region Parameters

		/// <summary>Returns the param connector for SQLServer, @</summary>
		/// <returns></returns>
		public override string ParameterConnector {
			get {
				return "@";
			}
		}

		/// <summary>Gets the wild card character.</summary>
		/// <value>The wild card character.</value>
		public override string WildCardCharacter {
			get { return "%"; }
		}

		/// <summary>Gets the column delimiter character.</summary>
		public override string ColumnDelimiter {
			get { throw new NotImplementedException(); }
		}

		/// <summary>Creates the SQL server param.</summary>
		/// <param name="srcTableColumnName">Name of the SRC table column.</param>
		/// <param name="paramType">         Type of the param.</param>
		/// <returns></returns>
		private SqlParameter CreateSqlServerParam(string srcTableColumnName, SqlDbType paramType) {
			SqlParameter param = new(ToSafeParamName(srcTableColumnName), paramType);
			param.SourceColumn = srcTableColumnName;
			return param;
		}

		/// <summary>Creates the SQL server param.</summary>
		/// <param name="srcTableColumnName">Name of the SRC table column.</param>
		/// <param name="paramType">         Type of the param.</param>
		/// <param name="size">              The size.</param>
		/// <returns></returns>
		private SqlParameter CreateSqlServerParam(string srcTableColumnName, SqlDbType paramType, int size) {
			SqlParameter param = new(ToSafeParamName(srcTableColumnName), paramType, size);
			param.SourceColumn = srcTableColumnName;
			return param;
		}

		/// <summary>Creates the XML parameter.</summary>
		/// <param name="srcTaleColumnName">Name of the SRC tale column.</param>
		/// <param name="parameterValue">   The parameter value.</param>
		/// <returns></returns>
		public override IDataParameter CreateXMLParameter(string srcTaleColumnName, string parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTaleColumnName, SqlDbType.Xml);
			returnValue.Value = parameterValue != string.Empty ? parameterValue : DBNull.Value;
			return returnValue;
		}

		/// <summary>Creates a boolean parameter.</summary>
		/// <param name="srcTaleColumnName">Name of the SRC tale column.</param>
		/// <param name="parameterValue">   The parameter value.</param>
		/// <returns></returns>
		public override IDataParameter CreateBooleanParameter(string srcTableColumnName, bool parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.Bit);
			returnValue.Value = parameterValue;
			return returnValue;
		}

		/// <summary>Creates parameters for the supported database.</summary>
		/// <param name="obj">  The Business Entity from which to extract the data</param>
		/// <param name="col">  The column for which the data must be extracted from the business entity</param>
		/// <param name="store">The store that handles the IO</param>
		/// <returns></returns>
		public override IDataParameter CreateParameter(object obj, TableColumn col, IDBStore store) {
			Boolean isNull = false;
			int sqlDataType = 0;

			object extractedData = store.Extract(obj, col.name);
			try {
				switch (col.type) {
					case DbTypeSupported.dbNVarChar:
						isNull = (extractedData == null || (string)extractedData == "");
						sqlDataType = (int)SqlDbType.NVarChar;
						break;

					case DbTypeSupported.dbVarChar:
						isNull = (extractedData == null || (string)extractedData == "");
						sqlDataType = (int)SqlDbType.VarChar;
						break;

					case DbTypeSupported.dbInt64:
						isNull = ((Int64)extractedData == Int64.MinValue);
						sqlDataType = (int)SqlDbType.BigInt;
						break;

					case DbTypeSupported.dbInt32:
						isNull = ((Int32)extractedData == int.MinValue);
						sqlDataType = (int)SqlDbType.Int;
						break;

					case DbTypeSupported.dbInt16:
						isNull = ((Int16)extractedData == Int16.MinValue);
						sqlDataType = (int)SqlDbType.SmallInt;
						break;

					case DbTypeSupported.dbDouble:
						isNull = ((double)extractedData == double.MinValue);
						sqlDataType = (int)SqlDbType.Float;
						break;

					case DbTypeSupported.dbDateTime:
						isNull = ((DateTime)extractedData == DateTime.MinValue);
						sqlDataType = (int)SqlDbType.DateTime;
						break;

					case DbTypeSupported.dbChar:
						isNull = (extractedData == null || System.Convert.ToString(extractedData) == "");
						sqlDataType = (int)SqlDbType.Char;
						break;

					case DbTypeSupported.dbBlob:    // Text, not Image
						isNull = (extractedData == null);
						sqlDataType = (int)SqlDbType.Binary;
						break;

					case DbTypeSupported.dbBit:
						isNull = (extractedData == null);
						sqlDataType = (int)SqlDbType.Bit;
						break;

					case DbTypeSupported.dbDecimal:
						isNull = ((decimal)extractedData == decimal.MinValue);
						sqlDataType = (int)SqlDbType.Decimal;
						break;

					case DbTypeSupported.dbImage:
					case DbTypeSupported.dbVarBinary:
						isNull = (extractedData == null);
						sqlDataType = (int)SqlDbType.VarBinary;
						break;

					case DbTypeSupported.dbGUID:
						isNull = ((Guid)extractedData == Guid.Empty);
						sqlDataType = (int)SqlDbType.UniqueIdentifier;
						break;

					default:
						throw new Exceptions.CodedThoughtApplicationException("Data type not supported.  DataTypes currently supported are: DbTypeSupported.dbString, DbTypeSupported.dbInt32, DbTypeSupported.dbDouble, DbTypeSupported.dbDateTime, DbTypeSupported.dbChar");
				}
			} catch (Exception ex) {
				throw new Exceptions.CodedThoughtApplicationException("Error creating Parameter", ex);
			}

			SqlParameter parameter = CreateSqlServerParam(col.name, (SqlDbType)sqlDataType);

			parameter.Value = isNull ? DBNull.Value : extractedData;

			return parameter;
		}

		/// <summary>Create an empty parameter for SQLServer</summary>
		/// <returns></returns>
		public override IDataParameter CreateEmptyParameter() {
			IDataParameter returnValue = null;

			returnValue = new SqlParameter();

			return returnValue;
		}

        /// <summary>Creates the output parameter.</summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="returnType">   Type of the return.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.CodedThoughtApplicationException">
        /// Data type not supported. DataTypes currently supported are: DbTypeSupported.dbString, DbTypeSupported.dbInt32, DbTypeSupported.dbDouble, DbTypeSupported.dbDateTime, DbTypeSupported.dbChar
        /// </exception>
        public override IDataParameter CreateOutputParameter(string parameterName, DbTypeSupported returnType) {
			IDataParameter returnParam = null;
			SqlDbType sqlType;
			switch (returnType) {
				case DbTypeSupported.dbNVarChar:
					sqlType = SqlDbType.NVarChar;
					break;

				case DbTypeSupported.dbVarChar:
					sqlType = SqlDbType.VarChar;
					break;

				case DbTypeSupported.dbInt64:
					sqlType = SqlDbType.BigInt;
					break;

				case DbTypeSupported.dbInt32:
					sqlType = SqlDbType.Int;
					break;

				case DbTypeSupported.dbInt16:
					sqlType = SqlDbType.SmallInt;
					break;

				case DbTypeSupported.dbDouble:
					sqlType = SqlDbType.Float;
					break;

				case DbTypeSupported.dbDateTime:
					sqlType = SqlDbType.DateTime;
					break;

				case DbTypeSupported.dbChar:
					sqlType = SqlDbType.Char;
					break;

				case DbTypeSupported.dbBlob:    // Text, not Image
					sqlType = SqlDbType.Binary;
					break;

				case DbTypeSupported.dbBit:
					sqlType = SqlDbType.Bit;
					break;

				case DbTypeSupported.dbDecimal:
					sqlType = SqlDbType.Decimal;
					break;

				case DbTypeSupported.dbImage:
					sqlType = SqlDbType.Image;
					break;

				case DbTypeSupported.dbGUID:
					sqlType = SqlDbType.UniqueIdentifier;
					break;

				default:
					throw new Exceptions.CodedThoughtApplicationException("Data type not supported.  DataTypes currently supported are: DbTypeSupported.dbString, DbTypeSupported.dbInt32, DbTypeSupported.dbDouble, DbTypeSupported.dbDateTime, DbTypeSupported.dbChar");
			}

			returnParam = CreateSqlServerParam(parameterName, sqlType);
			returnParam.Direction = ParameterDirection.Output;
			return returnParam;
		}

        /// <summary>Creates and returns a return parameter for the supported database.</summary>
        /// <param name="parameterName"></param>
        /// <param name="returnType">   </param>
        /// <returns></returns>
        /// <exception cref="Exceptions.CodedThoughtApplicationException">
        /// Data type not supported. DataTypes currently supported are: DbTypeSupported.dbString, DbTypeSupported.dbInt32, DbTypeSupported.dbDouble, DbTypeSupported.dbDateTime, DbTypeSupported.dbChar
        /// </exception>
        public override IDataParameter CreateReturnParameter(string parameterName, DbTypeSupported returnType) {
			IDataParameter returnParam = null;
			SqlDbType sqlType;
			switch (returnType) {
				case DbTypeSupported.dbNVarChar:
					sqlType = SqlDbType.NVarChar;
					break;

				case DbTypeSupported.dbVarChar:
					sqlType = SqlDbType.VarChar;
					break;

				case DbTypeSupported.dbInt64:
					sqlType = SqlDbType.BigInt;
					break;

				case DbTypeSupported.dbInt32:
					sqlType = SqlDbType.Int;
					break;

				case DbTypeSupported.dbInt16:
					sqlType = SqlDbType.SmallInt;
					break;

				case DbTypeSupported.dbDouble:
					sqlType = SqlDbType.Float;
					break;

				case DbTypeSupported.dbDateTime:
					sqlType = SqlDbType.DateTime;
					break;

				case DbTypeSupported.dbChar:
					sqlType = SqlDbType.Char;
					break;

				case DbTypeSupported.dbBlob:    // Text, not Image
					sqlType = SqlDbType.Binary;
					break;

				case DbTypeSupported.dbBit:
					sqlType = SqlDbType.Bit;
					break;

				case DbTypeSupported.dbDecimal:
					sqlType = SqlDbType.Decimal;
					break;

				case DbTypeSupported.dbImage:
					sqlType = SqlDbType.Image;
					break;

				case DbTypeSupported.dbGUID:
					sqlType = SqlDbType.UniqueIdentifier;
					break;

				default:
					throw new Exceptions.CodedThoughtApplicationException("Data type not supported.  DataTypes currently supported are: DbTypeSupported.dbString, DbTypeSupported.dbInt32, DbTypeSupported.dbDouble, DbTypeSupported.dbDateTime, DbTypeSupported.dbChar");
			}

			returnParam = CreateSqlServerParam(parameterName, sqlType);
			returnParam.Direction = ParameterDirection.ReturnValue;
			return returnParam;
		}

		/// <summary>Creates and returns a string parameter for the supported database.</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns></returns>
		public override IDataParameter CreateStringParameter(string srcTableColumnName, string parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.NVarChar);
			returnValue.Value = parameterValue != string.Empty ? parameterValue : DBNull.Value;

			return returnValue;
		}

		/// <summary>Creates a Int32 parameter for the supported database</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns></returns>
		public override IDataParameter CreateInt32Parameter(string srcTableColumnName, int parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.Int);
			returnValue.Value = parameterValue != int.MinValue ? parameterValue : DBNull.Value;

			return returnValue;
		}

		/// <summary>Creates a Double parameter based on supported database</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns></returns>
		public override IDataParameter CreateDoubleParameter(string srcTableColumnName, double parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.Float);
			returnValue.Value = parameterValue != double.MinValue ? parameterValue : DBNull.Value;

			return returnValue;
		}

		/// <summary>Create a data time parameter based on supported database.</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns></returns>
		public override IDataParameter CreateDateTimeParameter(string srcTableColumnName, DateTime parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.DateTime);
			returnValue.Value = parameterValue != DateTime.MinValue ? parameterValue : DBNull.Value;

			return returnValue;
		}

		/// <summary>Creates a Char parameter based on supported database.</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <param name="size">              </param>
		/// <returns></returns>
		public override IDataParameter CreateCharParameter(string srcTableColumnName, string parameterValue, int size) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.VarChar);
			returnValue.Value = parameterValue != string.Empty ? parameterValue : DBNull.Value;

			return returnValue;
		}

		/// <summary>Creates a Blob parameter based on supported database.</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <param name="size">              </param>
		/// <returns></returns>
		public IDataParameter CreateBlobParameter(string srcTableColumnName, byte[] parameterValue, int size) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.Text, size);
			returnValue.Value = parameterValue;

			return returnValue;
		}

		/// <summary>Creates the GUID parameter.</summary>
		/// <param name="srcTableColumnName">Name of the SRC table column.</param>
		/// <param name="parameterValue">    The parameter value.</param>
		/// <returns></returns>
		public override IDataParameter CreateGuidParameter(string srcTableColumnName, Guid parameterValue) {
			IDataParameter returnValue = null;

			returnValue = CreateSqlServerParam(srcTableColumnName, SqlDbType.UniqueIdentifier);
			returnValue.Value = parameterValue;

			return returnValue;
		}

		public override IDataParameter CreateBetweenParameter(string srcTableColumnName, BetweenParameter betweenParam) {
			throw new NotImplementedException();
		}

		#endregion Parameters

		#region Add method

		/// <summary>Adds data to the database</summary>
		/// <param name="tableName"></param>
		/// <param name="obj">      </param>
		/// <param name="columns">  </param>
		/// <param name="store">    </param>
		/// <returns></returns>

		public override void Add(string tableName, object obj, List<TableColumn> columns, IDBStore store) {
			try {
                ParameterCollection parameters = new();
                StringBuilder sbColumns = new();
                StringBuilder sbValues = new();

				for (int i = 0; i < columns.Count; i++) {
                    TableColumn col = columns[i];

					if (col.isInsertable) {
                        //we do not insert columns such as identity columns
                        IDataParameter parameter = CreateParameter(obj, col, store);
						sbColumns.Append(__comma).Append(col.name);
						sbValues.Append(__comma).Append(this.ParameterConnector).Append(parameter.ParameterName);
						parameters.Add(parameter);
					}
				}

                StringBuilder sql = new("INSERT INTO " + tableName + " (");
				sql.Append(sbColumns.Remove(0, 2));
				sql.Append(") VALUES (");
				sql.Append(sbValues.Remove(0, 2));
				sql.Append(") ");

                // ================================================================ print sql to output window to debugging purpose
#if DEBUG
                DebugParameters(sql, tableName, parameters);
#endif
                // ================================================================
                BeginTransaction();
				if (store.HasKeyColumn(obj)) {
					//Check if we have an identity Column
					sql.Append("SELECT SCOPE_IDENTITY() ");
					// ExecuteScalar will execute both the INSERT statement and the SELECT statement.
					int retval = System.Convert.ToInt32(this.ExecuteScalar(sql.ToString(), System.Data.CommandType.Text, parameters));
					store.SetPrimaryKey(obj, retval);
				} else {
					this.ExecuteNonQuery(sql.ToString(), System.Data.CommandType.Text, parameters);
				}

				// this is the way to get the CONTEXT_INFO of a SQL connection session string contextInfo = System.Convert.ToString( this.ExecuteScalar( "SELECT dbo.AUDIT_LOG_GET_USER_NAME() ",
				// System.Data.CommandType.Text, null ) );
			} catch (Exceptions.CodedThoughtApplicationException irEx) {
                RollbackTransaction();
				// this is not a good method to catch DUPLICATE
				if (irEx.Message.IndexOf("duplicate key") >= 0) {
					throw new FolderException(irEx.Message, (Exception)irEx);
				} else {
					throw new Exceptions.CodedThoughtApplicationException((string)("Failed to add record to: " + tableName + "<BR>" + irEx.Message + "<BR>" + irEx.Source), (Exception)irEx);
				}
			} catch (Exception ex) {
                RollbackTransaction();
				throw new Exceptions.CodedThoughtApplicationException("Failed to add record to: " + tableName + "<BR>" + ex.Message + "<BR>" + ex.Source, ex);
			} finally {
                CommitTransaction();
			}
		}

		#endregion Add method

		#region GetValue Methods

		/// <summary>
		/// Get a BLOB from a TEXT or IMAGE column.
		/// In order to get BLOB, a IDataReader's CommandBehavior must be set to SequentialAccess.
		/// That also means to Get columns in sequence is extremely important.
		/// Otherwise the GetBlobValue method won't return correct data.
		/// [EXAMPLE]
		/// this.DataReaderBehavior = CommandBehavior.SequentialAccess;
		///	using(IDataReader reader = this.ExecuteReader("select BigName, ID, BigBlob from BigTable", CommandType.Text))
		///	{
		///		while (reader.Read())
		///		{
		///			string bigName = reader.GetString(0);
		///			int id = this.GetInt32Value( reader, 1);
		///			byte[] bigText = this.GetBlobValue( reader, 2 );
		///		}
		///	}
		/// </summary>
		/// <param name="reader"></param>
		///<param name="columnName"></param>
		/// <returns></returns>
		protected override byte[] GetBlobValue(IDataReader reader, string columnName) {
			int position = reader.GetOrdinal(columnName);

			// The DataReader's CommandBehavior must be CommandBehavior.SequentialAccess.
			if (this.DataReaderBehavior != CommandBehavior.SequentialAccess) {
				throw new Exceptions.CodedThoughtApplicationException("Please set the DataReaderBehavior to SequentialAccess to call this method.");
			}
			SqlDataReader sqlReader = (SqlDataReader)reader;
			int bufferSize = 100;                   // Size of the BLOB buffer.
			byte[] outBuff = new byte[bufferSize];  // a buffer for every read in "bufferSize" bytes
			long totalBytes;                        // The total chars returned from GetBytes.
			long retval;                            // The bytes returned from GetBytes.
			long startIndex = 0;                    // The starting position in the BLOB output.
			byte[] outBytes = null;                 // The BLOB byte[] buffer holder.

			// Read the total bytes into outbyte[] and retain the number of chars returned.
			totalBytes = sqlReader.GetBytes(position, startIndex, outBytes, 0, bufferSize);
			outBytes = new byte[totalBytes];

			// initial reading from the BLOB column
			retval = sqlReader.GetBytes(position, startIndex, outBytes, 0, bufferSize);

			// Continue reading and writing while there are bytes beyond the size of the buffer.
			while (retval == bufferSize) {
				// Reposition the start index to the end of the last buffer and fill the buffer.
				startIndex += bufferSize;
				retval = sqlReader.GetBytes(position, startIndex, outBytes, System.Convert.ToInt32(startIndex), bufferSize);
			}

			return outBytes;
		}

		/// <summary>
		/// Gets a string from a BLOB, Text (SQLServer) or CLOB (Oracle),. developers should use
		/// this method only if they know for certain that the data stored in the field is a string.
		/// </summary>
		/// <param name="reader"></param>
		///<param name="columnName"></param>
		/// <returns></returns>
		public override string GetStringFromBlob(IDataReader reader, string columnName) {
			int position = reader.GetOrdinal(columnName);
			string returnValue = string.Empty;

			returnValue = System.Text.Encoding.ASCII.GetString(GetBlobValue(reader, columnName));

			return returnValue;
		}

		#endregion GetValue Methods

		#region Database Specific

		/// <summary>Gets the table definition.</summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		protected override String GetTableDefinitionQuery(string tableName) {
			try {
				StringBuilder sql = new();
				List<TableColumn> tableDefinition = new();
				// Remove any brackets since the definitiion query doesn't support that.
				string tName = tableName.Replace("[", "").Replace("]", "");
				string schemaName = DefaultSchemaName.Replace("[", "").Replace("]", "");
				if (tName.Split(".".ToCharArray()).Length > 0) {
					// The schema name appears to have been passed along with the table name. So parse them out and use them instead of the default values.
					string[] tableNameData = tName.Split(".".ToCharArray());
					schemaName = tableNameData[0].Replace("[", "").Replace("]", "");
					tName = tableNameData[1].Replace("[", "").Replace("]", "");
				}
				sql.Append("SELECT C.COLUMN_NAME, C.DATA_TYPE, ");
				sql.Append("CASE WHEN C.IS_NULLABLE = 'NO' THEN 0 ELSE 1 END as IS_NULLABLE, ");
				sql.Append("CASE WHEN C.CHARACTER_MAXIMUM_LENGTH IS NULL THEN 0 ELSE C.CHARACTER_MAXIMUM_LENGTH END AS CHARACTER_MAXIMUM_LENGTH, ");
				sql.Append("C.ORDINAL_POSITION - 1 as ORDINAL_POSITION,	COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') as IS_IDENTITY ");
				sql.Append("FROM INFORMATION_SCHEMA.COLUMNS C ");
				sql.Append("LEFT OUTER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K ON C.COLUMN_NAME = K.COLUMN_NAME AND C.TABLE_NAME = K.TABLE_NAME ");
				sql.AppendFormat("WHERE C.TABLE_NAME = '{0}' AND C.TABLE_SCHEMA = '{1}' ORDER BY C.ORDINAL_POSITION", tName, schemaName);

				return sql.ToString();
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Gets SQL syntax of Year</summary>
		/// <param name="dateString"></param>
		/// <returns></returns>
		public override string GetYearSQLSyntax(string dateString) {
			return "YEAR(" + dateString + ")";
		}

		/// <summary>Gets database function name</summary>
		/// <param name="functionName"></param>
		/// <returns></returns>
		public override string GetFunctionName(FunctionName functionName) {
			string retStr = string.Empty;
			switch (functionName) {
				case FunctionName.SUBSTRING:
					retStr = "SUBSTRING";
					break;

				case FunctionName.ISNULL:
					retStr = "ISNULL";
					break;

				case FunctionName.CURRENTDATE:
					retStr = "GETDATE()";
					break;

				case FunctionName.CONCATENATE:
					retStr = "+";
					break;
			}
			return retStr;
		}

		/// <summary>Gets Date string format.</summary>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="dateFormat">The date format.</param>
		/// <returns></returns>
		public override string GetDateToStringForColumn(string columnName, DateFormat dateFormat) {
			StringBuilder sb = new();
			switch (dateFormat) {
				case DateFormat.MMDDYYYY:
					sb.Append(" CONVERT(VARCHAR, ").Append(columnName).Append(", 101) ");
					break;

				case DateFormat.MMDDYYYY_Hyphen:
					sb.Append(" CONVERT(VARCHAR, ").Append(columnName).Append(", 110) ");
					break;

				case DateFormat.MonDDYYYY:
					sb.Append(" CONVERT(VARCHAR, ").Append(columnName).Append(", 107) ");
					break;

				default:
					sb.Append(columnName);
					break;
			}
			return sb.ToString();
		}

		/// <summary>Gets the date to string for value.</summary>
		/// <param name="value">     The value.</param>
		/// <param name="dateFormat">The date format.</param>
		/// <returns></returns>
		public override string GetDateToStringForValue(string value, DateFormat dateFormat) {
			StringBuilder sb = new();
			switch (dateFormat) {
				case DateFormat.MMDDYYYY:
					sb.Append(" CONVERT(VARCHAR, \"").Append(value).Append("\", 101) ");
					break;

				case DateFormat.MMDDYYYY_Hyphen:
					sb.Append(" CONVERT(VARCHAR, \"").Append(value).Append("\", 110) ");
					break;

				case DateFormat.MonDDYYYY:
					sb.Append(" CONVERT(VARCHAR, \"").Append(value).Append("\", 107) ");
					break;

				default:
					sb.Append("\"" + value + "\"");
					break;
			}
			return sb.ToString();
		}

		/// <summary>Get CASE (SQL Server) or DECODE (Oracle) SQL syntax.</summary>
		/// <param name="columnName"></param>
		/// <param name="equalValue"></param>
		/// <param name="trueValue"> </param>
		/// <param name="falseValue"></param>
		/// <param name="alias">     </param>
		/// <returns></returns>
		public override string GetCaseDecode(string columnName, string equalValue, string trueValue, string falseValue, string alias) {
			StringBuilder sb = new();

			sb.Append(" (CASE ").Append(columnName);
			sb.Append(" WHEN ").Append(equalValue);
			sb.Append(" THEN ").Append(trueValue).Append(" ELSE ").Append(falseValue).Append(" END) ");
			sb.Append(alias).Append(" ");

			return sb.ToString();
		}

		/// <summary>Get an IsNull (SQLServer) or NVL (Oracle)</summary>
		/// <param name="validateColumnName"></param>
		/// <param name="optionColumnName">  </param>
		/// <returns></returns>
		public override string GetIfNullFunction(string validateColumnName, string optionColumnName) {
			return " IsNULL(" + validateColumnName + ", " + optionColumnName + ") ";
		}

		/// <summary>Get a function name for NULL validation</summary>
		/// <returns></returns>
		public override string GetIfNullFunction() {
			return "IsNULL";
		}

		/// <summary>Get a function name that return current date</summary>
		/// <returns></returns>
		public override string GetCurrentDateFunction() {
			return "GETDATE()";
		}

		/// <summary>Get a database specific date only SQL syntax.</summary>
		/// <param name="dateColumn"></param>
		/// <returns></returns>
		public override string GetDateOnlySqlSyntax(string dateColumn) {
			return "CONVERT(VARCHAR, " + dateColumn + ", 107)";
		}

		/// <summary>Get a database specific syntax that converts string to date. Oracle does not convert date string to date implicitly like SQL Server does when there is a date comparison.</summary>
		/// <param name="dateString"></param>
		/// <returns></returns>
		public override string GetStringToDateSqlSyntax(string dateString) {
			return __singleQuote + dateString + __singleQuote + " ";
		}

		/// <summary>Get a database specific syntax that converts string to date. Oracle does not convert date string to date implicitly like SQL Server does when there is a date comparison.</summary>
		/// <param name="dateSQL"></param>
		/// <returns></returns>
		public override string GetStringToDateSqlSyntax(DateTime dateSQL) {
			return __singleQuote + dateSQL.ToString("G", System.Globalization.DateTimeFormatInfo.InvariantInfo) + __singleQuote + " ";
		}

		/// <summary>Gets date part(Day, month or year) of date</summary>
		/// <param name="datestring"></param>
		/// <param name="dateFormat"></param>
		/// <param name="datePart">  </param>
		/// <returns></returns>
		public override string GetDatePart(string datestring, DateFormat dateFormat, DatePart datePart) {
			string datePartstring = string.Empty;
			switch (datePart) {
				case DatePart.DAY:
					datePartstring = "day";
					break;

				case DatePart.MONTH:
					datePartstring = "month";
					break;

				case DatePart.YEAR:
					datePartstring = "year";
					break;
			}
			string result = "DATEPART( " + datePartstring + ", '" + datestring + "')";
			return result;
		}

		/// <summary>Convert a datestring to datetime when used for between.... and</summary>
		/// <param name="datestring">string</param>
		/// <param name="dateFormat">DateFormat</param>
		/// <returns></returns>
		public override string ToDate(string datestring, DateFormat dateFormat) {
			return __singleQuote + datestring + __singleQuote;
		}

		/// <summary>Converts a database type name to a system type.</summary>
		/// <param name="dbTypeName">Name of the db type.</param>
		/// <returns>System.Type</returns>
		public override System.Type ToSystemType(string dbTypeName) {
			switch (dbTypeName.ToLower()) {
				case "bigint":
					return typeof(System.Int64);

				case "varbinary":
				case "binary":
				case "timestamp":
					return typeof(System.Byte[]);

				case "bit":
					return typeof(System.Boolean);

				case "char":
				case "nchar":
				case "ntext":
				case "nvarchar":
				case "text":
				case "varchar":
					return typeof(System.String);

				case "date":
				case "datetime":
				case "datetime2":
					return typeof(System.DateTime);

				case "decimal":
				case "smallmoney":
				case "money":
					return typeof(System.Decimal);

				case "float":
					return typeof(System.Double);

				case "int":
					return typeof(System.Int32);

				case "smallint":
					return typeof(System.Int16);

				case "variant":
					return typeof(System.Object);

				case "time":
					return typeof(System.TimeSpan);

				case "tinyint":
					return typeof(System.Byte);

				case "uniqueidentifier":
					return typeof(System.Guid);

				default:
					return typeof(System.String);
			}
		}

		#endregion Database Specific


		#endregion Other Override Methods
	}
}