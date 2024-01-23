using Microsoft.Data.SqlClient;

namespace CodedThought.Core.Data {

    /// <summary>
    /// DatabaseObject encapsulates functionality of executing queries and non queries on a database. The DatabaseObject maintains connections, and transactions related to any database activity.
    /// </summary>
    public abstract class DatabaseObject : IDatabaseObject {

        #region private constant

        /// <summary>A comma constant string for forming a SQL statement.</summary>
        protected const string __comma = ", ";

        private const int MAX_SAFE_PARAM_NAME_SIZE = 100;

        /// <summary>A single quote string for forming a SQL statement.</summary>
        protected const string __singleQuote = "'";

        #endregion private constant

        #region Data

        /// <summary>IDbConnection for the object</summary>
        private IDbConnection? _connection = null;

        protected readonly ConnectionSetting CoreConnection;

        private CommandBehavior dataReaderBehavior = CommandBehavior.Default;

        public event SqlRowsCopiedEventHandler BulkCopySqlRowsCopied;

        /// <summary>Used to get a unique parameter number</summary>
        private int _nextParamNumber = 0;

        private object _syncLock = new();

        #endregion Data

        #region Static Properties

        public static CoreSettings? CurrentConfiguration { get; set; }

        /// <summary>Gets or sets the current database connection.</summary>
        /// <value>The current database connection.</value>
        public static DatabaseConnection? CurrentDatabaseConnection { get; set; }

        #endregion Static Properties

        #region Properties

        /// <summary>Gets the connection string.</summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; }

        /// <summary>Gets or sets the name of the database schema to use in query generation.  If a data object has the schema name .</summary>
        /// <value>The name of the database schema.</value>
        public string DefaultSchemaName { get; set; }

        /// <summary>
        /// Returns the active connection. If the stack has a connection then it is returned else the globalConnection is returned. If there are no connections, global or local then a new global
        /// connection is created.
        /// </summary>
        public IDbConnection Connection {
            get {
                if (_connection == null) {
                    _connection = OpenConnection(); //open connection if current pair has none
                }
                if (_connection.State == ConnectionState.Closed) {
                    _connection.Open();
                }
                return _connection;
            }
        }

        /// <summary>Get and set a IDataReader CommandBehavior</summary>
        public CommandBehavior DataReaderBehavior {
            get { return dataReaderBehavior; }
            set { dataReaderBehavior = value; }
        }

        /// <summary>Gets or sets the command timeout interval.</summary>
        /// <value>The command timeout.</value>
        public Int32 CommandTimeout { get; set; }

        /// <summary>Gets the supported database.</summary>
        /// <value>The supported database.</value>
        public DBSupported SupportedDatabase { get; set; }

        /// <summary>Gets or sets the transaction.</summary>
        /// <value>The transaction.</value>
        public IDbTransaction? Transaction { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="DatabaseObject" /> class.</summary>
        public DatabaseObject(DBSupported dbType) {
            // Default to Sql Server.
            SupportedDatabase = dbType;
            ConnectionString = string.Empty;
            CoreConnection = new();
            BulkCopySqlRowsCopied += DatabaseObject_BulkCopySqlRowsCopied;
            DefaultSchemaName = string.Empty;
        }

        private void DatabaseObject_BulkCopySqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) {
            throw new NotImplementedException();
        }

        /// <summary>Initializes a new instance of the <see cref="DatabaseObject" /> class.</summary>
        /// <param name="connectionString">The connection string.</param>
        public DatabaseObject(DBSupported dbType, string connectionString) : this(dbType) {
            this.ConnectionString = connectionString;
        }

        /// <summary>Initializes a new instance of the <see cref="DatabaseObject" /> class with a passed <see cref="ConnectionSetting" />.</summary>
        /// <param name="dbType">           </param>
        /// <param name="connectionSetting"></param>
        public DatabaseObject(DBSupported dbType, ConnectionSetting connectionSetting) : this(dbType, connectionSetting.ConnectionString) {
            this.CoreConnection = connectionSetting;
        }

        #endregion Constructors

        #region Static Factories

        /// <summary>Gets a default DatabaseObject with the passed <see cref="ConnectionSetting" /> and <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" />..</summary>
        /// <param name="databaseToUse"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(ConnectionSetting databaseToUse, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) {
            PopulateCurrentConnection(databaseToUse);
            return DatabaseObjectFactory(databaseToUse.DefaultSchema, databaseToUse, cache);
        }

