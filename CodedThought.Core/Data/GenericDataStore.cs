using CodedThought.Core.Data.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace CodedThought.Core.Data {

    /// <summary>Reference Delegate for using the Transaction Method <see cref="TransactionInvoker" /></summary>
    public delegate void TransactionInvoker();

    /// <summary>DBStore implementation that uses attributes and reflection to map objects into tables</summary>
    public class GenericDataStore : IDBStore {

        #region Declarations

        protected const string ORM_KEY = "ORM";
        protected const string ORM_ASSEMBLIES_KEY = "ORM_ASSEMBLIES";
        protected Dictionary<string, Dictionary<Type, Attribute>>? ORM; //object-relational-mapping
        protected List<string>? listLoadedAssemblies;
        protected DatabaseConnection? _specifiedDatabaseConnection;
        protected readonly IMemoryCache? _cache = null;
        protected readonly runtime.MemoryCache? _runtimeCache = null;
        protected string _defaultSchema;
        protected readonly bool _useHttpCache;
        private bool _enableRowLocking;

        public event SqlRowsCopiedEventHandler? BulkCopySqlRowsCopied;

        /// <summary>Occurs when [rows inserted] for Sql Server Bulk Insert methods.</summary>
        public event EventHandler<SqlRowsCopiedEventArgs>? RowsInserted;

        #endregion Declarations

        #region Properties

        public DatabaseConnection? CurrentDatabaseConnection { get; set; }
        public ConnectionSetting? ConnectionSetting { get; private set; }
        public bool UseHttpCache => _useHttpCache;
        /// <summary>Gets or sets the database object instance.</summary>
        /// <value>The database object instance.</value>
        protected DatabaseObject DatabaseObjectInstance { get; private set; }

        /// <summary>Gets the connection.</summary>
        /// <value>The connection.</value>
        /// <exception cref="NullReferenceException">The database object has not been instantiated yet.</exception>
        public virtual IDbConnection Connection {
            get {
                return DatabaseObjectInstance == null
                    ? throw new NullReferenceException("The database object has not been instantiated yet.")
                    : DatabaseObjectInstance.Connection;
            }
        }

        /// <summary>Gets or sets the name of the database schema.</summary>
        /// <value>The name of the database schema.</value>
        [Obsolete("This property is obsolete.  Please use DefaultSchemaName.")]
        public virtual string? DatabaseSchemaName {
            get => DefaultSchemaName;
            set => DefaultSchemaName = value;
        }
        /// <summary>
        /// Gets or sets the default schema name to be used.  This can be overridden by using the <see cref="DataTableAttribute.SchemaName"/> property of a data object.
        /// </summary>
        public virtual string? DefaultSchemaName {
            get {
                if (_defaultSchema == string.Empty) {
                    _defaultSchema = DatabaseObjectInstance.GetSchemaName();
                }
                return _defaultSchema;

            }
            set => _defaultSchema = value;
        }

        /// <summary>Gets or sets the database connection to use.</summary>
        /// <value>The database to use.</value>
        public virtual DatabaseConnection DatabaseToUse {
            get => _specifiedDatabaseConnection;
            set {
                _specifiedDatabaseConnection = value;
                DefaultSchemaName = value.SchemaName;
                CommandTimeout = value.CommandTimeout;
                CurrentDatabaseConnection = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether a transaction is in progress.</summary>
        /// <value><c>true</c> if [transaction in progress]; otherwise, <c>false</c>.</value>
        public virtual bool TransactionInProgress { get; set; }

        /// <summary>Gets or sets the timeout override.</summary>
        /// <value>The timeout override. If the value is not set then the default timeout of the connection set in the connection string will be used.</value>
        public virtual int CommandTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated CRUD statements should request row-level locks when supported by the database provider.
        /// </summary>
        public virtual bool EnableRowLocking {
            get => DatabaseObjectInstance == null ? _enableRowLocking : DatabaseObjectInstance.EnableRowLocking;
            set {
                _enableRowLocking = value;
                if (DatabaseObjectInstance != null) {
                    DatabaseObjectInstance.EnableRowLocking = value;
                }
            }
        }

        /// <summary>Gets the wildcard character.</summary>
        /// <value>The wildcard character.</value>
        public virtual string WildcardCharacter => DatabaseObjectInstance.WildCardCharacter;

        #endregion Properties

        #region Constructors

        private GenericDataStore() {
            _specifiedDatabaseConnection = null;
            _cache = null;
        }
        private GenericDataStore(runtime.MemoryCache cache) : this() {
            _runtimeCache = cache;
            _useHttpCache = false;
            if (!_runtimeCache.TryGetValue(ORM_KEY, out ORM)) {
                ORM = [];
            }
        }
        private GenericDataStore(IMemoryCache cache) : this() {
            _cache = cache;
            _useHttpCache = true;
            if (!_cache.TryGetValue(ORM_KEY, out ORM)) {
                ORM = [];
            }
        }

        /// <summary>
        /// Initializes a new instance of the GenericDataStore class. The internal ORM model (shared between all instances of the GenericDataStore class) is initialized to the calling assembly.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="databaseToUse"></param>
        public GenericDataStore(IServiceProvider serviceProvider, runtime.MemoryCache cache, ConnectionSetting databaseToUse) : this(cache) {
            // The calling assembly should be the assembly with the data aware classes to load into the ORM.
            if (ORM.Count == 0) {
                LoadAssemblyAndORM(Assembly.GetCallingAssembly());
            }
            TransactionInProgress = false;
            DatabaseObjectInstance = DatabaseObject.DatabaseObjectFactory(serviceProvider, cache, databaseToUse);
            DatabaseObjectInstance.CommandTimeout = 0;
            DatabaseObjectInstance.EnableRowLocking = _enableRowLocking;
            DatabaseToUse = new(databaseToUse);
        }
        /// <summary>
        /// Initializes a new instance of the GenericDataStore class. The internal ORM model (shared between all instances of the GenericDataStore class) is initialized to the calling assembly.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="cache">Http Cache</param>
        /// <param name="databaseToUse"></param>
        public GenericDataStore(IServiceProvider serviceProvider, IMemoryCache cache, ConnectionSetting databaseToUse) : this(cache) {
            // The calling assembly should be the assembly with the data aware classes to load into the ORM.
            if (ORM.Count == 0) {
                LoadAssemblyAndORM(Assembly.GetCallingAssembly());
            }

            TransactionInProgress = false;
            DatabaseObjectInstance = DatabaseObject.DatabaseObjectFactory(serviceProvider, cache, databaseToUse);
            DatabaseObjectInstance.CommandTimeout = 0;
            DatabaseObjectInstance.EnableRowLocking = _enableRowLocking;
            DatabaseToUse = new(databaseToUse);
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>Creates a parameter collection.</summary>
        /// <returns></returns>
        public ParameterCollection CreateParameterCollection() {
            ParameterCollection param = [];
            SetParameterCollectionDbObject(param);
            return param;
        }

        /// <summary>
        /// Parses an assembly for ORM mappings.
        /// </summary>
        /// <param name="assembly">The assembly to parse.</param>
        public virtual void LoadAssemblyAndORM(Assembly callingAssembly) {
            if (UseHttpCache) {
                listLoadedAssemblies = _cache.GetFromHttpCache<List<string>>(ORM_ASSEMBLIES_KEY);
                if (listLoadedAssemblies == null) {
                    // Load all data aware assemblies.
                    listLoadedAssemblies = [];
                    List<Assembly> dataAwareAssemblies = GetDataAwareAssemblies();
                    dataAwareAssemblies.ForEach(a => {
                        listLoadedAssemblies.Add(a.GetName().Name);
                        GenerateMapCollection(ORM, a);
                        // Cache the objects for next time.
                        _cache.AddToHttpCache(ORM_KEY, ORM);
                        _cache.AddToHttpCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
                    });
                }
                listLoadedAssemblies ??= [];
                if (!listLoadedAssemblies.Contains(callingAssembly.GetName().Name)) {
                    listLoadedAssemblies.Add(callingAssembly.GetName().Name);
                    GenerateMapCollection(ORM, callingAssembly);

                    //re-cache the objects after updates
                    _cache.AddToHttpCache(ORM_KEY, ORM);
                    _cache.AddToHttpCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
                }
            } else {
                listLoadedAssemblies = _runtimeCache.GetFromLocalCache<List<string>>(ORM_ASSEMBLIES_KEY);
                if (listLoadedAssemblies == null) {
                    // Load all data aware assemblies.
                    listLoadedAssemblies = [];
                    List<Assembly> dataAwareAssemblies = GetDataAwareAssemblies();
                    dataAwareAssemblies.ForEach(a => {
                        listLoadedAssemblies.Add(a.GetName().Name);
                        GenerateMapCollection(ORM, a);
                        // Cache the objects for next time.
                        _runtimeCache.AddToLocalCache(ORM_KEY, ORM);
                        _runtimeCache.AddToLocalCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
                    });
                }
                if (!listLoadedAssemblies.Contains(callingAssembly.GetName().Name)) {
                    listLoadedAssemblies.Add(callingAssembly.GetName().Name);
                    GenerateMapCollection(ORM, callingAssembly);

                    //re-cache the objects after updates
                    _runtimeCache.AddToLocalCache(ORM_KEY, ORM);
                    _runtimeCache.AddToLocalCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
                }
            }
        }

        /// <summary>Retrieves an object from the database of type T for the given primary key.</summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="objectID">The primary key of the object.</param>
        /// <returns>Returns an object of the type T.</returns>
        public T Get<T>(int objectID) where T : class, new() {
            ParameterCollection parameters = [];
            SetParameterCollectionDbObject(parameters);
            DataTableAttribute attrTable = GetTableAttribute<T>();
            parameters.AddInt32Parameter(attrTable.Key.ColumnName, objectID);
            return Get<T>(parameters);
        }
        public T Get<T>(object objectID) where T : class, new() {
            // Determine the data type from the passed object's primary key.
            DataColumnAttribute attrib = GetPrimaryKeyAttribute<T>();
            ParameterCollection parameters = [];
            SetParameterCollectionDbObject(parameters);
            switch (attrib.ColumnType) {
                case DbType.Guid:
                    parameters.AddGuidParameter(attrib.ColumnName, (Guid)objectID);
                    break;
                case DbType.String:
                    parameters.AddStringParameter(attrib.ColumnName, (string)objectID);
                    break;
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                    parameters.AddInt32Parameter(attrib.ColumnName, (int)objectID);
                    break;
            }
            return parameters.Count > 0
                ? Get<T>(parameters)
                : throw new NotSupportedException($"The DbType {attrib.ColumnType}, is not supported by this method. Only Guid, String, or Integer types are supported.");
        }
        /// <summary>Retrieves an object based on the supplied Key-Value pair collection.</summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="parameters">A collection of Key-Value pairs.</param>
        /// <returns>Returns an object of the type T.</returns>
        public T Get<T>(ParameterCollection parameters) where T : class, new() {
            IDataReader reader = null;
            T entity = null;
            List<T> list = null;
            try {
                List<string> selectColumns = GetColumnNames<T>();
                List<string> orderColumns = GetOrderByColumnNames<T>();
                DataTableAttribute attrTable = GetTableAttribute<T>();
                string schemaName = attrTable.SchemaName ?? DefaultSchemaName;
                string sourceName = attrTable.UseView ? attrTable.ViewName : attrTable.TableName;
                SetParameterCollectionDbObject(parameters);
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                reader = DatabaseObjectInstance.Get(sourceName, schemaName, selectColumns, parameters, orderColumns);

                list = CreateBusinessEntity<T>(reader);
                if (list.Count > 0) {
                    entity = list[0];
                }
            } catch {
                throw;
            } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }

            return entity;
        }

        /// <summary>Retrieves a list of objects matching the supplied Kay-Value par criteria.</summary>
        /// <typeparam name="T">The type of objects to retrieve.</typeparam>
        /// <param name="parameters">A collection of Key-Value pairs.</param>
        /// <returns>Returns a List&lt;&gt; of type T</returns>
        public List<T> GetMultiple<T>(ParameterCollection? parameters) where T : class, new() {
            if (parameters != null)
                SetParameterCollectionDbObject(parameters);
            IList<T> list = [];
            GetMultiple(ref list, parameters);
            return (List<T>)list;
        }

        /// <summary>Retrieves a list of objects matching the supplied Kay-Value par criteria.</summary>
        /// <typeparam name="T">The type of objects to retrieve.</typeparam>
        /// <param name="list">      A reference to the list in which the items are to be returned.</param>
        /// <param name="parameters">A collection of Key-Value pairs.</param>
        public void GetMultiple<T>(ref IList<T> list, ParameterCollection? parameters) where T : class, new() {
            IDataReader reader = null;
            try {
                DataTableAttribute attrTable = GetTableAttribute<T>();
                string schemaName = attrTable.SchemaName;
                string sourceName = attrTable.UseView ? attrTable.ViewName : attrTable.TableName;
                List<string> selectColumns = GetColumnNames<T>();
                List<string> orderColumns = GetOrderByColumnNames<T>();
                if (parameters != null) {
                    SetParameterCollectionDbObject(parameters);
                } else {
                    parameters = [];
                }
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                reader = DatabaseObjectInstance.Get(sourceName, schemaName, selectColumns, parameters, orderColumns);
                CreateBusinessEntity(ref list, reader, true);
            } catch {
                throw;
            } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }
        }

        /// <summary>Sorts the specified list.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">         The list.</param>
        /// <param name="sortBy">       The property to sort by.</param>
        /// <param name="sortDirection">The direction to sort the list.</param>
        public void Sort<T>(ref List<T> list, PropertyInfo sortBy, ListSortDirection sortDirection) where T : class, new() {
            try {
                string propertyName = sortBy.Name;

                list.Sort(delegate (T obj1, T obj2) {
                    return obj1.GetType().GetProperty(propertyName).MemberType.CompareTo(obj2.GetType().GetProperty(propertyName).MemberType);
                });
            } catch {
                throw;
            }
        }

        /// <summary>Sorts the specified list.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">         The list.</param>
        /// <param name="sortBy">       The property name to sort by.</param>
        /// <param name="sortDirection">The sort direction to sort.</param>
        public void Sort<T>(ref List<T> list, string sortBy, ListSortDirection sortDirection) where T : class, new() {
            try {
                this.Sort(ref list, typeof(T).GetProperty(sortBy), sortDirection);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// Saves an object to the database. If the object has a non-zero primary key, an update is performed otherwise an insert is performed.
        /// </summary>
        /// <typeparam name="T">The type of objects to save.</typeparam>
        /// <param name="obj">The object to save.</param>
        /// <remarks>If the primary key is clear it assumed to be a new entry.
        /// Different Kinds of Keys:
        ///     Integer: Set to 0
        ///     String: Set to Empty String
        ///     Guid: Set to Guid minimum
        /// </remarks>
        public void Save<T>(T obj) {
            bool bIsNew = true;
            //determine if this is an insert or an update
            DataTableAttribute attrTable = GetTableAttribute<T>();
            if (attrTable.Key != null) {
                object oPrimaryKey = typeof(T).GetProperty(attrTable.Key.PropertyName).GetValue(obj, null);
				bIsNew = oPrimaryKey is null
					? true
					: attrTable.Key.ColumnType == DbType.Guid
						? (Guid)oPrimaryKey == Guid.Empty
						: oPrimaryKey.IsNumericType() ? Convert.ToDecimal(oPrimaryKey) <= 0 : string.IsNullOrEmpty(oPrimaryKey.ToString());
			}
            if (bIsNew) {
                //insert
                SaveNew(obj);
            } else {
                //update
                SaveExisting(obj);
            }
        }

        /// <summary>Saves the specified obj as new.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        public void SaveNew<T>(T obj) {
            //insert
            DataTableAttribute attrTable = GetTableAttribute<T>();
            if (attrTable.ReadOnly)
                throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no insert, update, or delete operations can be performed against it.");

            // GUID identity keys are generated client-side before the insert is constructed.
            bool autoGenerateGuidKey = attrTable.Key?.ColumnType == DbType.Guid
                && (attrTable.AutoGenerateUniqueIdentifier || attrTable.Key.IsIdentity);
            if (autoGenerateGuidKey) {
                object? primaryKey = typeof(T).GetProperty(attrTable.Key.PropertyName)?.GetValue(obj, null);
                if (primaryKey is null || (Guid) primaryKey == Guid.Empty) {
                    SetPrimaryKeyValue<T>(obj, attrTable.Key.PropertyName, Guid.NewGuid());
                }
            }

            List<TableColumn> listColumns = [];
            foreach (DataColumnAttribute attrColumn in attrTable.Properties) {
                TableColumn tc = new(attrColumn.ColumnName, attrColumn.ConvertTypeToDbTypeSupported(), attrColumn.Size, attrColumn.IsPrimaryKey);
                tc.DbType = attrColumn.ColumnType;
                tc.IsInsertable = tc.IsUpdateable;
                tc.IsIdentity = attrColumn.IsIdentity;
                tc.IsNullableType = attrColumn.IsNullableType;
                if (tc.IsPrimary && tc.Type == DbTypeSupported.dbGUID && autoGenerateGuidKey) {
                    tc.IsInsertable = true;
                }
                listColumns.Add(tc);
            }
            DatabaseObjectInstance.Add(attrTable.TableName, attrTable.SchemaName, obj, listColumns, this);
        }

        /// <summary>Saves an existing object.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        public void SaveExisting<T>(T obj) {
            //update
            DataTableAttribute attrTable = GetTableAttribute<T>();
            if (attrTable.ReadOnly)
                throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

            ParameterCollection oParamColumns = [];
            ParameterCollection oParamWhere = [];
            SetParameterCollectionDbObject(oParamColumns);
            SetParameterCollectionDbObject(oParamWhere);

            foreach (DataColumnAttribute attrColumn in attrTable.Properties) {
                IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
                param.DbType = attrColumn.ColumnType;
                param.Direction = ParameterDirection.Input;
                param.ParameterName = attrColumn.ColumnName;
                param.SourceColumn = attrColumn.ColumnName;

                // Check if we need to get the value from a sub object or from the local variable.
                object value;
                if (attrColumn.ExtendedPropertyType == null) {
                    value = typeof(T).GetProperty(attrColumn.PropertyName).GetValue(obj, null);
                } else {
                    Type extendedType = attrColumn.ExtendedPropertyType;
                    if (extendedType.IsEnum) {
                        value = (int)typeof(T).GetProperty(attrColumn.PropertyName).GetValue(obj, null);
                    } else {
                        object oExtendedObject = typeof(T).GetProperty(attrColumn.PropertyName).GetValue(obj, null);
                        value = oExtendedObject.GetType().GetProperty(attrColumn.ExtendedPropertyName).GetValue(oExtendedObject, null);
                    }
                }
                param.Value = FormatValueForNull(value, attrColumn.PropertyType);

                //set primary key as where clause
                if (attrColumn == attrTable.Key) {
                    oParamWhere.Add(param);
                } else {
                    // This isn't the primary key so add it to the main parameter collection.
                    oParamColumns.Add(param);
                }
            }
            //no primary key: throw error
            if (oParamWhere.Count == 0) {
                throw new CodedThoughtApplicationException("Cannot perform an update when no where clause is specified.");
            }
            DatabaseObjectInstance.Update(attrTable.TableName, oParamColumns, oParamWhere);
        }

        /// <summary>Uses bulk insert and learns the generic object's table structure to complete the process.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">The records.</param>
        /// <returns></returns>
        public bool SaveBulk<T>(List<T> records, int notifyAfter = 2000) {
            try {
                DataTableAttribute attrTable = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
                if (attrTable.ReadOnly)
                    throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

                string tableName = attrTable.TableName;
                DataTable dt = new(tableName);
                List<TableColumn> tableColumns = DatabaseObjectInstance.GetTableDefinition(tableName);
                tableColumns.Sort(delegate (TableColumn c1, TableColumn c2) { return c1.OrdinalPosition.CompareTo(c2.OrdinalPosition); });

                tableColumns = tableColumns.Where(col => GetPropertyNameColumn<T>(col.Name) != string.Empty && col.IsIdentity != true).ToList();
                // Propertied columns refers to columns in the table that indeed have a property assigned to them in the object.
                List<TableColumn> propertiedColumns = tableColumns.Where(col => col.IsIdentity != true)
                    .Select(col => {
                        col.CorrespondingPropertyName = GetPropertyNameColumn<T>(col.Name);
                        return col;
                    }).ToList();

                dt = records.ToDataTable();

                if (DatabaseToUse.DatabaseType != DBSupported.SqlServer) {
                    throw new NotSupportedException("The current database type, " + DatabaseToUse.DatabaseType.ToString() + " is not supported by this bulk insert method.");
                } else {
                    // make sure to enable triggers more on triggers in next post
                    SqlBulkCopy bulkCopy =
                        new((SqlConnection)DatabaseObjectInstance.Connection) {
                            BulkCopyTimeout = DatabaseObjectInstance.CommandTimeout < 0 ? 0 : DatabaseObjectInstance.CommandTimeout
                        };

                    // Set up the bulk copy mappings
                    int x = 0;
                    foreach (TableColumn col in propertiedColumns) {
                        //bulkCopy.ColumnMappings.Add( x, col.ordinalPosition ); // Part Number
                        bulkCopy.ColumnMappings.Add(x, col.OrdinalPosition);
                        x++;
                    }
                    // Set the destination table name
                    bulkCopy.DestinationTableName = DatabaseObjectInstance.GetTableName(attrTable.SchemaName, attrTable.TableName);
                    if (Connection.State != ConnectionState.Open)
                        Connection.Open();

                    // write the data in the dataTable write the data in the dataTable
                    bulkCopy.NotifyAfter = dt.Rows.Count < notifyAfter ? dt.Rows.Count : notifyAfter;
                    bulkCopy.BatchSize = bulkCopy.NotifyAfter;
                    bulkCopy.SqlRowsCopied += BulkCopy_SqlRowsCopied;
                    try {
                        bulkCopy.WriteToServer(dt);
                    } catch (Exception ex) {
                        if (ex.Message.Contains("Received an invalid column length")) {
                            string errorMessage = string.Empty;
                            errorMessage = GetBulkCopyColumnException(ex, bulkCopy);
                            throw new Exception(errorMessage, ex);
                        } else {
                            throw;
                        }
                    }
                }
                return true;
            } catch (CodedThoughtException ex) {
                throw;
            }
        }

        public bool SaveBulk<T>(DataTable records, int notifyAfter = 2000) {
            try {
                DataTableAttribute attrTable = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
                if (attrTable.ReadOnly)
                    throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

                DataTable dt = records;
                List<TableColumn> tableColumns = DatabaseObjectInstance.GetTableDefinition(attrTable.TableName);
                tableColumns.Sort(delegate (TableColumn c1, TableColumn c2) { return c1.OrdinalPosition.CompareTo(c2.OrdinalPosition); });

                tableColumns = tableColumns.Where(col => GetPropertyNameColumn<T>(col.Name) != string.Empty && col.IsIdentity != true).ToList();
                // Propertied columns refers to columns in the table that indeed have a property assigned to them in the object.
                List<TableColumn> propertiedColumns = tableColumns.Where(col => col.IsIdentity != true)
                    .Select(col => {
                        col.CorrespondingPropertyName = GetPropertyNameColumn<T>(col.Name);
                        return col;
                    }).ToList();

                if (DatabaseToUse.DatabaseType != DBSupported.SqlServer) {
                    throw new NotSupportedException("The current database type, " + DatabaseToUse.DatabaseType.ToString() + " is not supported by this bulk insert method.");
                } else {
                    // make sure to enable triggers more on triggers in next post
                    SqlBulkCopy bulkCopy =
                        new((SqlConnection)DatabaseObjectInstance.Connection) {
                            BulkCopyTimeout = DatabaseObjectInstance.CommandTimeout
                        };

                    // Set up the bulk copy mappings
                    int x = 0;
                    foreach (TableColumn col in propertiedColumns) {
                        bulkCopy.ColumnMappings.Add(x, col.OrdinalPosition);
                        x++;
                    }
                    // Set the destination table name
                    bulkCopy.DestinationTableName = DatabaseObjectInstance.GetTableName(attrTable.SchemaName, attrTable.TableName);
                    if (Connection.State != ConnectionState.Open)
                        Connection.Open();

                    // write the data in the dataTable write the data in the dataTable
                    bulkCopy.NotifyAfter = dt.Rows.Count < notifyAfter ? dt.Rows.Count : notifyAfter;
                    bulkCopy.BatchSize = bulkCopy.NotifyAfter;
                    bulkCopy.SqlRowsCopied += BulkCopy_SqlRowsCopied;
                    try {
                        bulkCopy.WriteToServer(dt);
                    } catch (Exception ex) {
                        if (ex.Message.Contains("Received an invalid column length")) {
                            string errorMessage = string.Empty;
                            errorMessage = GetBulkCopyColumnException(ex, bulkCopy);
                            throw new Exception(errorMessage, ex);
                        } else {
                            throw;
                        }
                    }
                }
                return true;
            } catch (CodedThoughtException ex) {
                throw;
            }
        }

        /// <summary>Handles the SqlRowsCopied event of the bulkCopy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">     The <see cref="SqlRowsCopiedEventArgs" /> instance containing the event data.</param>
        protected void BulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) => RowsInserted?.Invoke(this, e);

        protected string GetBulkCopyColumnException(Exception ex, SqlBulkCopy bulkcopy) {
            string message = string.Empty;
            if (ex.Message.Contains("Received an invalid column length from the bcp client for colid")) {
                string pattern = @"\d+";
                Match match = Regex.Match(ex.Message.ToString(), pattern);
                int index = Convert.ToInt32(match.Value) - 1;

                FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
                object? sortedColumns = fi.GetValue(bulkcopy);
                object[]? items = (object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
                object? metadata = itemdata.GetValue(items[index]);
                object? column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                object? length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                message = string.Format("Column: {0} contains data with a length greater than: {1}", column, length);
            }
            return message;
        }

        /// <summary>Removes an object from the database with the supplied primary key.</summary>
        /// <typeparam name="T">The type of object to remove.</typeparam>
        /// <param name="objectID">The primary key of the object.</param>
        public void Remove<T>(int objectID) where T : class, new() {
            ParameterCollection parameters = [];
            DataTableAttribute attrTable = GetTableAttribute<T>();
            SetParameterCollectionDbObject(parameters);
            parameters.AddInt32Parameter(attrTable.Key.ColumnName, objectID);
            Remove<T>(parameters);
        }

        /// <summary>Removes an object from the database with the specified parameters.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters.</param>
        public void Remove<T>(ParameterCollection parameters) where T : class, new() {
            try {
                DataTableAttribute attrTable = GetTableAttribute<T>();
                if (attrTable.ReadOnly)
                    throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

                SetParameterCollectionDbObject(parameters);
                DatabaseObjectInstance.Remove(attrTable.TableName, attrTable.SchemaName, parameters);
            } catch { throw; }
        }

        /// <summary>Executes the reader.</summary>
        /// <param name="strSql">The string SQL.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string strSql) {
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                return DatabaseObjectInstance.ExecuteReader(strSql, CommandType.Text);
            } catch { throw; }
        }

        /// <summary>Executes the reader.</summary>
        /// <param name="strSql">The string SQL.</param>
        /// <param name="type">  The type.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string strSql, CommandType type) {
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                return DatabaseObjectInstance.ExecuteReader(strSql, type);
            } catch { throw; }
        }

        /// <summary>Executes the reader.</summary>
        /// <param name="strSql">   The string SQL.</param>
        /// <param name="type">     The type.</param>
        /// <param name="paramColl">The parameter coll.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string strSql, CommandType type, ParameterCollection paramColl) {
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                return DatabaseObjectInstance.ExecuteReader(strSql, type, paramColl);
            } catch { throw; }
        }

        /// <summary>Executes the reader.</summary>
        /// <param name="strSql">   The string SQL.</param>
        /// <param name="type">     The type.</param>
        /// <param name="paramColl">The parameter coll.</param>
        /// <param name="behavior"> The behavior.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string strSql, CommandType type, CommandBehavior behavior) {
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                return DatabaseObjectInstance.ExecuteReader(strSql, type, [], behavior);
            } catch { throw; }
        }

        /// <summary>Executes the reader.</summary>
        /// <param name="strSql">   The string SQL.</param>
        /// <param name="type">     The type.</param>
        /// <param name="paramColl">The parameter coll.</param>
        /// <param name="behavior"> The behavior.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string strSql, CommandType type, ParameterCollection paramColl, CommandBehavior behavior) {
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                paramColl.DerivedDatabaseObject = DatabaseObjectInstance as DatabaseObject;
                return DatabaseObjectInstance.ExecuteReader(strSql, type, paramColl, behavior);
            } catch { throw; }
        }

        /// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a single object.</summary>
        /// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <returns>An object of type T.</returns>
        public T? ExecuteStoredProcedure<T>(string storedProcedureName) where T : class, new() {
            List<T> list = ExecuteStoredProcedureForList<T>(storedProcedureName, []);
            return list == null ? null : list.Count > 0 ? list[0] : null;
        }

        /// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a single object.</summary>
        /// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <param name="parameters">         A collection of Key-Value pairs.</param>
        /// <returns>An object of type T.</returns>
        public T? ExecuteStoredProcedure<T>(string storedProcedureName, ParameterCollection parameters) where T : class, new() {
            SetParameterCollectionDbObject(parameters);
            List<T> list = ExecuteStoredProcedureForList<T>(storedProcedureName, parameters);
            return list == null ? null : list.Count > 0 ? list[0] : null;
        }

        /// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
        /// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <returns>An List of type T.</returns>
        public List<T> ExecuteStoredProcedureForList<T>(string storedProcedureName) where T : class, new() {
            IList<T> list = [];
            ExecuteStoredProcedureForList(ref list, storedProcedureName, []);
            return (List<T>)list;
        }

        /// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
        /// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <param name="parameters">         A collection of Key-Value pairs.</param>
        /// <returns>An List of type T.</returns>
        public List<T> ExecuteStoredProcedureForList<T>(string storedProcedureName, ParameterCollection parameters) where T : class, new() {
            IList<T> list = [];
            SetParameterCollectionDbObject(parameters);
            ExecuteStoredProcedureForList(ref list, storedProcedureName, parameters);
            return (List<T>)list;
        }

        /// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
        /// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
        /// <param name="list">               A reference to the list in which the items are to be returned.</param>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <param name="parameters">         A collection of Key-Value pairs.</param>
        public void ExecuteStoredProcedureForList<T>(ref IList<T> list, string storedProcedureName) where T : class, new() {
            IDataReader reader = null;
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                reader = DatabaseObjectInstance.ExecuteReader(storedProcedureName, CommandType.StoredProcedure, []);
                CreateBusinessEntity(ref list, reader);
            } catch { throw; } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }
        }

        /// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
        /// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
        /// <param name="list">               A reference to the list in which the items are to be returned.</param>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <param name="parameters">         A collection of Key-Value pairs.</param>
        public void ExecuteStoredProcedureForList<T>(ref IList<T> list, string storedProcedureName, ParameterCollection parameters) where T : class, new() {
            IDataReader reader = null;
            try {
                SetParameterCollectionDbObject(parameters);
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                reader = DatabaseObjectInstance.ExecuteReader(storedProcedureName, CommandType.StoredProcedure, parameters);
                CreateBusinessEntity(ref list, reader);
            } catch { throw; } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }
        }

        /// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSql">    The STR SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public List<T> ExecuteNonQueryForList<T>(string strSql) where T : class, new() {
            IList<T> list = [];
            ExecuteNonQueryForList(ref list, strSql, []);
            return (List<T>)list;
        }

        /// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSql">    The STR SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public List<T> ExecuteNonQueryForList<T>(string strSql, ParameterCollection? parameters) where T : class, new() {
            IList<T> list = [];
            if (parameters != null)
                SetParameterCollectionDbObject(parameters);
            ExecuteNonQueryForList(ref list, strSql, parameters);
            return (List<T>)list;
        }

        /// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">      The list.</param>
        /// <param name="strSql">    The STR SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public void ExecuteNonQueryForList<T>(ref IList<T> list, string strSql) where T : class, new() {
            IDataReader reader = null;
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                reader = DatabaseObjectInstance.ExecuteReader(strSql, CommandType.Text, []);
                CreateBusinessEntity(ref list, reader, true);
            } catch { throw; } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }
        }

        /// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">      The list.</param>
        /// <param name="strSql">    The STR SQL.</param>
        /// <param name="parameters">The parameters.</param>
        public void ExecuteNonQueryForList<T>(ref IList<T> list, string strSql, ParameterCollection parameters) where T : class, new() {
            IDataReader reader = null;
            try {
                if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
                SetParameterCollectionDbObject(parameters);
                reader = DatabaseObjectInstance.ExecuteReader(strSql, CommandType.Text, parameters);
                CreateBusinessEntity(ref list, reader, true);
            } catch { throw; } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }
        }

        /// <summary>Executes the SQL scaler.</summary>
        /// <param name="strSQL">The string SQL.</param>
        /// <returns></returns>
        public object ExecuteSQLScaler(string strSQL) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteScalar(strSQL, CommandType.Text, null);
        }

        /// <summary>Executes the SQL scaler.</summary>
        /// <param name="strSQL">    The STR SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteSQLScaler(string strSQL, CommandType commandType) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteScalar(strSQL, commandType, []);
        }

        /// <summary>Executes the SQL scaler.</summary>
        /// <param name="strSQL">    The STR SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteSQLScaler(string strSQL, CommandType commandType, ParameterCollection parameters) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            SetParameterCollectionDbObject(parameters);
            return DatabaseObjectInstance.ExecuteScalar(strSQL, commandType, parameters);
        }

        /// <summary>Executes the non query.</summary>
        /// <param name="strSQL">The string SQL.</param>
        public int ExecuteNonQuery(string strSQL) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteNonQuery(strSQL, CommandType.Text);
        }

        /// <summary>Executes the non-query.</summary>
        /// <param name="strSQL">     The STR SQL.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters"> The parameters.</param>
        public int ExecuteNonQuery(string strSQL, CommandType commandType) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteNonQuery(strSQL, commandType);
        }

        /// <summary>Executes the non-query.</summary>
        /// <param name="strSQL">     The STR SQL.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters"> The parameters.</param>
        public int ExecuteNonQuery(string strSQL, CommandType commandType, ParameterCollection parameters) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            SetParameterCollectionDbObject(parameters);
            return DatabaseObjectInstance.ExecuteNonQuery(strSQL, commandType, parameters);
        }

        /// <summary>Executes the query and returns as a DataTable.</summary>
        /// <param name="strSQL">The STR SQL.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string strSQL) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteDataTable(strSQL);
        }

        /// <summary>Executes the query and returns as a DataTable.</summary>
        /// <param name="strSQL">The STR SQL.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string strSQL, CommandType commandType) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteDataTable(strSQL);
        }

        /// <summary>Executes the query and returns as a DataSet.</summary>
        /// <param name="strSQL">The STR SQL.</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string strSQL) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteDataSet(strSQL);
        }

        /// <summary>Executes the query and returns as a DataSet.</summary>
        /// <param name="strSQL">     The STR SQL.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string strSQL, CommandType commandType) {
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return DatabaseObjectInstance.ExecuteDataSet(strSQL, commandType);
        }

        /// <summary>Executes the query and returns as a DataSet.</summary>
        /// <param name="strSQL">The STR SQL.</param>
        /// <param name="cmd">   <see cref="Microsoft.Data.CommandType" /></param>
        /// <param name="param"> The param.</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string strSQL, CommandType commandType, ParameterCollection? param) {
            param.DerivedDatabaseObject = DatabaseObjectInstance as DatabaseObject;
            if (CommandTimeout > -1) { DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
            return param != null
                ? DatabaseObjectInstance.ExecuteDataSet(strSQL, commandType, param)
                : DatabaseObjectInstance.ExecuteDataSet(strSQL, commandType);
        }

        /// <summary>Executes the bulk copy.</summary>
        /// <param name="records">         The records.</param>
        /// <param name="destinationTable">The destination table.</param>
        /// <exception cref="ArgumentNullException">The table name wasn't found. If the destinationTable is not passed it must be passed as the DataTable TableName property.</exception>
        /// <exception cref="NotSupportedException">The current database type, " + SupportedDatabase.ToString() + " is not supported by this bulk insert method.</exception>
        /// <returns></returns>
        public bool ExecuteBulkCopy(DataTable records, int notificationRecordInterval, string? destinationTable) {
            DatabaseObjectInstance.BulkCopySqlRowsCopied += ExecuteBulkCopy_SqlRowsCopied;
            return DatabaseObjectInstance.ExecuteBulkCopy(records, notificationRecordInterval, destinationTable);
        }

        /// <summary>Handles the SqlRowsCopied event of the ExecuteBulkCopy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">     The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void ExecuteBulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) => BulkCopySqlRowsCopied?.Invoke(sender, e);

        /// <summary>Performs a sequence of database calls using the same transaction. Commits or Rollback automatically at the end depending on whether the code completed successfully.</summary>
        /// <param name="workToDo">Anonymous method or a delegate containing the code to wrap in a transaction</param>
        public void Transaction(Action workToDo) {
            try {
                BeginTransaction();
                workToDo();
            } catch (Exception ex) {
                Rollback();
                throw;
            } finally {
                Commit();
            }
        }

        /// <summary>Commits the operation.</summary>
        public void CommitOperation() {
            if (TransactionInProgress) {
                TransactionInProgress = false;
                DatabaseObjectInstance.CommitTransaction();
            }
        }

        /// <summary>Begins the transaction.</summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction() {
            TransactionInProgress = true;
            return DatabaseObjectInstance.BeginTransaction();
        }

        /// <summary>Commits a transaction inside of the <see cref="Transaction" /> method.</summary>
        public void Commit() {
            if (TransactionInProgress) {
                TransactionInProgress = false;
                DatabaseObjectInstance.CommitTransaction();
            }
        }

        /// <summary>Tests the connection.</summary>
        /// <returns></returns>
        public bool TestConnection() => DatabaseObjectInstance.TestConnection();

        /// <summary>Rollback a transaction inside of the <see cref="Transaction" /> method.</summary>
        public void Rollback() {
            if (TransactionInProgress) {
                TransactionInProgress = false;
                DatabaseObjectInstance.RollbackTransaction();
            }
        }
        /// <summary>
        /// Gets the table name from passed type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetTableNameFromObject<T>() {
            DataTableAttribute table = GetTableAttribute<T>();
            return table == null
                ? throw new Exception($"The DataTable table attribute could not be found based on the pass generic type, {nameof(T)}.")
                : table.TableName;
        }
        /// <summary>
        /// Gets the table name from passed type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public string GetTableNameFromObject(string typeName) {
            DataTableAttribute table = GetTableAttribute(typeName);
            return table == null
                ? throw new Exception($"The DataTable table attribute could not be found based on the pass generic type, {typeName}.")
                : table.TableName;
        }
        /// <summary>
		/// Gets the schema name from passed type.  If none is
		/// configured then the default schema will be used if set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="string"/></returns>
        public string? GetSchemaNameFromObject<T>() {
            DataTableAttribute table = GetTableAttribute<T>();
            return table == null
                ? throw new Exception($"The DataTable table attribute could not be found based on the passed type, {nameof(T)}.")
                : string.IsNullOrEmpty(table.SchemaName) ? DefaultSchemaName : table.SchemaName;
        }
        /// <summary>
        /// Gets the schema name from the passed type name.  If none is
        /// configured then the default schema will be used if set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see cref="string"/></returns>
        public string? GetSchemaNameFromObject(string typeName) {
            DataTableAttribute table = GetTableAttribute(typeName);
            return table == null
                ? throw new Exception($"The DataTable table attribute could not be found based on the pass generic type, {typeName}.")
                : string.IsNullOrEmpty(table.SchemaName) ? DefaultSchemaName : table.SchemaName;
        }

        /// <summary>Gets the view name from object type.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string? GetViewNameFromObject<T>() {
            DataTableAttribute table = GetTableAttribute<T>();
            return table == null
                ? throw new Exception($"The DataTable table attribute could not be found based on the passed type, {nameof(T)}.")
                : table.ViewName;
        }

        /// <summary>Gets the view name from the object's name.</summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string? GetViewNameFromObject(string typeName) {
            DataTableAttribute table = GetTableAttribute(typeName);
            return table == null
                ? throw new Exception($"The DataTable table attribute could not be found based on the pass generic type, {typeName}.")
                : table.ViewName;
        }

        /// <summary>Gets the source name from object.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("The method is obsolete.  The full source name should be pulled from the inheriting provider's methods of GetTableName and GetSchemaName.")]
        public string GetSourceNameFromObject<T>() {
            DataTableAttribute table = GetTableAttribute<T>();
            return DatabaseObjectInstance.GetTableName(DefaultSchemaName, table.SourceName);
        }

        /// <summary>Gets the source name from object.</summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        [Obsolete("The method is obsolete.  The full source name should be pulled from the inheriting provider's methods of GetTableName and GetSchemaName.")]
        public string GetSourceNameFromObject(string typeName) {
            DataTableAttribute table = GetTableAttribute(typeName);
            return DatabaseObjectInstance.GetTableName(DefaultSchemaName, table.SourceName);
        }

        /// <summary>Gets the column name from the passed property name.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public string GetColumnNameFromProperty<T>(string propertyName) {
            string columName = string.Empty;
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    columName = attrColumn.ColumnName;
                    break;
                }
            }
            return columName;
        }

        /// <summary>Gets the property name from the column name.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string GetPropertyNameColumn<T>(string columnName) {
            string propertyName = string.Empty;
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)) {
                    propertyName = attrColumn.PropertyName;
                    break;
                }
            }
            return propertyName;
        }
        /// <summary>
        /// Gets the DataTableAttribute associated with the passed data aware type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DataTableAttribute GetTableAttribute<T>() {
            DataTableAttribute table = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
            return table ?? throw new Exception($"The DataTable table attribute could not be found based on the pass generic type, {nameof(T)}.");
        }
        /// <summary>
        /// Gets the DataTableAttribute associated with the passed data aware type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DataTableAttribute GetTableAttribute(string typeName) {
            DataTableAttribute table = (DataTableAttribute)ORM[typeName][Type.GetType(typeName)];
            return table ?? throw new Exception($"The DataTable table attribute could not be found based on the pass generic type, {typeName}.");
        }
        /// <summary>Gets the column data attribute.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public DataColumnAttribute GetColumnDataAttribute<T>(string propertyName) {
            DataColumnAttribute propAttr = null;
            object[] arrColumnAttributes;

            foreach (PropertyInfo pi in typeof(T).GetProperties()) {
                if (pi.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    arrColumnAttributes = pi.GetCustomAttributes(typeof(DataColumnAttribute), true);
                    if (arrColumnAttributes.Length > 0) {
                        propAttr = (DataColumnAttribute)arrColumnAttributes[0];
                        break;
                    }
                }
            }

            return propAttr;
        }

        /// <summary>Gets the table definition.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeIdentityColumns">The include identity columns.</param>
        /// <returns></returns>
        public List<TableColumn> GetTableDefinition<T>(bool includeIdentityColumns) {
            try {
                List<TableColumn> tableDefinition = DatabaseObjectInstance.GetTableDefinition(GetTableNameFromObject<T>());

                foreach (TableColumn col in tableDefinition) {
                    col.CorrespondingPropertyName = GetPropertyNameColumn<T>(col.Name);
                }
                return !includeIdentityColumns
                    ? tableDefinition.FindAll(delegate (TableColumn c) { return c.IsIdentity == false; })
                    : tableDefinition;
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>Determines whether the property is key to the object.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if [is property key] [the specified property name]; otherwise, <c>false</c>.</returns>
        public bool IsPropertyKey<T>(string propertyName) {
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    return attrColumn.IsPrimaryKey;
                }
            }
            return false;
        }

        /// <summary>Gets the <see cref="DataColumnAttribute" /> marked with the primary key flag.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("This method is obsolete due to the naming.  Please use GetPrimaryKeyAttribute().")]
        public DataColumnAttribute GetKeyPropertyName<T>() {
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.IsPrimaryKey) {
                    return attrColumn;
                }
            }
            return new DataColumnAttribute("", DbType.String);
        }

        /// <summary>Gets the <see cref="DataColumnAttribute" /> marked with the primary key flag.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DataColumnAttribute GetPrimaryKeyAttribute<T>() {
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.IsPrimaryKey) {
                    return attrColumn;
                }
            }
            return new DataColumnAttribute("", DbType.String);
        }
        /// <summary>
        /// Sets the primary key's value for the object passed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        public void SetPrimaryKeyValue<T>(Object obj, object val) {
            DataColumnAttribute keyProperty = GetPrimaryKeyAttribute<T>();
            SetPrimaryKeyValue<T>(obj, keyProperty.PropertyName, val);
        }
        /// <summary>
        /// Sets the primary key's value for the passed object and key property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="keyPropertyName"></param>
        /// <param name="val"></param>
        public void SetPrimaryKeyValue<T>(Object obj, string keyPropertyName, object val) {
            PropertyInfo pi = typeof(T).GetProperty(keyPropertyName);
            pi.SetValue(obj, val, null);
        }
        /// <summary>Makes a parameter from the supplied object property with the given value</summary>
        /// <typeparam name="T">The type of object from which to make the parameter.</typeparam>
        /// <param name="propertyName">The property of the object from which to make the parameter.</param>
        /// <param name="value">       The value to which the parameter should be set.</param>
        /// <returns></returns>
        public IDataParameter MakeParameter<T>(string propertyName, object value) {
            //TODO: determine if this function needs to return null
            IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    param.DbType = attrColumn.ColumnType;
                    param.Direction = ParameterDirection.Input;
                    param.ParameterName = attrColumn.ColumnName;
                    param.SourceColumn = attrColumn.ColumnName;
                    param.Value = value;
                    break;
                }
            }
            return param;
        }

        /// <summary>Makes a parameter from the supplied object property with the property's value</summary>
        /// <typeparam name="T">The type of object from which to make the parameter.</typeparam>
        /// <param name="propertyName">The property of the object from which to make the parameter.</param>
        /// <param name="obj">         The object who's property should be used</param>
        /// <returns></returns>
        public IDataParameter MakeParameter<T>(string propertyName, T obj) {
            //TODO: determine if this function needs to return null
            IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    param.DbType = attrColumn.ColumnType;
                    param.Direction = ParameterDirection.Input;
                    param.ParameterName = attrColumn.ColumnName;
                    param.SourceColumn = attrColumn.ColumnName;
                    param.Value = typeof(T).GetProperty(propertyName).GetValue(obj, null);
                    break;
                }
            }
            return param;
        }

        /// <summary>Makes a parameter of type U that references database table T</summary>
        /// <typeparam name="T">The database mapped object to reference</typeparam>
        /// <typeparam name="U">The type of parameter desired</typeparam>
        /// <param name="propertyName">the column mapped property</param>
        /// <param name="value">       the value to set the parameter</param>
        /// <param name="whereType">   The where type operator to apply.</param>
        /// <returns></returns>
        public Parameter MakeParameter<T, U>(string propertyName, object value, WhereType whereType = WhereType.AND) where U : Parameter, new() {
            // Convert the parameter to the type that was requested.
            U param = new() {
                BaseParameter = DatabaseObjectInstance.CreateEmptyParameter()
            };
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    param.DbType = attrColumn.ColumnType;
                    param.Direction = ParameterDirection.Input;
                    param.ParameterName = attrColumn.ColumnName;
                    param.SourceColumn = attrColumn.ColumnName;
                    param.Value = value;
                    param.WhereType = whereType;
                    break;
                }
            }
            return param;
        }

        [Obsolete("This method is replaced by one that allows a specific column name to be passed. This method assumed the column name and parameter name to be the same.")]
        /// <summary>Make a parameter from the supplied data.</summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterType">The DbType of the parameter.</param>
        /// <param name="paramterValue">The value of the parameter.</param>
        /// <returns></returns>
        public IDataParameter MakeParameter(string parameterName, DbType parameterType, object paramterValue) {
            //TODO: determine if this function needs to return null
            IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
            param.DbType = parameterType;
            param.Direction = ParameterDirection.Input;
            param.ParameterName = parameterName;
            param.SourceColumn = parameterName;
            param.Value = paramterValue;

            return param;
        }
        /// <summary>Make a parameter from the supplied data.</summary>
        /// <param name="parameterName">The name of the parameter. This is also used as the source data column name.</param>
        /// <param name="dataColumnName">The corresponding column name for the parameter name.</param>
        /// <param name="parameterType">The DbType of the parameter.</param>
        /// <param name="paramterValue">The value of the parameter.</param>
        /// <returns></returns>
        public IDataParameter MakeParameter(string parameterName, string dataColumnName, DbType parameterType, object paramterValue) {
            //TODO: determine if this function needs to return null
            IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
            param.DbType = parameterType;
            param.Direction = ParameterDirection.Input;
            param.ParameterName = parameterName;
            param.SourceColumn = dataColumnName;
            param.Value = paramterValue;

            return param;
        }
        /// <summary>
        /// Makes a special type of parameter called a BetweenParameter. This type of parameter enables the addition of
        /// a BETWEEN clause in you database calls.
        /// </summary>
        /// <typeparam name="T">The type of DataAware class</typeparam>
        /// <param name="propertyName"></param>
        /// <param name="startVal"></param>
        /// <param name="endVal"></param>
        /// <returns></returns>
        public IDataParameter MakeBetweenParameter<T>(string propertyName, object startVal, object endVal) where T : class {

            IDataParameter startRangeParam = DatabaseObjectInstance.CreateEmptyParameter();
            IDataParameter endRangeParam = DatabaseObjectInstance.CreateEmptyParameter();
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                if (attrColumn.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    startRangeParam.DbType = attrColumn.ColumnType;
                    startRangeParam.Direction = ParameterDirection.Input;
                    startRangeParam.ParameterName = attrColumn.ColumnName;
                    startRangeParam.SourceColumn = attrColumn.ColumnName;
                    startRangeParam.Value = startVal;

                    endRangeParam.DbType = attrColumn.ColumnType;
                    endRangeParam.Direction = ParameterDirection.Input;
                    endRangeParam.ParameterName = attrColumn.ColumnName;
                    endRangeParam.SourceColumn = attrColumn.ColumnName;
                    endRangeParam.Value = endVal;

                    break;
                }
            }

            return new BetweenParameter(startRangeParam, endRangeParam);

        }
        /// <summary>
        /// Makes a special type of parameter called a ComparisonParameter. This type of parameter enables the addition of what type of comparison you want,
        /// and if it is based on direct column names or a parameterized query. The default is Column Based (aka Parameterized).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="comparison"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDataParameter MakeParameter<T>(string propertyName, ComparisonParameterType comparison, object value) {

            IDataParameter baseParam = MakeParameter<T>(propertyName, value);
            ComparisonParameter comparisonParameter = new(baseParam, comparison, ComparisonOrigin.ValueBased);
            return comparisonParameter;
        }
        #endregion Public Methods

        #region Private Methods

        /// <summary>Gets the column names from the passed data object.</summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<string> GetColumnNames<T>() {
            List<string> listColumns = [];
            foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                listColumns.Add(attrColumn.ColumnName);
            }
            return listColumns;
        }

        public List<string> GetApiParameters<T>() {
            List<string> listParameters = [];
            foreach (ApiDataParameterAttribute attrParam in ((ApiDataControllerAttribute)ORM[typeof(T).FullName][typeof(T)]).Properties) {
                listParameters.Add(attrParam.ParameterName);
            }
            return listParameters;
        }

        protected List<string> GetOrderByColumnNames<T>() {
            List<string> listColumns = [];
            //not supported at the moment
            //foreach (DataColumnAttribute attrColumn in ORM[typeof(T).FullName][typeof(T)].Properties)
            //{
            //	listColumns.Add(attrColumn.Name);
            //}
            return listColumns;
        }

        protected List<T> CreateBusinessEntity<T>(IDataReader reader, bool useOrdinal = false) where T : class, new() {
            IList<T> list = [];
            CreateBusinessEntity(ref list, reader, useOrdinal);
            return (List<T>)list;
        }

        protected List<T> CreateBusinessEntity<T>(DataTable dt, bool useOrdinal = false) where T : class, new() {
            IList<T> list = [];
            CreateBusinessEntity(ref list, dt, useOrdinal);
            return (List<T>)list;
        }

        protected void CreateBusinessEntity<T>(ref IList<T> list, IDataReader reader) where T : class, new() {
            T entity;
            DataTableAttribute attrTable = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
            while (reader.Read()) {
                entity = new T();
                for (int i = 0; i < reader.FieldCount; i++) {
                    foreach (DataColumnAttribute attrColumn in attrTable.Properties) {
                        if (attrColumn.ColumnName.ToLower() == reader.GetName(i).ToLower()) {
                            //extended object defined:
                            // - create an instance of the extended object (must have a parameterless constructor
                            // - set the extended object's value with the data from the database
                            // - set the mapped property value to the etended object
                            if (attrColumn.ExtendedPropertyType != null) {
                                object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

                                //type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                if (extendedObj.GetType().IsEnum) {
                                    PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                    object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
                                    Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
                                    object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
                                    pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int)enumValue), null);
                                } else {
                                    PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
                                    object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
                                    pi.SetValue(extendedObj, objValue, null);
                                }
                            } else {//normal property - set it directly
                                PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                object obj = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
                                pi.SetValue(entity, obj, null);
                            }
                            break;
                        }
                    }
                }
                list.Add(entity);
            }
        }

        protected void CreateBusinessEntity<T>(ref IList<T> list, DataTable dt) where T : class, new() {
            T entity;
            DataTableAttribute attrTable = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
            foreach (DataRow row in dt.Rows) {
                entity = new T();
                for (int i = 0; i < dt.Columns.Count; i++) {
                    foreach (DataColumnAttribute attrColumn in attrTable.Properties) {
                        if (attrColumn.ColumnName.ToLower() == dt.Columns[i].ColumnName.ToLower()) {
                            //extended object defined:
                            // - create an instance of the extended object (must have a parameterless constructor
                            // - set the extended object's value with the data from the database
                            // - set the mapped property value to the etended object
                            if (attrColumn.ExtendedPropertyType != null) {
                                object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

                                //type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                if (extendedObj.GetType().IsEnum) {
                                    PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                    object objValue = GetReaderValueAs(row[i], pi.PropertyType);
                                    Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
                                    object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
                                    pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int)enumValue), null);
                                } else {
                                    PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
                                    object objValue = GetReaderValueAs(row[i], pi.PropertyType);
                                    pi.SetValue(extendedObj, objValue, null);
                                }
                            } else {//normal property - set it directly
                                PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                object obj = GetReaderValueAs(row[i], pi.PropertyType);
                                pi.SetValue(entity, obj, null);
                            }
                            break;
                        }
                    }
                }
                list.Add(entity);
            }
        }

        /// <summary>Creates the business entity.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">      The list.</param>
        /// <param name="reader">    The reader.</param>
        /// <param name="useOrdinal">if set to <c>true</c> this process will use a process using ordinal positions.</param>
        protected void CreateBusinessEntity<T>(ref IList<T> list, IDataReader reader, bool useOrdinal = true) where T : class, new() {
            if (!useOrdinal) {
                CreateBusinessEntity(ref list, reader);
            } else {
                T entity;
                DataTableAttribute attrTable = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
                while (reader.Read()) {
                    entity = new T();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        DataColumnAttribute attrColumn = attrTable.Properties.SingleOrDefault(p => p.ColumnName.ToLower() == reader.GetName(i).ToLower());
                        if (attrColumn != null) {
                            //extended object defined:
                            // - create an instance of the extended object (must have a parameterless constructor
                            // - set the extended object's value with the data from the database
                            // - set the mapped property value to the etended object
                            if (attrColumn.ExtendedPropertyType != null) {
                                object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

                                //type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                if (extendedObj.GetType().IsEnum) {
                                    PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                    object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
                                    Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
                                    object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
                                    pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int)enumValue), null);
                                } else {
                                    PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
                                    object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
                                    pi.SetValue(extendedObj, objValue, null);
                                }
                            } else {//normal property - set it directly
                                PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                object obj = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
                                pi.SetValue(entity, obj, null);
                            }
                        }
                    }
                    list.Add(entity);
                }
            }
        }

        protected void CreateBusinessEntity<T>(ref IList<T> list, DataTable dt, bool useOrdinal = true) where T : class, new() {
            if (!useOrdinal) {
                CreateBusinessEntity(ref list, dt);
            } else {
                T entity;
                DataTableAttribute attrTable = (DataTableAttribute)ORM[typeof(T).FullName][typeof(T)];
                foreach (DataRow row in dt.Rows) {
                    entity = new T();
                    for (int i = 0; i < dt.Columns.Count; i++) {
                        DataColumnAttribute attrColumn = attrTable.Properties.SingleOrDefault(p => p.ColumnName.ToLower() == dt.Columns[i].ColumnName.ToLower());
                        if (attrColumn != null) {
                            //extended object defined:
                            // - create an instance of the extended object (must have a parameterless constructor
                            // - set the extended object's value with the data from the database
                            // - set the mapped property value to the etended object
                            if (attrColumn.ExtendedPropertyType != null) {
                                object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

                                //type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                if (extendedObj.GetType().IsEnum) {
                                    PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                    object objValue = GetReaderValueAs(row[i], pi.PropertyType);
                                    Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
                                    object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
                                    pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int)enumValue), null);
                                } else {
                                    PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
                                    object objValue = GetReaderValueAs(row[i], pi.PropertyType);
                                    pi.SetValue(extendedObj, objValue, null);
                                }
                            } else {//normal property - set it directly
                                PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
                                object obj = GetReaderValueAs(row[i], pi.PropertyType);
                                pi.SetValue(entity, obj, null);
                            }
                        }
                    }
                    list.Add(entity);
                }
            }
        }

        /// <summary>Converts the data table to an entity list.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public List<T> ConvertDataTableToList<T>(DataTable dt) where T : class, new() {
            try {
                List<T> list = [];
                return CreateBusinessEntity<T>(dt);
            } catch {
                throw;
            }
        }

        /// <summary>Gets the reader value as [defined data type].</summary>
        /// <param name="value">       The value.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <returns></returns>
        protected object? GetReaderValueAs(object value, Type targetType) {
            if (value == DBNull.Value)
                return null;

            // Handle null values for nullable types
            if (value == null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return null;
            }

            // Get the underlying type if it's nullable
            Type nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try {
                // Since Guids are not handled by the Convert.ChangeType system method we have to handle it here.
                return targetType.IsEnum
                    ? Enum.ToObject(targetType, value)
                    : targetType == typeof(Guid) ? Guid.Parse(value.ToString()) : Convert.ChangeType(value, nonNullableType);
            } catch (InvalidCastException) {
                throw new InvalidCastException($"Cannot convert value '{value}' to type {targetType.Name}");
            } catch (FormatException) {
                throw new FormatException($"Value '{value}' is not in a format suitable for type {targetType.Name}");
            } catch (OverflowException) {
                throw new OverflowException($"Value '{value}' is outside the range for type {targetType.Name}");
            }

            //if (value != DBNull.Value)
            //{
            //    return expectedType.GetType() == typeof(Enum)
            //        ? value
            //        : expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Nullable<>)
            //        ? ((DateTime?) value) ?? GetTypeMinValue(expectedType)
            //        : expectedType.Name switch
            //        {
            //            nameof(Boolean) => Convert.ToBoolean(value),
            //            nameof(Guid) => Guid.Parse(value.ToString()),
            //            nameof(Int16) => Int16.Parse(value.ToString()),
            //            nameof(Int32) => Int32.Parse(value.ToString()),
            //            nameof(DateTime) => DateTime.Parse(value.ToString()),
            //            nameof(Double) => Double.Parse(value.ToString()),
            //            nameof(Decimal) => Decimal.Parse(value.ToString()),
            //            nameof(Int64) => Int64.Parse(value.ToString()),
            //            _ => value,
            //        };
            //}
            //else
            //{
            //    if (expectedType.IsPrimitive
            //        || expectedType.Equals(typeof(string))
            //        || expectedType.Equals(typeof(bool)))
            //    {
            //        object? returnVal = null;
            //        Switch.On(expectedType)
            //            .Case(typeof(string), () => returnVal = String.Empty)
            //            .Case(typeof(String), () => returnVal = String.Empty)
            //            .Case(typeof(Char), () => returnVal = String.Empty)
            //            .Case(typeof(Boolean), () => returnVal = false)
            //            .Case(typeof(bool), () => returnVal = false)
            //            .Default(() => returnVal = null);

            //        return returnVal;
            //    }
            //    else
            //    {
            //        return GetTypeMinValue(expectedType);
            //    }
            //}
        }

        /// <summary>Formats the value for null based on the determined type of the value.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected object FormatValueForNull(object value) {
            if (value.Equals(null))
                return DBNull.Value;
            return FormatValueForNull(value, value.GetType());
        }

        /// <summary>Formats the value for null based on the value type passed.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected object FormatValueForNull(object value, Type valueType) {
            object returnVal = DBNull.Value;
            try {
                Switch.On(valueType)
                    .Case(typeof(short), () => returnVal = short.MinValue == (short)value ? DBNull.Value : value)
                    .Case(typeof(short?), () => returnVal = short.MinValue == (short)value ? DBNull.Value : value)
                    .Case(typeof(int), () => returnVal = int.MinValue == (int)value ? DBNull.Value : value)
                    .Case(typeof(int?), () => returnVal = int.MinValue == (int)value ? DBNull.Value : value)
                    .Case(typeof(long), () => returnVal = long.MinValue == (long)value ? DBNull.Value : value)
                    .Case(typeof(long?), () => returnVal = long.MinValue == (long)value ? DBNull.Value : value)
                    .Case(typeof(bool), () => returnVal = value == null ? DBNull.Value : value)
                    .Case(typeof(bool?), () => returnVal = value == null || !((bool?)Convert.ToBoolean(value)).HasValue ? DBNull.Value : value)
                    .Case(typeof(float), () => returnVal = float.MinValue == (float)value ? DBNull.Value : value)
                    .Case(typeof(float?), () => returnVal = float.MinValue == (float)value ? DBNull.Value : value)
                    .Case(typeof(double), () => returnVal = double.MinValue == (double)value ? DBNull.Value : value)
                    .Case(typeof(double?), () => returnVal = double.MinValue == (double)value ? DBNull.Value : value)
                    .Case(typeof(decimal), () => returnVal = decimal.MinValue == (decimal)value ? DBNull.Value : value)
                    .Case(typeof(decimal?), () => returnVal = decimal.MinValue == (decimal)value ? DBNull.Value : value)
                    .Case(typeof(string), () => returnVal = string.IsNullOrEmpty((string)value) ? DBNull.Value : value)
                    .Case(typeof(char), () => returnVal = string.IsNullOrEmpty((string)value) ? DBNull.Value : value)
                    .Case(typeof(char?), () => returnVal = string.IsNullOrEmpty((string)value) ? DBNull.Value : value)
                    .Case(typeof(DateTime), () => returnVal = DateTime.MinValue == ConvertToSafeDateTime(value) ? DBNull.Value : Convert.ToDateTime(value))
                    .Case(typeof(DateTime?), () => returnVal = DateTime.MinValue == ConvertToSafeDateTime(value) ? DBNull.Value : Convert.ToDateTime(value))
                    .Case(typeof(object), () => returnVal = value ?? DBNull.Value)
                    .Case(typeof(byte[]), () => returnVal = value == null || ((byte[])value).Length == 0 ? DBNull.Value : value)
                    .Case(typeof(Guid), () => returnVal = value == null || Guid.Empty == Guid.Parse(value.ToString()) ? DBNull.Value : Guid.Parse(value.ToString()))
                    .Case(typeof(Guid?), () => returnVal = value == null || Guid.Empty == Guid.Parse(value.ToString()) ? DBNull.Value : Guid.Parse(value.ToString()));
                return returnVal;
            } catch (CodedThoughtException ex) {
                throw;
            }
        }

        protected DateTime ConvertToSafeDateTime(object value) {
            DateTime.TryParse(value.ToString(), out DateTime dateTime);
            return dateTime;
        }

        protected void SetParameterCollectionDbObject(ParameterCollection param) {
            if (param != null) {
                param.DerivedDatabaseObject ??= DatabaseObjectInstance as DatabaseObject;
            }
        }

        #endregion Private Methods

        #region Static Methods

        /// <summary>Gets the type minimum value.</summary>
        /// <param name="expectedType">The expected type.</param>
        /// <returns></returns>
        protected static object GetTypeMinValue(Type expectedType) {
            object returnVal = null;
            CodedThought.Core.Switch.On(expectedType)
                .Case(typeof(short), () => returnVal = short.MinValue)
                .Case(typeof(int), () => returnVal = int.MinValue)
                .Case(typeof(long), () => returnVal = long.MinValue)
                .Case(typeof(float), () => returnVal = float.MinValue)
                .Case(typeof(double), () => returnVal = double.MinValue)
                .Case(typeof(decimal), () => returnVal = decimal.MinValue)
                .Case(typeof(string), () => returnVal = string.Empty)
                .Case(typeof(char), () => returnVal = string.Empty)
                .Case(typeof(DateTime), () => returnVal = DateTime.MinValue)
                .Case(typeof(DateTime?), () => returnVal = DateTime.MinValue)
                .Case(typeof(object), () => returnVal = null);
            return returnVal;
        }

        #endregion Static Methods

        #region Class Mapping

        /// <summary>
        /// Loops through all types of the registed assemblies (registered via the
        /// DataTableAttribute) and generates a DictionaryCollection of each type and its DB mappings
        /// </summary>
        /// <returns></returns>
        protected void GenerateMapCollection(Dictionary<string, Dictionary<Type, Attribute>> mapCollection, Assembly assembly) {
            mapCollection ??= [];

            foreach (Type type in assembly.GetTypes()) {
                if (mapCollection.ContainsKey(type.FullName) || mapCollection.ContainsKey($"{type.FullName}.api")) {
                    continue;
                }

                DataTableAttribute attrTable = type.GetDataTableAttribute(false);
                if (attrTable != null) {
                    DataTableUsageAttribute attrUsage = type.GetDataTableUsageAttribute(attrTable.IgnoreInherited);
                    attrUsage ??= new DataTableUsageAttribute();
                    if (mapCollection.ContainsKey(type.FullName))
                        throw new CodedThoughtException($"The type, {type.FullName}, already exists in the mapping.");
                    attrTable.ClassType = type;
                    attrTable.ClassName = type.Name;
                    attrTable.ReadOnly = attrUsage.UseAs.HasFlag(DataTableUsage.ReadOnly);
                    attrTable.IgnoreInherited = attrUsage.UseAs.HasFlag(DataTableUsage.IgnoreInherited);
                    attrTable.UseView = attrUsage.UseAs.HasFlag(DataTableUsage.ViewPriority);
                    attrTable.AutoGenerateUniqueIdentifier = attrUsage.UseAs.HasFlag(DataTableUsage.AutoGenerateUniqueIdentifier);
                    attrTable.ReadOnly = attrUsage.UseAs.HasFlag(DataTableUsage.ReadOnly);
                    MapTableColumns(type, attrTable);
                    Dictionary<Type, Attribute> dbDic = new() {
                        { type, attrTable }
                    };
                    mapCollection.Add(type.FullName, dbDic);
                }

                // Look for API Attributes
                ApiDataControllerAttribute paramTable = type.GetApiDataControllerAttribute(false);
                if (paramTable != null) {
                    if (mapCollection.ContainsKey($"{type.FullName}.api"))
                        throw new CodedThoughtException($"The type, {type.FullName}.api, already exists in the mapping.");
                    MapApiColumns(type, paramTable);
                    Dictionary<Type, Attribute> apiDic = new() {
                        { type, paramTable }
                    };
                    mapCollection.Add($"{type.FullName}.api", apiDic);
                }
            }
        }

        /// <summary>Finds and loads all <see cref="DataColumnAttribute" /> attributes into the passed <see cref="DataTableAttribute" /> attribute.</summary>
        /// <param name="type">     </param>
        /// <param name="attrTable"></param>
        protected void MapTableColumns(Type type, DataTableAttribute attrTable) {
            List<PropertyInfo> properties = !attrTable.IgnoreInherited
                ? type.GetProperties().ToList()
                : type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
            try {
                foreach (PropertyInfo pi in properties) {
                    DataColumnAttribute attr = pi.GetDataColumnAttributes(false);
                    if (attr != null) {
                        // Ignore properties not set up with the DataColumn Attribute.
                        attr.PropertyType = pi.PropertyType;
                        attr.PropertyName = pi.Name;
                        attr.IsNullableType = Nullable.GetUnderlyingType(pi.PropertyType) != null;
                        attrTable.Properties.Add(attr);
                    }
                }
            } catch { throw; }
        }

        /// <summary>Finds and loads all <see cref="ApiDataParameterAttribute" /> attributes into the passed <see cref="ApiDataControllerAttribute" /> attribute.</summary>
        /// <param name="type">   </param>
        /// <param name="attrApi"></param>
        protected void MapApiColumns(Type type, ApiDataControllerAttribute attrApi) {
            List<PropertyInfo> properties = type.GetProperties().ToList();
            try {
                foreach (PropertyInfo pi in properties) {
                    ApiDataParameterAttribute param = pi.GetApiDataParametersAttributes(false);
                    if (param != null) {
                        if (attrApi.Properties == null)
                            attrApi.Properties = [];
                        // Determine if this attribute should override any others.
                        if (attrApi.Properties.Count(p => p.ParameterName.ToLower() == param.ParameterName.ToLower()) > 0 && param.Options.HasFlag(ApiDataParameterOptions.OverridesInherited)) {
                            attrApi.Properties.ToList().ForEach(a => {
                                if (a.ParameterName.ToLower() == param.ParameterName.ToLower()) {
                                    if (!a.Options.HasFlag(ApiDataParameterOptions.OverridesInherited)) {
                                        attrApi.Properties.Remove(a);
                                    }
                                } else {
                                    attrApi.Properties.Add(param);
                                }
                            });
                        } else {
                            attrApi.Properties.Add(param);
                        }
                    }
                }
            } catch { throw; }
        }
        protected List<Assembly> GetDataAwareAssemblies() {
            try {
                List<Assembly> dataAwareAssemblies = [];
                List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                foreach (Assembly assembly in allAssemblies) {
                    if (Attribute.GetCustomAttribute(assembly, typeof(DataAwareAssemblyAttribute)) is DataAwareAssemblyAttribute isDataAware) { dataAwareAssemblies.Add(assembly); }
                }
                return dataAwareAssemblies;
            } catch { throw; }
        }
        #endregion Class Mapping

        #region IDBStore Members

        object IDBStore.Extract(object businessEntity, string columnName) {
            Type t = businessEntity.GetType();
            if (ORM.TryGetValue(t.FullName, out Dictionary<Type, Attribute>? value)) {
                foreach (DataColumnAttribute attrColumn in ((DataTableAttribute)value[t]).Properties) {
                    if (attrColumn.ColumnName == columnName) {
                        //original function body
                        //return t.GetProperty(attrColumn.PropertyName).GetValue(businessEntity, null);

                        if (attrColumn.ExtendedPropertyType != null) {
                            //the property is an extended object, so get the object first, then get the extended property from the extended object
                            object oExtendedObject = t.GetProperty(attrColumn.PropertyName).GetValue(businessEntity, null);
                            t = oExtendedObject.GetType();
                            return t.GetProperty(attrColumn.ExtendedPropertyName).GetValue(oExtendedObject, null);
                        } else {
                            return t.GetProperty(attrColumn.PropertyName).GetValue(businessEntity, null);
                        }
                    }
                }
            }
            return null;
        }

        int IDBStore.GetPrimaryKey(object obj) {
            try {
                Type t = obj.GetType();
                return ORM.TryGetValue(t.FullName, out Dictionary<Type, Attribute>? value)
                    ? ((DataTableAttribute)value[t]).Key.ColumnType switch {
                        DbType.Int16 or DbType.Int32 or DbType.Int64 or DbType.Decimal => Convert.ToInt32(t.GetProperty(((DataTableAttribute)value[t]).Key.PropertyName).GetValue(obj, null)),
                        _ => 0,
                    }
                    : 0;
            } catch { return 0; }

        }
        DataColumnAttribute IDBStore.GetPrimaryKeyColumnAttribute(object obj) {
            try {
                Type t = obj.GetType();
                DataColumnAttribute dca = null;
                if (ORM.TryGetValue(t.FullName, out Dictionary<Type, Attribute>? value)) {
                    dca = ((DataTableAttribute)value[t]).Key;
                }
                return dca;
            } catch { return null; }
        }
        string IDBStore.GetPrimaryKeyName(object obj) {
            try {
                Type t = obj.GetType();
                return ORM.TryGetValue(t.FullName, out Dictionary<Type, Attribute>? value) ? ((DataTableAttribute)value[t]).Key.ColumnName : string.Empty;
            } catch { return string.Empty; }
        }

        bool IDBStore.HasKeyColumn(object obj) {
            try {
                Type t = obj.GetType();
                return ORM.ContainsKey(t.FullName) && ((DataTableAttribute)ORM[t.FullName][t]).Key != null;
            } catch { return false; }
        }

        bool IDBStore.SetPrimaryKey(object obj, int value) {
            Type t = obj.GetType();
            if (ORM.TryGetValue(t.FullName, out Dictionary<Type, Attribute>? value2)) {
                t.GetProperty(((DataTableAttribute)value2[t]).Key.PropertyName).SetValue(obj, value, null);
                return true;
            } else
                return false;
        }
        bool IDBStore.SetPrimaryKey(object obj, Guid value) {
            Type t = obj.GetType();
            if (ORM.TryGetValue(t.FullName, out Dictionary<Type, Attribute>? value2)) {
                t.GetProperty(((DataTableAttribute)value2[t]).Key.PropertyName).SetValue(obj, value, null);
                return true;
            } else
                return false;
        }
        #endregion IDBStore Members
    }
}