        /// <summary>Gets a default DatabaseObject with the passed <see cref="DatabaseConnection" /> and <see cref="runtime.MemoryCache" />..</summary>
        /// <param name="databaseToUse"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="runtime.MemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(DatabaseConnection databaseToUse, runtime.MemoryCache cache) {
            CurrentDatabaseConnection = databaseToUse;
            return DatabaseObjectFactory(CurrentDatabaseConnection.DatabaseType, CurrentDatabaseConnection.ConnectionString, CurrentDatabaseConnection.SchemaName, cache);
        }

        /// <summary>Gets a default DatabaseObject with the passed <see cref="DatabaseConnection" /> and <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" />.</summary>
        /// <param name="databaseToUse"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(DatabaseConnection databaseToUse, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) {
            CurrentDatabaseConnection = databaseToUse;
            return DatabaseObjectFactory(CurrentDatabaseConnection.DatabaseType, CurrentDatabaseConnection.ConnectionString, CurrentDatabaseConnection.SchemaName, cache);
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="defaultSchema"></param>
        /// <param name="databaseToUse"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="runtime.MemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(string defaultSchema, DatabaseConnection databaseToUse, runtime.MemoryCache cache) {
            CurrentDatabaseConnection = databaseToUse;
            CurrentDatabaseConnection.SchemaName = defaultSchema;
            return DatabaseObjectFactory(CurrentDatabaseConnection.DatabaseType, CurrentDatabaseConnection.ConnectionString, CurrentDatabaseConnection.SchemaName, cache);
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="defaultSchema"></param>
        /// <param name="databaseToUse"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(string defaultSchema, DatabaseConnection databaseToUse, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) {
            CurrentDatabaseConnection = databaseToUse;
            CurrentDatabaseConnection.SchemaName = defaultSchema;
            return DatabaseObjectFactory(CurrentDatabaseConnection.DatabaseType, CurrentDatabaseConnection.ConnectionString, CurrentDatabaseConnection.SchemaName, cache);
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="defaultSchema"></param>
        /// <param name="databaseToUse"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(string defaultSchema, ConnectionSetting databaseToUse, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) {
            return DatabaseObjectFactory(CurrentDatabaseConnection.DatabaseType, databaseToUse, databaseToUse.DefaultSchema, cache);
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="dbType"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseSchema"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="runtime.MemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(DBSupported dbType, ConnectionSetting connectionString, string databaseSchema, runtime.MemoryCache cache) {
            return DatabaseObjectFactory(dbType, connectionString.ConnectionString, databaseSchema, cache);
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="dbType"></param>
        /// <param name="connectionSetting"></param>
        /// <param name="databaseSchema"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the HTTP Caching method, <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /></remarks>
        /// <returns></returns>
        public static DatabaseObject DatabaseObjectFactory(DBSupported dbType, ConnectionSetting connectionSetting, string databaseSchema, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) {
            return DatabaseObjectFactory(dbType, connectionSetting.ConnectionString, databaseSchema, cache);
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="dbType"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseSchema"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the runtime caching method, <see cref="runtime.MemoryCache" /></remarks>
        /// <returns></returns>
        /// <exception cref="CodedThoughtApplicationException"></exception>
        public static DatabaseObject DatabaseObjectFactory(DBSupported dbType, string connectionString, string databaseSchema, runtime.MemoryCache cache) {
            bool bWinformApp = CurrentConfiguration.IsWinForm;
            string dbObjectCacheName = String.Format("DATABASE_ASSEMBLY_{0}", dbType);
            DatabaseObject retVal;
            try {
                if (string.IsNullOrEmpty(connectionString)) {
                    throw new Exceptions.CodedThoughtApplicationException("Cannot obtain ConnectionString from setting file while running in non remoting mode.");
                }
                object[] arguments = new object[] { connectionString };
                // reflection to create the DatabaseObject based on the Database Type
                Assembly assembly = null;
                assembly = cache.GetFromLocalCache<Assembly>(dbObjectCacheName);
                if (assembly == null) {
                    string binPath = Common.GetBinPath();
                    // If the data provider assembly is found in the bin folder then use it. Otherwise load it from the runtime.
                    assembly = File.Exists(Path.Combine(binPath, $"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}.dll"))
                        ? Assembly.LoadFrom(Path.Combine(binPath, $"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}.dll"))
                        : Assembly.Load($"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}");
                    cache.AddToLocalCache(dbObjectCacheName, assembly);
                }
                string dbTypeName = $"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}.{dbType}DatabaseObject";
                Type dbObjType = assembly.GetType(dbTypeName);

                if (dbObjType == null) {
                    throw new Exceptions.CodedThoughtApplicationException($"The Object type {dbTypeName} was not found in the HP Core {dbType} library file.");
                }

                // create a database object
                object obj = Activator.CreateInstance(dbObjType, arguments, null);
                DatabaseObject databaseObject = (DatabaseObject)obj;
                databaseObject.DefaultSchemaName = databaseSchema;
                retVal = databaseObject ?? throw new Exceptions.CodedThoughtApplicationException("Cannot obtain DB Object from Factory.");
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException($"Cannot obtain DB Connection [{connectionString}]", ex);
            }

            return retVal;
        }

        /// <summary>Gets the DatabaseObject from the CallContext.</summary>
        /// <param name="dbType"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseSchema"></param>
        /// <param name="cache"></param>
        /// <remarks>Uses the HTTP Caching method, <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /></remarks>
        /// <returns></returns>
        /// <exception cref="CodedThoughtApplicationException"></exception>
        public static DatabaseObject DatabaseObjectFactory(DBSupported dbType, string connectionString, string databaseSchema, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) {
            string dbObjectCacheName = String.Format("DATABASE_ASSEMBLY_{0}", dbType);
            DatabaseObject retVal;
            try {
                if (string.IsNullOrEmpty(connectionString)) {
                    throw new Exceptions.CodedThoughtApplicationException("Cannot obtain ConnectionString from setting file while running in non remoting mode.");
                }

                object[] arguments = new object[] { connectionString };
                // reflection to create the DatabaseObject based on the Database Type
                Assembly assembly = null;
                assembly = cache.GetFromHttpCache<Assembly>(dbObjectCacheName);
                if (assembly == null) {
                    string binPath = Common.GetBinPath();
                    // If the data provider assembly is found in the bin folder then use it. Otherwise load it from the runtime.
                    assembly = File.Exists(Path.Combine(binPath, $"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}.dll"))
                        ? Assembly.LoadFrom(Path.Combine(binPath, $"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}.dll"))
                        : Assembly.Load($"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}");
                    cache.AddToHttpCache(dbObjectCacheName, assembly);
                }
                string dbTypeName = $"{Assembly.GetExecutingAssembly().GetName().Name}.Data.{dbType}.{dbType}DatabaseObject";
                Type dbObjType = assembly.GetType(dbTypeName);

                if (dbObjType == null) {
                    throw new Exceptions.CodedThoughtApplicationException($"The Object type {dbTypeName} was not found in the HP Core {dbType} library file.");
                }

                // create a database object
                object obj = Activator.CreateInstance(dbObjType, arguments, null);
                DatabaseObject databaseObject = (DatabaseObject)obj;
                databaseObject.DefaultSchemaName = databaseSchema;

                retVal = databaseObject ?? throw new Exceptions.CodedThoughtApplicationException("Cannot obtain DB Object from Factory.");
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException($"Cannot obtain DB Connection [{connectionString}]", ex);
            }

            return retVal;
        }
        private static List<Assembly> GetDataAwareAssemblies() {
            try {
                List<Assembly> dataAwareAssemblies = new();
                List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                foreach (Assembly assembly in allAssemblies) {
                    var isDataAware = Attribute.GetCustomAttribute(assembly, typeof(DataAwareAssemblyAttribute)) as DataAwareAssemblyAttribute;
                    if (isDataAware != null) { dataAwareAssemblies.Add(assembly); }
                }
                return dataAwareAssemblies;
            } catch { throw; }
        }
        #endregion Static Factories

        #region Methods

        /// <summary>Begins a transaction for the connection</summary>
        public IDbTransaction BeginTransaction() {
            if (Transaction == null) {
                this.Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            return Transaction;
        }

        /// <summary>Rolls back the transaction.</summary>
        public void RollbackTransaction() {
            if (this.Transaction != null) {
                this.Transaction.Rollback();
                this.Transaction = null;
            }
        }

        /// <summary>Commits the transaction.</summary>
        public void CommitTransaction() {
            if (this.Transaction != null) {
                this.Transaction.Commit();
                this.Transaction = null;
            }
        }

        /// <summary>Closes the connection</summary>
        public void CloseConnection() {
            try {
                if (_connection != null) {
                    _connection.Close();
                    _connection = null;
                }
            } catch (Exception) {
                //TODO We do not want any exception flying out of here.  Any exception caught here
                //must be logged and consumed
            }
        }

        #region Abstract Methods

        /// <summary>Creates the data adapter.</summary>
        /// <param name="cmd">The CMD.</param>
        /// <returns></returns>
        protected abstract IDataAdapter CreateDataAdapter(IDbCommand cmd);

        #region Parameter Abstracts

        /// <summary>Returns the parameter qualifier for the supported database. @ for SQLServer and : for Oracle</summary>
        /// <returns></returns>
        public virtual string ParameterConnector { get; }

        /// <summary>Gets the wild card character.</summary>
        /// <value>The wild card character.</value>
        public virtual string WildCardCharacter { get; }

        /// <summary>Gets the column delimiter.</summary>
        /// <value>The column delimiter.</value>
        public virtual string ColumnDelimiter { get; }

        /// <summary>Tests the connection.</summary>
        /// <returns></returns>
        public abstract bool TestConnection();

        public abstract ParameterCollection CreateParameterCollection();

        /// <summary>Creates parameters for the supported database.</summary>
        /// <param name="obj">  The Business Entity from which to extract the data</param>
        /// <param name="col">  The column for which the data must be extracted from the business entity</param>
        /// <param name="store">The store that handles the IO</param>
        /// <returns></returns>
        public abstract IDataParameter CreateParameter(object obj, TableColumn col, IDBStore store);

        /// <summary>Returns an empty parameter for oracle or sql server or any other supported database.</summary>
        /// <returns></returns>
        public abstract IDataParameter CreateEmptyParameter();

        /// <summary>Creates and returns an output parameter for the supported database.</summary>
        /// <param name="parameterName"></param>
        /// <param name="returnType">   </param>
        /// <returns></returns>
        public abstract IDataParameter CreateOutputParameter(string parameterName, DbTypeSupported returnType);

        /// <summary>Creates and returns a return parameter for the supported database.</summary>
        /// <param name="parameterName"></param>
        /// <param name="returnType">   </param>
        /// <returns></returns>
        public abstract IDataParameter CreateReturnParameter(string parameterName, DbTypeSupported returnType);

        /// <summary>Creates and returns a string parameter for the supported database.</summary>
        /// <param name="srcTableColumnName"></param>
        /// <param name="parameterValue">    </param>
        /// <returns></returns>
        public abstract IDataParameter CreateStringParameter(string srcTableColumnName, string parameterValue);

        /// <summary>Creates a Int32 parameter for the supported database</summary>
        /// <param name="srcTableColumnName"></param>
        /// <param name="parameterValue">    </param>
        /// <returns></returns>
        public abstract IDataParameter CreateInt32Parameter(string srcTableColumnName, int parameterValue);

        /// <summary>Creates a Double parameter based on supported database</summary>
        /// <param name="srcTableColumnName"></param>
        /// <param name="parameterValue">    </param>
        /// <returns></returns>
        public abstract IDataParameter CreateDoubleParameter(string srcTableColumnName, double parameterValue);

        /// <summary>Create a data time parameter based on supported database.</summary>
        /// <param name="srcTableColumnName"></param>
        /// <param name="parameterValue">    </param>
        /// <returns></returns>
        public abstract IDataParameter CreateDateTimeParameter(string srcTableColumnName, DateTime parameterValue);

        public abstract IDataParameter CreateBetweenParameter(string srcTableColumnName, BetweenParameter betweenParam);

        /// <summary>Creates a Char parameter based on supported database.</summary>
        /// <param name="srcTableColumnName"></param>
        /// <param name="parameterValue">    </param>
        /// <param name="size">              </param>
        /// <returns></returns>
        public abstract IDataParameter CreateCharParameter(string srcTableColumnName, string parameterValue, int size);

        /// <summary>Creates the XML parameter.</summary>
        /// <param name="srcTaleColumnName">Name of the SRC tale column.</param>
        /// <param name="parameterValue">   The parameter value.</param>
        /// <returns></returns>
        public abstract IDataParameter CreateXMLParameter(string srcTaleColumnName, string parameterValue);

        /// <summary>Creates a boolean parameter.</summary>
        /// <param name="srcTableColumnName">Name of the source table column.</param>
        /// <param name="parameterValue">    The parameter value.</param>
        /// <returns></returns>
        public abstract IDataParameter CreateBooleanParameter(string srcTableColumnName, bool parameterValue);

        /// <summary>Creates the GUID parameter.</summary>
        /// <param name="srcTableColumnName">Name of the SRC table column.</param>
        /// <param name="parameterValue">    The parameter value.</param>
        /// <returns></returns>
        public abstract IDataParameter CreateGuidParameter(string srcTableColumnName, Guid parameterValue);

        /// <summary>Creates a parameter to be used with a REST Api query.</summary>
        /// <param name="paraemterName"> </param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public abstract IDataParameter CreateApiParameter(string paraemterName, string parameterValue);

        /*
        /// <summary>Creates a Blob parameter based on supported database.</summary>
        /// <param name="srcTableColumnName"></param>
        /// <param name="parameterValue">    </param>
        /// <param name="size">              </param>
        /// <returns></returns>
        // public override IDataParameter CreateBlobParameter( string srcTableColumnName, string parameterValue);
        */

        #endregion Parameter Abstracts

        #region GetValue Abstracts

        /// <summary>
        /// Gets an int32 from the reader based on the database supported from the position passed.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>

        public virtual int GetInt32Value(IDataReader reader, string columnName) {
            int retVal = int.MinValue;

            //the reason why we have this business with checking if the type is decimal, is that SyBase only allows identity column that are
            //of type numeric. So a user would create an ID of numeric(10,0) for example. Of course, we can safely cast a numeric(x,0) to an integer without
            //loss of data. So normally, it is perfectly safe to get back a decimal (the .NET data type of numeric) and cast it to an integer. However, it may
            //be the case that a user accidentally called the GetInt32Value when they are actually storing doubles. In this case, we need to throw an exception
            //because the user could lose information without realizing it.

            int position = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(position)) {
                if (reader.GetFieldType(position) == typeof(Decimal)) {
                    Decimal dec = reader.GetDecimal(position);
                    retVal = Convert.ToInt32(dec);
                    if (retVal.ToString() != dec.ToString()) {
                        throw new InvalidCastException("Cast attempt failed: cannot convert decimal " + dec + "to int without loss of information");
                    }
                } else {
                    retVal = reader.GetInt32(position);
                }
            }

            return retVal;
        }

        /// <summary>Gets the bit value.</summary>
        /// <param name="reader">    The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public virtual Boolean GetBitValue(IDataReader reader, string columnName) {
            bool retVal = false;

            //the reason why we have this business with checking if the type is decimal, is that SyBase only allows identity column that are
            //of type numeric. So a user would create an ID of numeric(10,0) for example. Of course, we can safely cast a numeric(x,0) to an integer without
            //loss of data. So normally, it is perfectly safe to get back a decimal (the .NET data type of numeric) and cast it to an integer. However, it may
            //be the case that a user accidentally called the GetInt32Value when they are actually storing doubles. In this case, we need to throw an exception
            //because the user could lose information without realizing it.

            int position = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(position)) {
                retVal = reader.GetBoolean(position);
            }

            return retVal;
        }

        /// <summary>
        /// Gets a VarChar from the reader based on the database supported from the position passed.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>
        public virtual string GetVarCharValue(IDataReader reader, string columnName) {
            string retVal = string.Empty;
            int position = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(position)) {
                retVal = reader.GetString(position);
            }

            return retVal;
        }

        /// <summary>
        /// Gets a double from the reader based on the database supported from the position passed.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>
        public virtual double GetDoubleValue(IDataReader reader, string columnName) {
            double retVal = double.MinValue;
            int position = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(position)) {
                retVal = reader.GetDouble(position);
            }
            return retVal;
        }

        /// <summary>
        /// Gets a DateTime from the reader based on the database supported from the position passed.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>
        public virtual DateTime GetDateTimeValue(IDataReader reader, string columnName) {
            DateTime retVal = DateTime.MinValue;
            int position = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(position)) {
                retVal = reader.GetDateTime(position);
            }

            return retVal;
        }

        /// <summary>
        /// Gets a dbChar as string from the reader based on the database supported from the position passed.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>
        public virtual string GetCharValue(IDataReader reader, string columnName) {
            string retVal = string.Empty;
            int position = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(position)) {
                retVal = reader.GetString(position);
            }

            return retVal;
        }

        /// <summary>
        /// Get a BLOB from a TEXT or IMAGE column.
        /// In order to get BLOB, a IDataReader's CommandBehavior must be set to SequentialAccess.
        /// That also means to Get columns in sequence is extremely important.
        /// Otherwise the GetBlobValue method won't return correct data.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>
        protected abstract byte[] GetBlobValue(IDataReader reader, string columnName);

        /// <summary>
        /// Gets a string from a BLOB, Text (SQLServer) or CLOB (Oracle),. developers should use
        /// this method only if they know for certain that the data stored in the field is a string.
        /// </summary>
        /// <param name="reader"></param>
        ///<param name="columnName"></param>
        /// <returns></returns>
        public abstract string GetStringFromBlob(IDataReader reader, string columnName);

        #endregion GetValue Abstracts

        #region Database Specific Abstracts

        /// <summary>Gets an IsNull (SQLServer) or NVL (Oracle)</summary>
        /// <param name="validateColumnName"></param>
        /// <param name="optionColumnName">  </param>
        /// <returns></returns>
        public abstract string GetIfNullFunction(string validateColumnName, string optionColumnName);

        /// <summary>Gets a function name for NULL validation</summary>
        /// <returns></returns>
        public abstract string GetIfNullFunction();

        /// <summary>Gets a function name that return current date</summary>
        /// <returns></returns>
        public abstract string GetCurrentDateFunction();

        /// <summary>Gets a database specific date only SQL syntax. Mainly this method is used to removed time from SQL Server DateTime column.</summary>
        /// <param name="dateColumn"></param>
        /// <returns></returns>
        public abstract string GetDateOnlySqlSyntax(string dateColumn);

        /// <summary>Gets a database specific syntax that converts string to date. Oracle does not convert date string to date implicitly like SQL Server does when there is a date comparison.</summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public abstract string GetStringToDateSqlSyntax(string dateString);

        /// <summary>Gets a database specific syntax that converts string to date. Oracle does not convert date string to date implicitly like SQL Server does when there is a date comparison.</summary>
        /// <param name="dateSQL"></param>
        /// <returns></returns>
        public abstract string GetStringToDateSqlSyntax(DateTime dateSQL);

        /// <summary>Gets CASE (SQL Server) or DECODE (Oracle) SQL syntax.</summary>
        /// <param name="columnName"></param>
        /// <param name="equalValue"></param>
        /// <param name="trueValue"> </param>
        /// <param name="falseValue"></param>
        /// <param name="alias">     </param>
        /// <returns></returns>
        public abstract string GetCaseDecode(string columnName, string equalValue, string trueValue, string falseValue, string alias);

        /// <summary>Gets Date string format.</summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dateFormat">The date format.</param>
        /// <returns></returns>
        public abstract string GetDateToStringForColumn(string columnName, DateFormat dateFormat);

        /// <summary>Gets the date to string for value.</summary>
        /// <param name="value">     The value.</param>
        /// <param name="dateFormat">The date format.</param>
        /// <returns></returns>
        public abstract string GetDateToStringForValue(string value, DateFormat dateFormat);

        /// <summary>Gets database function name</summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns></returns>
        public abstract string GetFunctionName(FunctionName functionName);

        /// <summary>Gets SQL syntax of Year</summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public abstract string GetYearSQLSyntax(string dateString);

        /// <summary>Gets date part(Day, month or year) of date</summary>
        /// <param name="datestring">string</param>
        /// <param name="dateFormat">DateFormat</param>
        /// <param name="datePart">  DatePart</param>
        /// <returns></returns>
        public abstract string GetDatePart(string datestring, DateFormat dateFormat, DatePart datePart);

        /// <summary>Convert a date string to datetime when used for between.... and</summary>
        /// <param name="datestring">string</param>
        /// <param name="dateFormat">DateFormat</param>
        /// <returns></returns>
        public abstract string ToDate(string datestring, DateFormat dateFormat);

        /// <summary>Converts a database type name to a system type.</summary>
        /// <param name="dbTypeName">Name of the db type.</param>
        /// <returns>System.Type</returns>
        public abstract Type ToSystemType(String dbTypeName);

        /// <summary>Convert any data type to Char</summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public abstract string ConvertToChar(string columnName);

        /// <summary>Gets the table definition query for the supported database.</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected abstract String GetTableDefinitionQuery(string tableName);

        public virtual List<TableColumn> GetTableDefinition(string tableName) {
            try {
                List<TableColumn> tableDefinition = new();

                DataTable dtColumns = this.ExecuteDataTable(GetTableDefinitionQuery(tableName));
                foreach (DataRow row in dtColumns.Rows) {
                    TableColumn column = new("", DbTypeSupported.dbVarChar, 0, true) {
                        name = row["COLUMN_NAME"].ToString(),
                        isNullable = Convert.ToBoolean(row["IS_NULLABLE"]),
                        systemType = ToSystemType(row["DATA_TYPE"].ToString()),
                        maxLength = Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]),
                        isIdentity = Convert.ToBoolean(row["IS_IDENTITY"]),
                        ordinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"])
                    };
                    tableDefinition.Add(column);
                }
                return tableDefinition;
            } catch { throw; }
        }

        #endregion Database Specific Abstracts

        #endregion Abstract Methods

        #endregion Methods

        #region CreateWhereOverloads

        //TODO: parameter name should return a unique name
        /// <summary>
        /// Create a database specific where clause with parameters. for example x = @x for SQLServer. Additionally, the parameter created is automatically added to the parameter collection passed in
        /// </summary>
        /// <param name="parameters">     </param>
        /// <param name="tableColumnName">Name of column in table this parameter refers to</param>
        /// <param name="parameterValue"> Value to assign parameter</param>
        /// <returns></returns>
        public string CreateWhere(ParameterCollection parameters, string tableColumnName, int parameterValue) {
            IDataParameter param = CreateInt32Parameter(ToSafeParamName(tableColumnName), parameterValue);
            parameters.Add(param);

            return tableColumnName + "=" + ParameterConnector + param.ParameterName;
        }

        //TODO: Parameter name should be a unique name
        //TODO: check to see whether this even works in SQL server, I don't think you can compare text types. VarChar would probably work OK
        /// <summary>
        /// Create a database specific where clause with parameters. for example x = @x for SQLServer. Additionally, the parameter created is automatically added to the parameter collection passed in
        /// </summary>
        /// <param name="parameters">     </param>
        /// <param name="tableColumnName">Name of column in table this parameter refers to</param>
        /// <param name="parameterValue"> Value to assign parameter</param>
        /// <returns></returns>
        public string CreateWhere(ParameterCollection parameters, string tableColumnName, string parameterValue) {
            IDataParameter param = CreateStringParameter(ToSafeParamName(tableColumnName), parameterValue);

            parameters.Add(param);

            return tableColumnName + "=" + ParameterConnector + param.ParameterName;
        }

        /// <summary>
        /// Create a database specific where clause with parameters. for example x = @x for SQLServer. Additionally, the parameter created is automatically added to the parameter collection passed in
        /// </summary>
        /// <param name="parameters">     </param>
        /// <param name="tableColumnName">Name of column in table this parameter refers to</param>
        /// <param name="parameterValue"> Value to assign parameter</param>
        /// <returns></returns>
        public string CreateWhere(ParameterCollection parameters, string tableColumnName, DateTime parameterValue) {
            IDataParameter param = CreateDateTimeParameter(ToSafeParamName(tableColumnName), parameterValue);

            parameters.Add(param);

            return tableColumnName + "=" + ParameterConnector + param.ParameterName;
        }

        /// <summary>
        /// Create a database specific where clause with parameters. for example x = @x for SQLServer. Additionally, the parameter created is automatically added to the parameter collection passed in
        /// </summary>
        /// <param name="parameters">     </param>
        /// <param name="tableColumnName">Name of column in table this parameter refers to</param>
        /// <param name="parameterValue"> Value to assign parameter</param>
        /// <returns></returns>
        public string CreateWhere(ParameterCollection parameters, string tableColumnName, double parameterValue) {
            IDataParameter param = CreateDoubleParameter(ToSafeParamName(tableColumnName), parameterValue);

            parameters.Add(param);

            return tableColumnName + "=" + ParameterConnector + param.ParameterName;
        }

        #endregion CreateWhereOverloads

        #region CRUD Methods

        #region Add Abstract Methods

        /// <summary>Adds data to the database (abstract method)</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="obj">      The obj.</param>
        /// <param name="columns">  The columns.</param>
        /// <param name="store">    The store.</param>
        public abstract void Add(string tableName, object obj, List<TableColumn> columns, IDBStore store);

        #endregion Add Abstract Methods

        #region Get Methods

        /// <summary>Gets an IDataReader this creates a simple query where all parameters are joined by 'AND'</summary>
        public IDataReader Get(string tableName, List<string> selectColumns, ParameterCollection parameters) {
            return Get(tableName, DefaultSchemaName, selectColumns, parameters, null);
        }

        /// <summary>Gets an IDataReader this creates a simple query where all parameters are joined by 'AND'</summary>
        public IDataReader Get(string tableName, string schemaName, List<string> selectColumns, ParameterCollection parameters) {
            return Get(tableName, schemaName, selectColumns, parameters, null);
        }

        /// <summary>Gets a data reader based on table name, columns names etc.</summary>
        /// <param name="tableName">     </param>
        /// <param name="selectColumns"> </param>
        /// <param name="parameters">    </param>
        /// <param name="orderByColumns"></param>
        /// <returns></returns>
        public virtual IDataReader Get(string tableName, List<string> selectColumns, ParameterCollection parameters, List<string> orderByColumns) {
            return Get(tableName, string.Empty, selectColumns, parameters, orderByColumns);
        }

        /// <summary>Gets a data reader based on table name, columns names etc.</summary>
        /// <param name="tableName">     </param>
        /// <param name="selectColumns"> </param>
        /// <param name="parameters">    </param>
        /// <param name="orderByColumns"></param>
        /// <returns></returns>
        public virtual IDataReader Get(string tableName, string schemaName, List<string> selectColumns, ParameterCollection parameters, List<string> orderByColumns) {
            IDataReader reader = null;
            try {
                StringBuilder sql = new("SELECT ");
                sql.Append(GenerateColumnList(selectColumns));
                if (schemaName != string.Empty) {
                    sql.AppendFormat(" FROM {0}.{1}", schemaName, tableName);
                } else {
                    sql.AppendFormat(" FROM {0}", tableName);
                }
                sql.Append(" WITH (NOLOCK)");
                if (parameters != null && parameters.Count > 0) {
                    sql.Append(" WHERE " + GenerateWhereClauseFromParams(parameters));
                }

                sql.Append(GenerateOrderByClause(orderByColumns));
                reader = ExecuteReader(sql.ToString(), parameters);
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException("Failed to add retrieve data from: " + tableName, ex);
            } finally {
                CommitTransaction();
            }

            return reader;
        }

        /// <summary>Returns a DataReader based on a SQL embedded with parameters for a where clause and a parameter collection.</summary>
        /// <param name="sql">        The SQL.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters"> The parameters.</param>
        /// <returns></returns>
        public virtual IDataReader Get(string sql, CommandType commandType, ParameterCollection parameters) {
            IDataReader reader;
            try {
                reader = ExecuteReader(sql, commandType, parameters);
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException("Failed to add retrieve data from: " + sql, ex);
            }

            return reader;
        }

        /// <summary>Returns an ApiDataReader object based on the action and parameters passed.</summary>
        /// <param name="controller"></param>
        /// <param name="action">    </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<ApiDataReader> Get(string controller, string action, ParameterCollection parameters) {
            try {
                ApiDataReader reader = new(ConnectionString) {
                    Timeout = CommandTimeout
                };
                reader = await ExecuteReader(controller, action, parameters);
                return reader;
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException($"Failed to add retrieve data from: {GetApiSourceUrl()}", ex);
            }
        }
        /// <summary>Returns an ApiDataReader object based on the action and parameters passed.</summary>
        /// <param name="action">    </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <remarks>Use this method when the API only has an endpoint and not specified controller.</remarks>
        public async Task<ApiDataReader> Get(string action, ParameterCollection parameters) {
            try {
                ApiDataReader reader = new(ConnectionString) {
                    Timeout = CommandTimeout
                };
                reader = await ExecuteReader(string.Empty, action, parameters);
                return reader;
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException($"Failed to add retrieve data from: {GetApiSourceUrl()}", ex);
            }
        }
        /// <summary>Gets an DataSet this creates a simple query where all parameters are joined by 'AND'</summary>
        public DataSet GetDataSet(string tableName, List<string> selectColumns, ParameterCollection parameters) {
            return GetDataSet(tableName, String.Empty, selectColumns, parameters);
        }

        /// <summary>Gets an DataSet this creates a simple query where all parameters are joined by 'AND'</summary>
        public DataSet GetDataSet(string tableName, string schemaName, List<string> selectColumns, ParameterCollection parameters) {
            DataSet dataSet;
            DefaultSchemaName = schemaName;
            try {
                StringBuilder sql = new("SELECT ");
                sql.Append(GenerateColumnList(selectColumns));
                if (DefaultSchemaName != string.Empty) {
                    sql.AppendFormat(" FROM {0}.{1}", DefaultSchemaName, tableName);
                } else {
                    sql.AppendFormat(" FROM {0}", tableName);
                }
                sql.Append(" (NOLOCK)");
                if (parameters != null && parameters.Count > 0) {
                    sql.Append(" WHERE " + GenerateWhereClauseFromParams(parameters));
                }

                dataSet = ExecuteDataSet(sql.ToString(), parameters);
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException("Failed to retrieve data from: " + tableName, ex);
            }

            return dataSet;
        }

        #endregion Get Methods

        #region Remove Methods

        /// <summary>
        /// Removes data based on data in the parameter collection
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="parameters"></param>
        public virtual void Remove(string tableName, ParameterCollection parameters) {
            try {
                Remove(tableName, string.Empty, parameters);
            } catch { throw; }
        }
        /// <summary>
        /// Removes data based on data in the parameter collection.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schemaName"></param>
        /// <param name="parameters"></param>
        /// <exception cref="CodedThoughtApplicationException"></exception>
        public virtual void Remove(string tableName, string schemaName, ParameterCollection parameters) {
            try {
                BeginTransaction();
                string sql;
                if (schemaName != string.Empty && schemaName != null) {
                    sql = $"DELETE FROM {schemaName}.{tableName}";
                } else {
                    sql = $"DELETE FROM {tableName}";
                }
                //ParameterCollection deleteParameters=new ParameterCollection();

                if (parameters != null && parameters.Count > 0) {
                    sql += $" WHERE {GenerateWhereClauseFromParams(parameters)}";
                }

                ExecuteNonQuery(sql.ToString(), parameters);
            } catch (Exception ex) {
                RollbackTransaction();
                throw new Exceptions.CodedThoughtApplicationException("Failed to delete record from: " + tableName, ex);
            } finally {
                CommitTransaction();
            }
        }
        #endregion Remove Methods

        #region Update Methods

        /// <summary>Updates data in the database the update list is built from the parameters, and the where clause is built on the whereParamCollection</summary>
        /// <param name="tableName">           </param>
        /// <param name="parameters">          </param>
        /// <param name="whereParamCollection"></param>
        public virtual void Update(string tableName, ParameterCollection parameters, ParameterCollection whereParamCollection) {
            try {
                Update(tableName, string.Empty, parameters, whereParamCollection);
            } catch { throw; }
        }
        /// <summary>
        /// Updates data in the database the update list is built from the parameters, and the where clause is built on the whereParamCollection
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schemaName"></param>
        /// <param name="parameters"></param>
        /// <param name="whereParamCollection"></param>
        /// <exception cref="CodedThoughtApplicationException"></exception>
        public virtual void Update(string tableName, string schemaName, ParameterCollection parameters, ParameterCollection whereParamCollection) {
            try {
                BeginTransaction();
                StringBuilder sql = new();
                if (schemaName != string.Empty && schemaName != null) {
                    sql.Append($"UPDATE {schemaName}.{tableName} SET ");
                } else {
                    sql.Append($"UPDATE {tableName} SET ");
                }

                sql.Append(GenerateUpdateList(parameters));

                if (whereParamCollection != null && whereParamCollection.Count > 0) {
                    sql.Append(" WHERE " + GenerateWhereClauseFromParams(whereParamCollection));

                    //Add where clause to the parameter collection
                    parameters.Add(whereParamCollection);
                    //empty where collection
                    whereParamCollection.Clear();
                }

                ExecuteNonQuery(sql.ToString(), parameters);
            } catch (Exception ex) {
                RollbackTransaction();
                throw new Exceptions.CodedThoughtApplicationException("Failed to update record to: " + tableName, ex);
            } finally {
                CommitTransaction();
            }
        }
        #endregion Update Methods

        #endregion CRUD Methods

        #region Connection Methods

        /// <summary>Opens a connection to the database in question.</summary>
        /// <returns></returns>
        protected abstract IDbConnection OpenConnection();

        /// <summary>Commits updates and inserts. This is only for Oracle database operations.</summary>
        public abstract void Commit();

        #endregion Connection Methods

        #region Execute Query Methods

        /// <summary>Executes a command in the database which returns no data</summary>
        /// <param name="commandText"></param>
        public virtual int ExecuteNonQuery(string commandText) {
            return this.ExecuteNonQuery(commandText, CommandType.Text, null);
        }

        /// <summary>Executes a command in the database which returns no data</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        public virtual int ExecuteNonQuery(string commandText, CommandType type) {
            return this.ExecuteNonQuery(commandText, type, null);
        }

        /// <summary>Executes a command in the database which returns no data</summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"> </param>
        public virtual int ExecuteNonQuery(string commandText, ParameterCollection parameters) {
            return this.ExecuteNonQuery(commandText, CommandType.Text, parameters);
        }

        /// <summary>Executes a command in the database which returns no data</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        /// <param name="parameters"> </param>

        public virtual int ExecuteNonQuery(string commandText, CommandType type, ParameterCollection parameters) {
            try {
                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = type;
                cmd.CommandTimeout = this.CommandTimeout > -1 ? this.CommandTimeout : this.Connection.ConnectionTimeout;
                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                AddParametersToCommand(parameters, cmd);

                //Execute the query
                int affected = cmd.ExecuteNonQuery();

                ExtractAndReloadParameterCollection(parameters, cmd);
                return affected;
            } catch (Exception ex) {
                RollbackTransaction();
                throw new Exceptions.CodedThoughtApplicationException(ex.Message + "[" + commandText + "]", ex);
            } finally {
                CommitTransaction();
            }
        }

        /// <summary>Executes a command in the database which returns data as a IDataReader implementation</summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual IDataReader ExecuteReader(string commandText) {
            return this.ExecuteReader(commandText, CommandType.Text, null);
        }

        /// <summary>Executes a command in the database which returns data as a IDataReader implementation</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        /// <returns></returns>
        public virtual IDataReader ExecuteReader(string commandText, CommandType type) {
            return this.ExecuteReader(commandText, type, null);
        }

        /// <summary>Executes a command in the database which returns data as a IDataReader implementation</summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"> </param>
        /// <returns></returns>
        public virtual IDataReader ExecuteReader(string commandText, ParameterCollection parameters) {
            return this.ExecuteReader(commandText, CommandType.Text, parameters);
        }

        /// <summary>Executes a command whose CommandBehavior got set to commandBehavior in the database which returns an IDataReader object</summary>
        /// <param name="commandText">    </param>
        /// <param name="type">           </param>
        /// <param name="parameters">     </param>
        /// <param name="commandBehavior"></param>
        /// <returns></returns>
        public virtual IDataReader ExecuteReader(string commandText, CommandType type, ParameterCollection parameters, CommandBehavior commandBehavior) {
            // set command behavior
            DataReaderBehavior = commandBehavior;
            return ExecuteReader(commandText, type, parameters);
        }

        /// <summary>Executes a command in the database which returns data as a IDataReader implementation</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        /// <param name="parameters"> </param>
        /// <returns></returns>
        public virtual IDataReader ExecuteReader(string commandText, CommandType type, ParameterCollection parameters) {
            try {
                DataReaderBehavior = CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;

                // Add the transaction to set the isolation level.
                IDbCommand cmd = this.Connection.CreateCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = type;
                cmd.CommandTimeout = this.CommandTimeout > -1 ? this.CommandTimeout : this.Connection.ConnectionTimeout;
                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                AddParametersToCommand(parameters, cmd);

                IDataReader returnValue;
                //Execute the command
                try {
                    returnValue = cmd.ExecuteReader(DataReaderBehavior);
                } catch (InvalidOperationException) {
                    cmd.Connection.Open();
                    returnValue = cmd.ExecuteReader(dataReaderBehavior);
                }
                ExtractAndReloadParameterCollection(parameters, cmd);

                return returnValue;
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException(ex.Message + "[" + commandText + "]", ex);
            }
        }

        /// <summary>Executes the Api call which returns data within the IApiDataReader implementation.</summary>
        /// <param name="action">    </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<ApiDataReader> ExecuteReader(string controller, string action, ParameterCollection parameters) {
            try {
                ApiDataReader reader = new(this.ConnectionString);
                reader.Timeout = CommandTimeout;
                reader = await reader.ExecuteReader(controller, action, parameters);
                return reader;
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>Executes a command in the database which returns data as a DataTable</summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual DataTable ExecuteDataTable(string commandText) {
            DataSet ds = this.ExecuteDataSet(commandText, CommandType.Text, null);
            DataTable dt = ds.Tables[0];
            // detach the table from the data set object
            ds.Tables.Remove(dt);

            return dt;
        }

        /// <summary>Executes a command in the database which returns data as a DataSet</summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string commandText) {
            return this.ExecuteDataSet(commandText, CommandType.Text, null);
        }

        /// <summary>Executes a command in the database which returns data as a DataSet</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string commandText, CommandType type) {
            return this.ExecuteDataSet(commandText, type, null);
        }

        /// <summary>Executes a command in the database which returns data as a DataSet</summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"> </param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string commandText, ParameterCollection parameters) {
            return this.ExecuteDataSet(commandText, CommandType.Text, parameters);
        }

        /// <summary>Executes a command in the database which returns data as a DataSet</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        /// <param name="parameters"> </param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string commandText, CommandType type, ParameterCollection parameters) {
            try {
                if (parameters != null) {
                    if (type == CommandType.Text && parameters.Count > 0 && !commandText.ToUpper().Contains("WHERE")) {
                        commandText += $" WHERE {GenerateWhereClauseFromParams(parameters)}";
                    }
                }

                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = type;
                cmd.CommandTimeout = this.CommandTimeout > -1 ? this.CommandTimeout : this.Connection.ConnectionTimeout;
                AddParametersToCommand(parameters, cmd);

                IDataAdapter adapter = CreateDataAdapter(cmd);

                DataSet dataSet = new();
                adapter.Fill(dataSet);

                ExtractAndReloadParameterCollection(parameters, cmd);

                return dataSet;
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException(ex.Message + "[" + commandText + "]", ex);
            }
        }

        /// <summary>Executes a command in the database which returns a single scalar object</summary>
        /// <param name="commandText"></param>
        /// <param name="type">       </param>
        /// <param name="parameters"> </param>
        /// <returns></returns>
        public virtual object ExecuteScalar(string commandText, CommandType type, ParameterCollection parameters) {
            try {
                object? returnValue = null;

                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = type;
                cmd.CommandTimeout = this.CommandTimeout > -1 ? this.CommandTimeout : this.Connection.ConnectionTimeout;
                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                AddParametersToCommand(parameters, cmd);

                //Execute the command
                returnValue = cmd.ExecuteScalar();

                ExtractAndReloadParameterCollection(parameters, cmd);

                return returnValue is null ? null : returnValue;
            } catch (Exception ex) {
                throw new Exceptions.CodedThoughtApplicationException(ex.Message + "[" + commandText + "]", ex);
            }
        }

        /// <summary>Executes a bulk copy procedure.</summary>
        /// <param name="records">         The records.</param>
        /// <param name="destinationTable">The destination table.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">The table name wasn't found. If the destinationTable is not passed it must be passed as the DataTable TableName property.</exception>
        /// <exception cref="NotSupportedException">The current database type, " + SupportedDatabase.ToString() + " is not supported by this bulk insert method.</exception>
        public virtual bool ExecuteBulkCopy(DataTable records, int notificationRecordInterval, string? destinationTable = null) {
            // Get the destination table name from the DataTable if not passed explicitly.
            String tableName = (destinationTable ?? records.TableName);
            if (String.IsNullOrEmpty(tableName)) {
                throw new ArgumentNullException("The table name wasn't found.  If the destinationTable is not passed it must be passed as the DataTable TableName property.");
            }

            if (SupportedDatabase != DBSupported.SqlServer) {
                throw new NotSupportedException("The current database type, " + SupportedDatabase.ToString() + " is not supported by this bulk insert method.");
            } else {
                // Bulk Insert the DataTable to the database.
                using (Connection) {
                    // make sure to enable triggers more on triggers in next post
                    SqlBulkCopy bulkCopy =
                        new((SqlConnection)Connection) {
                            BulkCopyTimeout = this.CommandTimeout
                        };

                    // Set up the bulk copy mappings
                    int x = 0;
                    foreach (DataColumn col in records.Columns) {
                        //bulkCopy.ColumnMappings.Add( x, col.ordinalPosition ); // Part Number
                        bulkCopy.ColumnMappings.Add(x, x);
                        x++;
                    }
                    // Set the destination table name
                    bulkCopy.DestinationTableName = tableName;
                    if (Connection.State != ConnectionState.Open)
                        Connection.Open();

                    // write the data in the dataTable write the data in the dataTable
                    bulkCopy.NotifyAfter = records.Rows.Count < notificationRecordInterval ? records.Rows.Count / 2 : notificationRecordInterval;
                    bulkCopy.BatchSize = 10000;
                    bulkCopy.SqlRowsCopied += ExecuteBulkCopy_SqlRowsCopied;
                    try {
                        bulkCopy.WriteToServer(records);
                    } catch { throw; } finally {
                        Connection.Close();
                    }
                }
            }
            return true;
        }

        /// <summary>Handles the SqlRowsCopied event of the ExecuteBulkCopy method.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">     The <see cref="SqlRowsCopiedEventArgs" /> instance containing the event data.</param>
        public virtual void ExecuteBulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) {
            BulkCopySqlRowsCopied?.Invoke(this, e);
        }

        #endregion Execute Query Methods

        #region Helpers

        /// <summary>Function to give a "safe" name for a parameter.</summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        protected string ToSafeParamName(string paramName) {
            string safeParamName = paramName.Replace(".", "DOT");  //+GetUnusedParamNumber();

            if (safeParamName.Length > MAX_SAFE_PARAM_NAME_SIZE) {
                safeParamName = safeParamName.Substring(safeParamName.Length - MAX_SAFE_PARAM_NAME_SIZE);
            }

            return safeParamName;
        }

        /// <summary>Returns the next unused parameter name. This is just a number that increments on every call to this function. This method is thread safe.</summary>
        /// <returns></returns>
        private string GetUnusedParamNumber() {
            int nextUnusedNum = 0;
            lock (_syncLock) {
                if (_nextParamNumber == Int32.MaxValue) {
                    _nextParamNumber = 0;
                } else {
                    _nextParamNumber++;
                }

                nextUnusedNum = _nextParamNumber;
            }
            return nextUnusedNum.ToString();
        }

        /// <summary>Generates a list of columns for SQL</summary>
        /// <returns></returns>
        public string GenerateColumnList(List<string> columnList) {
            return GenerateColumnList(columnList, "");
        }

        /// <summary>Generates a list of columns for SQL prefixed with a table alias.</summary>
        /// <returns></returns>
        public string GenerateColumnList(List<string> columnList, string tableAlias) {
            StringBuilder columnSql = new();
            for (int i = 0; i < columnList.Count; i++) {
                columnSql.Append(__comma);
                if (tableAlias != string.Empty) {
                    columnSql.Append(tableAlias + ".");
                }
                columnSql.Append(columnList[i]);
            }
            string columnListSql = columnSql.Remove(0, 2).ToString();
            return columnListSql;
        }

        //TODO: remove all the loops that do things like columnSql.Remove(0, 2).ToString();. There is just too much of a chance that this could break.

        //TODO use parameter's source column not the name of the parameter
        /// <summary>Generates a list of columns for the update part of a sql statement</summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual string GenerateUpdateList(ParameterCollection parameters) {
            string parameterCount = "P";

            StringBuilder columnSql = new();
            for (int i = 0; i < parameters.Count; i++) {
                columnSql.Append(__comma + ((IDataParameter)parameters[i]).SourceColumn + " = " + this.ParameterConnector + parameterCount + i.ToString());
                ((IDataParameter)parameters[i]).ParameterName = parameterCount + i.ToString();
            }
            string updateListSql = columnSql.Remove(0, 2).ToString();
            return updateListSql;
        }

        //TODO: don't use param name to refer to column name
        private string GenerateWhereClauseFromParams(ParameterCollection parameters) {
            return parameters != null ? parameters.GenerateWhereClauseFromParams(ParameterConnector) : string.Empty;
        }

        /// <summary>Generates an order by clause</summary>
        /// <param name="columnList"></param>
        /// <returns></returns>
        public string GenerateOrderByClause(List<string> columnList) {
            string columnListSql = "";
            StringBuilder columnSql = new();

            if (columnList != null && columnList.Count > 0) {
                columnSql.Append(" ORDER BY ");
                for (int i = 0; i < columnList.Count; i++) {
                    string col = ((string)columnList[i]);
                    columnSql.Append(col + __comma);
                }
                columnListSql = columnSql.Remove(columnSql.Length - 2, 2).ToString();
            }

            return columnListSql;
        }

        /// <summary>Debug mode only for parameters of TBL_EVENT_ENTRY</summary>
        /// <param name="tableName"> </param>
        /// <param name="parameters"></param>
        /// <param name="sbSQL">     </param>

        protected void DebugParameters(StringBuilder sbSQL, string tableName, ParameterCollection parameters) {
            string sTmp = tableName.ToUpper();
            StringBuilder sbRealSQL = new(sbSQL.ToString());
            string line = "==============================================================================";
            {
                StringBuilder sb = new();
                System.Diagnostics.Debug.WriteLine(line);
                System.Diagnostics.Debug.WriteLine("TABLE NAME: " + tableName);
                sbSQL.ToString();
                foreach (IDataParameter p in parameters) {
                    sb.Append(p.ParameterName).Append("\t").Append(p.SourceColumn).Append(" = [").Append(p.Value).Append("]");
                    // replace parameters with its data
                    const string __singleQuote = "'";
                    switch (p.DbType) {
                        case DbType.AnsiStringFixedLength:
                            sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, __singleQuote + p.Value.ToString() + __singleQuote);
                            sb.Append("[").Append(p.Value.ToString().Length).Append("]");
                            break;

                        case DbType.AnsiString:
                            sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, __singleQuote + p.Value.ToString() + __singleQuote);
                            sb.Append("[").Append(p.Value.ToString().Length).Append("]");
                            break;

                        case DbType.StringFixedLength:
                            sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, __singleQuote + p.Value.ToString() + __singleQuote);
                            sb.Append("[").Append(p.Value.ToString().Length).Append("]");
                            break;

                        case DbType.String:
                            sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, __singleQuote + p.Value.ToString() + __singleQuote);
                            sb.Append("[").Append(p.Value.ToString().Length).Append("]");
                            break;

                        case DbType.DateTime:
                            sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, __singleQuote + p.Value.ToString() + __singleQuote);
                            break;
                        // case System.Data.DbType.ch.dbChar: sbRealSQL.Replace(this.ParameterConnector+p.ParameterName, __singleQuote+p.Value.ToString()+__singleQuote); break;
                        case DbType.Int32:
                            if (p.Value.ToString().Length != 0)
                                sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, p.Value.ToString());
                            else
                                sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, "NULL");
                            break;

                        case DbType.Double:
                            if (p.Value.ToString().Length != 0)
                                sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, p.Value.ToString());
                            else
                                sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, "NULL");
                            break;

                        case DbType.Object:
                            if (p.Value.ToString().Length != 0)
                                sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, __singleQuote + p.Value.ToString() + __singleQuote);
                            else
                                sbRealSQL.Replace(this.ParameterConnector + p.ParameterName, "NULL");
                            break;

                        default:
                            break;
                    }
                    System.Diagnostics.Debug.WriteLine(sb.ToString());
                    sb.Remove(0, sb.Length);
                }
                System.Diagnostics.Debug.WriteLine(sbRealSQL.ToString());
                System.Diagnostics.Debug.WriteLine(line);
            }
        }

        /// <summary>Removes the parameters from the parameter collection and adds them to the cmd.Parameter collection</summary>
        /// <param name="parameters"></param>
        /// <param name="cmd">       </param>
        private void AddParametersToCommand(ParameterCollection parameters, IDbCommand cmd) {
            if (parameters != null) {
                parameters.AddParametersToCommand(cmd, ParameterConnector);
            }
        }

        /// <summary>Now extract and reload the ParameterCollection So the caller of the method can use the parameters or extract any out params.</summary>
        /// <param name="parameters"></param>
        /// <param name="cmd">       </param>

        private void ExtractAndReloadParameterCollection(ParameterCollection parameters, IDbCommand cmd) {
            if (parameters != null) {
                parameters.ExtractAndReloadParameterCollection(cmd, ParameterConnector);
            }
        }

        /// <summary>Populates the CurrentDatabaseConnection object with the passed <see cref="ConnectionSetting" /></summary>
        /// <param name="connectionSetting"></param>
        private static void PopulateCurrentConnection(ConnectionSetting connectionSetting) {
            CurrentDatabaseConnection = new() {
                ConnectionName = connectionSetting.Name,
                ConnectionString = connectionSetting.ConnectionString,
                CommandTimeout = -1,
                DatabaseType = Common.GetDatabaseType(connectionSetting.ProviderType),
                SchemaName = connectionSetting.DefaultSchema
            };
        }

        protected string GetApiSourceUrl() {
            string url = string.Empty;
            string sourceUrl = string.Empty, controller = string.Empty;
            string[] urlParts = CoreConnection.ConnectionString.Split(";".ToCharArray());
            for (int i = 0; i <= urlParts.Length - 1; i++) {
                string[] connectionParameter = urlParts[i].Split("=".ToCharArray());
                switch (connectionParameter[0]) {
                    case "Api Url":
                        sourceUrl = connectionParameter[1]; break;
                    case "Controller":
                        controller = connectionParameter[1]; break;
                }
            }
            return $"{sourceUrl}/{controller}";
        }

        #endregion Helpers
    }
}