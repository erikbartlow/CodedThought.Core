using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data
{

	/// <summary>Reference Delegate for using the Transaction Method <see cref="TransactionInvoker" /></summary>
	public delegate void TransactionInvoker();

	/// <summary>DBStore implementation that uses attributes and reflection to map objects into tables</summary>
	public class GenericDataStore : IDBStore
	{

		#region Declarations

		protected const string ORM_KEY = "ORM";
		protected const string ORM_ASSEMBLIES_KEY = "ORM_ASSEMBLIES";
		protected Dictionary<string, Dictionary<Type, Attribute>>? ORM; //object-relational-mapping
		protected List<String>? listLoadedAssemblies;
		protected DatabaseConnection? _specifiedDatabaseConnection;
		protected readonly IMemoryCache? _cache = null;
		protected readonly runtime.MemoryCache? _runtimeCache = null;
		private string defaultSchemaName = string.Empty;

		public event SqlRowsCopiedEventHandler? BulkCopySqlRowsCopied;

		/// <summary>Occurs when [rows inserted] for Sql Server Bulk Insert methods.</summary>
		public event EventHandler<SqlRowsCopiedEventArgs>? RowsInserted;

		#endregion Declarations

		#region Properties

		public DatabaseConnection? CurrentDatabaseConnection { get; set; }
		public ConnectionSetting? ConnectionSetting { get; private set; }
		public bool UseHttpCache => _runtimeCache == null;
		/// <summary>Gets or sets the database object instance.</summary>
		/// <value>The database object instance.</value>
		protected IDatabaseObject DatabaseObjectInstance { get; private set; }

		/// <summary>Gets the connection.</summary>
		/// <value>The connection.</value>
		/// <exception cref="NullReferenceException">The database object has not been instantiated yet.</exception>
		public virtual IDbConnection Connection
		{
			get
			{
				return DatabaseObjectInstance == null
					? throw new NullReferenceException("The database object has not been instantiated yet.")
					: DatabaseObjectInstance.Connection;
			}
		}

		/// <summary>Gets or sets the name of the database schema.</summary>
		/// <value>The name of the database schema.</value>
		[Obsolete("This property is obsolete.  Please use DefaultSchemaName.")]
		public virtual string? DatabaseSchemaName
		{
			get => DefaultSchemaName;
			set => DefaultSchemaName = value;
		}
		/// <summary>
		/// Gets or sets the default schema name to be used.  This can be overridden by using the <see cref="DataTableAttribute.SchemaName"/> property of a data object.
		/// </summary>
		public virtual string? DefaultSchemaName
		{
			get => defaultSchemaName == string.Empty ? "[dbo]" : $"[{defaultSchemaName}]";
			set => defaultSchemaName = value;
		}

		/// <summary>Gets or sets the database connection to use.</summary>
		/// <value>The database to use.</value>
		public virtual DatabaseConnection DatabaseToUse
		{
			get => _specifiedDatabaseConnection;
			set
			{
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
		public virtual Int32 CommandTimeout { get; set; }

		/// <summary>Gets the wildcard character.</summary>
		/// <value>The wildcard character.</value>
		public virtual string WildcardCharacter => DatabaseObjectInstance.WildCardCharacter;

		#endregion Properties

		#region Constructors

		private GenericDataStore(runtime.MemoryCache cache)
		{
			_specifiedDatabaseConnection = null;
			_runtimeCache = cache;

			if (!_runtimeCache.TryGetValue<Dictionary<string, Dictionary<Type, Attribute>>>(ORM_KEY, out ORM))
			{
				ORM = new Dictionary<string, Dictionary<Type, Attribute>>();
			}
		}
		private GenericDataStore(IMemoryCache cache)
		{
			_specifiedDatabaseConnection = null;
			_cache = cache;

			if (!_cache.TryGetValue<Dictionary<string, Dictionary<Type, Attribute>>>(ORM_KEY, out ORM))
			{
				ORM = new Dictionary<string, Dictionary<Type, Attribute>>();
			}
		}

		/// <summary>
		/// Initializes a new instance of the GenericDataStore class. The internal ORM model (shared between all instances of the GenericDataStore class) is initialized to the calling assembly.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="databaseToUse"></param>
		public GenericDataStore(IServiceProvider serviceProvider, runtime.MemoryCache cache, ConnectionSetting databaseToUse) : this(cache)
		{
			// The calling assembly should be the assembly with the data aware classes to load into the ORM.
			if (ORM.Count == 0)
			{
				LoadAssemblyAndORM(Assembly.GetCallingAssembly());
			}
			TransactionInProgress = false;
			DatabaseObjectInstance = DatabaseObject.DatabaseObjectFactory(serviceProvider, cache, databaseToUse);
			DatabaseObjectInstance.CommandTimeout = 0;
			DatabaseToUse = new(databaseToUse);
		}
		/// <summary>
		/// Initializes a new instance of the GenericDataStore class. The internal ORM model (shared between all instances of the GenericDataStore class) is initialized to the calling assembly.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="cache">Runtime Cache</param>
		/// <param name="databaseToUse"></param>
		/// <param name="serviceKey">If using a keyed service</param>
		public GenericDataStore(IServiceProvider serviceProvider, runtime.MemoryCache cache, ConnectionSetting databaseToUse, string? serviceKey) : this(cache)
		{
			// The calling assembly should be the assembly with the data aware classes to load into the ORM.
			if (ORM.Count == 0)
			{
				LoadAssemblyAndORM(Assembly.GetCallingAssembly());
			}
			TransactionInProgress = false;
			DatabaseObjectInstance = DatabaseObject.DatabaseObjectFactory(serviceProvider, cache, databaseToUse, serviceKey);
			DatabaseObjectInstance.CommandTimeout = 0;
			DatabaseToUse = new(databaseToUse);
		}
		/// <summary>
		/// Initializes a new instance of the GenericDataStore class. The internal ORM model (shared between all instances of the GenericDataStore class) is initialized to the calling assembly.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="cache">Http Cache</param>
		/// <param name="databaseToUse"></param>
		public GenericDataStore(IServiceProvider serviceProvider, IMemoryCache cache, ConnectionSetting databaseToUse) : this(cache)
		{
			// The calling assembly should be the assembly with the data aware classes to load into the ORM.
			if (ORM.Count == 0)
			{
				LoadAssemblyAndORM(Assembly.GetCallingAssembly());
			}

			TransactionInProgress = false;
			DatabaseObjectInstance = DatabaseObject.DatabaseObjectFactory(serviceProvider, cache, databaseToUse);
			DatabaseObjectInstance.CommandTimeout = 0;
			DatabaseToUse = new(databaseToUse);
		}
		/// <summary>
		/// Initializes a new instance of the GenericDataStore class. The internal ORM model (shared between all instances of the GenericDataStore class) is initialized to the calling assembly.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="cache">Runtime Cache</param>
		/// <param name="databaseToUse"></param>
		/// <param name="serviceKey">If using a keyed service</param>
		public GenericDataStore(IServiceProvider serviceProvider, IMemoryCache cache, ConnectionSetting databaseToUse, string key) : this(cache)
		{
			// The calling assembly should be the assembly with the data aware classes to load into the ORM.
			if (ORM.Count == 0)
			{
				LoadAssemblyAndORM(Assembly.GetCallingAssembly());
			}

			TransactionInProgress = false;
			DatabaseObjectInstance = DatabaseObject.DatabaseObjectFactory(serviceProvider, cache, databaseToUse, key);
			DatabaseObjectInstance.CommandTimeout = 0;
			DatabaseToUse = new(databaseToUse);
		}

		#endregion Constructors

		#region Public Methods

		/// <summary>Creates a parameter collection.</summary>
		/// <returns></returns>
		public ParameterCollection CreateParameterCollection()
		{
			ParameterCollection param = new();
			SetParameterCollectionDbObject(param);
			return param;
		}

		/// <summary>
		/// Parses an assembly for ORM mappings.
		/// </summary>
		/// <param name="assembly">The assembly to parse.</param>
		public void LoadAssemblyAndORM(Assembly callingAssembly)
		{
			if (UseHttpCache)
			{
				listLoadedAssemblies = _cache.GetFromHttpCache<List<string>>(ORM_ASSEMBLIES_KEY);
				if (listLoadedAssemblies == null)
				{
					// Load all data aware assemblies.
					listLoadedAssemblies = new();
					List<Assembly> dataAwareAssemblies = GetDataAwareAssemblies();
					dataAwareAssemblies.ForEach(a =>
					{
						listLoadedAssemblies.Add(a.GetName().Name);
						GenerateMapCollection(ORM, a);
						// Cache the objects for next time.
						_cache.AddToHttpCache(ORM_KEY, ORM);
						_cache.AddToHttpCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
					});
				}
				listLoadedAssemblies ??= new List<string>();
				if (!listLoadedAssemblies.Contains(callingAssembly.GetName().Name))
				{
					listLoadedAssemblies.Add(callingAssembly.GetName().Name);
					GenerateMapCollection(ORM, callingAssembly);

					//re-cache the objects after updates
					_cache.AddToHttpCache(ORM_KEY, ORM);
					_cache.AddToHttpCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
				}
			}
			else
			{
				listLoadedAssemblies = _runtimeCache.GetFromLocalCache<List<string>>(ORM_ASSEMBLIES_KEY);
				if (listLoadedAssemblies == null)
				{
					// Load all data aware assemblies.
					listLoadedAssemblies = new();
					List<Assembly> dataAwareAssemblies = GetDataAwareAssemblies();
					dataAwareAssemblies.ForEach(a =>
					{
						listLoadedAssemblies.Add(a.GetName().Name);
						GenerateMapCollection(ORM, a);
						// Cache the objects for next time.
						_runtimeCache.AddToLocalCache(ORM_KEY, ORM);
						_runtimeCache.AddToLocalCache(ORM_ASSEMBLIES_KEY, listLoadedAssemblies);
					});
				}
				if (!listLoadedAssemblies.Contains(callingAssembly.GetName().Name))
				{
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
		public T Get<T>(int objectID) where T : class, new()
		{
			ParameterCollection parameters = new();
			SetParameterCollectionDbObject(parameters);
			parameters.AddInt32Parameter(((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Key.ColumnName, objectID);
			return Get<T>(parameters);
		}

		/// <summary>Retrieves an object based on the supplied Key-Value pair collection.</summary>
		/// <typeparam name="T">The type of object to retrieve.</typeparam>
		/// <param name="parameters">A collection of Key-Value pairs.</param>
		/// <returns>Returns an object of the type T.</returns>
		public T Get<T>(ParameterCollection parameters) where T : class, new()
		{
			IDataReader reader = null;
			T entity = null;
			List<T> list = null;
			try
			{
				List<string> selectColumns = GetColumnNames<T>();
				List<string> orderColumns = GetOrderByColumnNames<T>();
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				SetParameterCollectionDbObject(parameters);
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				reader = DatabaseObjectInstance.Get(GetSourceNameFromObject<T>(), selectColumns, parameters, orderColumns);

				list = CreateBusinessEntity<T>(reader);
				if (list.Count > 0)
				{
					entity = list[0];
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}

			return entity;
		}

		/// <summary>Retrieves a list of objects matching the supplied Kay-Value par criteria.</summary>
		/// <typeparam name="T">The type of objects to retrieve.</typeparam>
		/// <param name="parameters">A collection of Key-Value pairs.</param>
		/// <returns>Returns a List&lt;&gt; of type T</returns>
		public List<T> GetMultiple<T>(ParameterCollection parameters) where T : class, new()
		{
			SetParameterCollectionDbObject(parameters);
			IList<T> list = new List<T>();
			GetMultiple(ref list, parameters);
			return (List<T>) list;
		}

		/// <summary>Retrieves a list of objects matching the supplied Kay-Value par criteria.</summary>
		/// <typeparam name="T">The type of objects to retrieve.</typeparam>
		/// <param name="list">      A reference to the list in which the items are to be returned.</param>
		/// <param name="parameters">A collection of Key-Value pairs.</param>
		public void GetMultiple<T>(ref IList<T> list, ParameterCollection parameters) where T : class, new()
		{
			IDataReader reader = null;
			try
			{
				List<string> selectColumns = GetColumnNames<T>();
				List<string> orderColumns = GetOrderByColumnNames<T>();
				SetParameterCollectionDbObject(parameters);
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				reader = DatabaseObjectInstance.Get(GetSourceNameFromObject<T>(), selectColumns, parameters, orderColumns);
				CreateBusinessEntity(ref list, reader, true);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}
		}

		/// <summary>Sorts the specified list.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">         The list.</param>
		/// <param name="sortBy">       The property to sort by.</param>
		/// <param name="sortDirection">The direction to sort the list.</param>
		public void Sort<T>(ref List<T> list, PropertyInfo sortBy, ListSortDirection sortDirection) where T : class, new()
		{
			try
			{
				string propertyName = sortBy.Name;

				list.Sort(delegate (T obj1, T obj2)
				{
					return obj1.GetType().GetProperty(propertyName).MemberType.CompareTo(obj2.GetType().GetProperty(propertyName).MemberType);
				});
			}
			catch
			{
				throw;
			}
		}

		/// <summary>Sorts the specified list.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">         The list.</param>
		/// <param name="sortBy">       The property name to sort by.</param>
		/// <param name="sortDirection">The sort direction to sort.</param>
		public void Sort<T>(ref List<T> list, string sortBy, ListSortDirection sortDirection) where T : class, new()
		{
			try
			{
				this.Sort(ref list, typeof(T).GetProperty(sortBy), sortDirection);
			}
			catch
			{
				throw;
			}
		}

		/// <summary>Saves an object to the database. If the object has a non-zero primary key, an update is performed otherwise an insert is performed.</summary>
		/// <typeparam name="T">The type of objects to save.</typeparam>
		/// <param name="obj">The object to save.</param>
		public void Save<T>(T obj)
		{
			bool bIsNew = true;
			//determine if this is an insert or an update
			DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			if (attrTable.Key != null)
			{
				object oPrimaryKey = typeof(T).GetProperty(attrTable.Key.PropertyName).GetValue(obj, null);
				if (((int) oPrimaryKey) > 0)
				{
					bIsNew = false;
				}
			}
			if (bIsNew)
			{
				//insert
				SaveNew(obj);
			}
			else
			{
				//update
				SaveExisting(obj);
			}
		}

		/// <summary>Saves the specified obj as new.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The obj.</param>
		public void SaveNew<T>(T obj)
		{
			//insert
			DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			if (attrTable.ReadOnly)
				throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

			List<TableColumn> listColumns = new();
			foreach (DataColumnAttribute attrColumn in attrTable.Properties)
			{
				TableColumn tc = new(attrColumn.ColumnName, attrColumn.ConvertTypeToDbTypeSupported(), attrColumn.Size, attrColumn.IsUpdateable);
				tc.isInsertable = tc.isUpdateable;
				listColumns.Add(tc);
			}
			DatabaseObjectInstance.Add(GetTableNameFromObject<T>(), obj, listColumns, this);
		}

		/// <summary>Saves an existing object.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The obj.</param>
		public void SaveExisting<T>(T obj)
		{
			//update
			DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			if (attrTable.ReadOnly)
				throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

			ParameterCollection oParamColumns = new();
			ParameterCollection oParamWhere = new();
			SetParameterCollectionDbObject(oParamColumns);
			SetParameterCollectionDbObject(oParamWhere);

			foreach (DataColumnAttribute attrColumn in attrTable.Properties)
			{
				IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
				param.DbType = attrColumn.ColumnType;
				param.Direction = ParameterDirection.Input;
				param.ParameterName = attrColumn.ColumnName;
				param.SourceColumn = attrColumn.ColumnName;

				// Check if we need to get the value from a sub object or from the local variable.
				object value;
				if (attrColumn.ExtendedPropertyType == null)
				{
					value = typeof(T).GetProperty(attrColumn.PropertyName).GetValue(obj, null);
				}
				else
				{
					Type extendedType = attrColumn.ExtendedPropertyType;
					if (extendedType.IsEnum)
					{
						value = (int) typeof(T).GetProperty(attrColumn.PropertyName).GetValue(obj, null);
					}
					else
					{
						object oExtendedObject = typeof(T).GetProperty(attrColumn.PropertyName).GetValue(obj, null);
						value = oExtendedObject.GetType().GetProperty(attrColumn.ExtendedPropertyName).GetValue(oExtendedObject, null);
					}
				}
				param.Value = FormatValueForNull(value);

				//set primary key as where clause
				if (attrColumn == attrTable.Key)
				{
					oParamWhere.Add(param);
				}
				else
				{
					// This isn't the primary key so add it to the main parameter collection.
					oParamColumns.Add(param);
				}
			}
			//no primary key: throw error
			if (oParamWhere.Count == 0)
			{
				throw new Exceptions.CodedThoughtApplicationException("Cannot perform an update when no where clause is specified.");
			}
			DatabaseObjectInstance.Update(GetTableNameFromObject<T>(), oParamColumns, oParamWhere);
		}

		/// <summary>Uses bulk insert and learns the generic object's table structure to complete the process.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="records">The records.</param>
		/// <returns></returns>
		public Boolean SaveBulk<T>(List<T> records, int notifyAfter = 2000)
		{
			try
			{
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				if (attrTable.ReadOnly)
					throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

				String tableName = GetTableNameFromObject<T>();
				DataTable dt = new(tableName);
				List<TableColumn> tableColumns = DatabaseObjectInstance.GetTableDefinition(tableName);
				tableColumns.Sort(delegate (TableColumn c1, TableColumn c2)
				{ return c1.ordinalPosition.CompareTo(c2.ordinalPosition); });

				tableColumns = tableColumns.Where(col => GetPropertyNameColumn<T>(col.name) != String.Empty && col.isIdentity != true).ToList();
				// Propertied columns refers to columns in the table that indeed have a property assigned to them in the object.
				List<TableColumn> propertiedColumns = tableColumns.Where(col => col.isIdentity != true)
					.Select(col =>
					{
						col.correspondingPropertyName = GetPropertyNameColumn<T>(col.name);
						return col;
					}).ToList();
				// Create the DataTable from the object. This allows changes to be made in the data object and flow to this controller.
				foreach (TableColumn col in propertiedColumns)
				{
					DataColumnAttribute colAttrib = GetColumnDataAttribute<T>(col.correspondingPropertyName);
					dt.Columns.Add(col.name, colAttrib.PropertyType);
				}

				// Convert the list to a DataTable.
				List<PropertyInfo> properties = typeof(T).GetProperties().ToList();
				foreach (T obj in records)
				{
					DataRow row = dt.NewRow();
					foreach (PropertyInfo prop in properties)
					{
						DataColumnAttribute dataColumn = prop.GetDataColumnAttributes();
						// Set the data column's value while handling nulls.
						row[dataColumn.ColumnName] = FormatValueForNull(prop.GetValue(obj, null), prop.PropertyType);
					}
					dt.Rows.Add(row);
				}
				if (DatabaseToUse.DatabaseType != DBSupported.SqlServer)
				{
					throw new NotSupportedException("The current database type, " + DatabaseToUse.DatabaseType.ToString() + " is not supported by this bulk insert method.");
				}
				else
				{
					// make sure to enable triggers more on triggers in next post
					SqlBulkCopy bulkCopy =
						new((SqlConnection) DatabaseObjectInstance.Connection)
						{
							BulkCopyTimeout = (DatabaseObjectInstance.CommandTimeout < 0 ? 0 : DatabaseObjectInstance.CommandTimeout)
						};

					// Set up the bulk copy mappings
					int x = 0;
					foreach (TableColumn col in propertiedColumns)
					{
						//bulkCopy.ColumnMappings.Add( x, col.ordinalPosition ); // Part Number
						bulkCopy.ColumnMappings.Add(x, col.ordinalPosition);
						x++;
					}
					// Set the destination table name
					bulkCopy.DestinationTableName = GetTableNameFromObject<T>();
					if (Connection.State != ConnectionState.Open)
						Connection.Open();

					// write the data in the dataTable write the data in the dataTable
					bulkCopy.NotifyAfter = dt.Rows.Count < notifyAfter ? dt.Rows.Count : notifyAfter;
					bulkCopy.BatchSize = bulkCopy.NotifyAfter;
					bulkCopy.SqlRowsCopied += bulkCopy_SqlRowsCopied;
					try
					{
						bulkCopy.WriteToServer(dt);
					}
					catch (Exception ex)
					{
						if (ex.Message.Contains("Received an invalid column length"))
						{
							string errorMessage = string.Empty;
							errorMessage = GetBulkCopyColumnException(ex, bulkCopy);
							throw new Exception(errorMessage, ex);
						}
						else
						{
							throw ex;
						}
					}
				}
				return true;
			}
			catch (CodedThoughtException ex)
			{
				throw ex;
			}
		}

		public Boolean SaveBulk<T>(DataTable records, int notifyAfter = 2000)
		{
			try
			{
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				if (attrTable.ReadOnly)
					throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

				DataTable dt = records;
				List<TableColumn> tableColumns = DatabaseObjectInstance.GetTableDefinition(GetTableNameFromObject<T>());
				tableColumns.Sort(delegate (TableColumn c1, TableColumn c2)
				{ return c1.ordinalPosition.CompareTo(c2.ordinalPosition); });

				tableColumns = tableColumns.Where(col => GetPropertyNameColumn<T>(col.name) != String.Empty && col.isIdentity != true).ToList();
				// Propertied columns refers to columns in the table that indeed have a property assigned to them in the object.
				List<TableColumn> propertiedColumns = tableColumns.Where(col => col.isIdentity != true)
					.Select(col =>
					{
						col.correspondingPropertyName = GetPropertyNameColumn<T>(col.name);
						return col;
					}).ToList();

				if (DatabaseToUse.DatabaseType != DBSupported.SqlServer)
				{
					throw new NotSupportedException("The current database type, " + DatabaseToUse.DatabaseType.ToString() + " is not supported by this bulk insert method.");
				}
				else
				{
					// make sure to enable triggers more on triggers in next post
					SqlBulkCopy bulkCopy =
						new((SqlConnection) DatabaseObjectInstance.Connection)
						{
							BulkCopyTimeout = DatabaseObjectInstance.CommandTimeout
						};

					// Set up the bulk copy mappings
					int x = 0;
					foreach (TableColumn col in propertiedColumns)
					{
						bulkCopy.ColumnMappings.Add(x, col.ordinalPosition);
						x++;
					}
					// Set the destination table name
					bulkCopy.DestinationTableName = GetTableNameFromObject<T>();
					if (Connection.State != ConnectionState.Open)
						Connection.Open();

					// write the data in the dataTable write the data in the dataTable
					bulkCopy.NotifyAfter = dt.Rows.Count < notifyAfter ? dt.Rows.Count : notifyAfter;
					bulkCopy.BatchSize = bulkCopy.NotifyAfter;
					bulkCopy.SqlRowsCopied += bulkCopy_SqlRowsCopied;
					try
					{
						bulkCopy.WriteToServer(dt);
					}
					catch (Exception ex)
					{
						if (ex.Message.Contains("Received an invalid column length"))
						{
							string errorMessage = string.Empty;
							errorMessage = GetBulkCopyColumnException(ex, bulkCopy);
							throw new Exception(errorMessage, ex);
						}
						else
						{
							throw ex;
						}
					}
				}
				return true;
			}
			catch (CodedThoughtException ex)
			{
				throw ex;
			}
		}
		[Obsolete("Due to naming violations from previous .NET versions this signature is obsolete.  Please use the one with the capitalized start.")]
#pragma warning disable IDE1006 // Naming Styles
		protected void bulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) => RowsInserted?.Invoke(this, e);
#pragma warning restore IDE1006 // Naming Styles

		/// <summary>Handles the SqlRowsCopied event of the bulkCopy control.</summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">     The <see cref="SqlRowsCopiedEventArgs" /> instance containing the event data.</param>
		protected void BulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) => RowsInserted?.Invoke(this, e);

		protected string GetBulkCopyColumnException(Exception ex, SqlBulkCopy bulkcopy)
		{
			string message = string.Empty;
			if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
			{
				string pattern = @"\d+";
				Match match = Regex.Match(ex.Message.ToString(), pattern);
				int index = Convert.ToInt32(match.Value) - 1;

				FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
				object? sortedColumns = fi.GetValue(bulkcopy);
				object[]? items = (Object[]) sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

				FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
				object? metadata = itemdata.GetValue(items[index]);
				object? column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
				object? length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
				message = (String.Format("Column: {0} contains data with a length greater than: {1}", column, length));
			}
			return message;
		}

		/// <summary>Removes an object from the database with the supplied primary key.</summary>
		/// <typeparam name="T">The type of object to remove.</typeparam>
		/// <param name="objectID">The primary key of the object.</param>
		public void Remove<T>(int objectID) where T : class, new()
		{
			ParameterCollection parameters = new();
			SetParameterCollectionDbObject(parameters);
			parameters.AddInt32Parameter(((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Key.ColumnName, objectID);
			Remove<T>(parameters);
		}

		/// <summary>Removes an object from the database with the specified parameters.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parameters">The parameters.</param>
		public void Remove<T>(ParameterCollection parameters) where T : class, new()
		{
			try
			{
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				if (attrTable.ReadOnly)
					throw new Exception($"This component, {typeof(T).Name}, is coded to be Read-Only.  Therefore no update or delete operations can be performed against it.");

				SetParameterCollectionDbObject(parameters);
				DatabaseObjectInstance.Remove(GetTableNameFromObject<T>(), parameters);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Executes the reader.</summary>
		/// <param name="strSql">The string SQL.</param>
		/// <returns></returns>
		public IDataReader ExecuteReader(string strSql)
		{
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				return DatabaseObjectInstance.ExecuteReader(strSql, CommandType.Text);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Executes the reader.</summary>
		/// <param name="strSql">The string SQL.</param>
		/// <param name="type">  The type.</param>
		/// <returns></returns>
		public IDataReader ExecuteReader(string strSql, CommandType type)
		{
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				return DatabaseObjectInstance.ExecuteReader(strSql, type);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Executes the reader.</summary>
		/// <param name="strSql">   The string SQL.</param>
		/// <param name="type">     The type.</param>
		/// <param name="paramColl">The parameter coll.</param>
		/// <returns></returns>
		public IDataReader ExecuteReader(string strSql, CommandType type, ParameterCollection paramColl)
		{
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				return DatabaseObjectInstance.ExecuteReader(strSql, type, paramColl);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Executes the reader.</summary>
		/// <param name="strSql">   The string SQL.</param>
		/// <param name="type">     The type.</param>
		/// <param name="paramColl">The parameter coll.</param>
		/// <param name="behavior"> The behavior.</param>
		/// <returns></returns>
		public IDataReader ExecuteReader(string strSql, CommandType type, CommandBehavior behavior)
		{
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				return DatabaseObjectInstance.ExecuteReader(strSql, type, new(), behavior);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Executes the reader.</summary>
		/// <param name="strSql">   The string SQL.</param>
		/// <param name="type">     The type.</param>
		/// <param name="paramColl">The parameter coll.</param>
		/// <param name="behavior"> The behavior.</param>
		/// <returns></returns>
		public IDataReader ExecuteReader(string strSql, CommandType type, ParameterCollection paramColl, CommandBehavior behavior)
		{
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				paramColl.DerivedDatabaseObject = DatabaseObjectInstance as DatabaseObject;
				return DatabaseObjectInstance.ExecuteReader(strSql, type, paramColl, behavior);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a single object.</summary>
		/// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
		/// <param name="storedProcedureName">The name of the stored procedure.</param>
		/// <returns>An object of type T.</returns>
		public T ExecuteStoredProcedure<T>(string storedProcedureName) where T : class, new()
		{
			List<T> list = ExecuteStoredProcedureForList<T>(storedProcedureName, new());

			return list.Count > 0 ? list[0] : null;
		}

		/// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a single object.</summary>
		/// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
		/// <param name="storedProcedureName">The name of the stored procedure.</param>
		/// <param name="parameters">         A collection of Key-Value pairs.</param>
		/// <returns>An object of type T.</returns>
		public T ExecuteStoredProcedure<T>(string storedProcedureName, ParameterCollection parameters) where T : class, new()
		{
			SetParameterCollectionDbObject(parameters);
			List<T> list = ExecuteStoredProcedureForList<T>(storedProcedureName, parameters);

			return list.Count > 0 ? list[0] : null;
		}

		/// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
		/// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
		/// <param name="storedProcedureName">The name of the stored procedure.</param>
		/// <returns>An List of type T.</returns>
		public List<T> ExecuteStoredProcedureForList<T>(string storedProcedureName) where T : class, new()
		{
			IList<T> list = new List<T>();
			ExecuteStoredProcedureForList(ref list, storedProcedureName, new());
			return (List<T>) list;
		}

		/// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
		/// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
		/// <param name="storedProcedureName">The name of the stored procedure.</param>
		/// <param name="parameters">         A collection of Key-Value pairs.</param>
		/// <returns>An List of type T.</returns>
		public List<T> ExecuteStoredProcedureForList<T>(string storedProcedureName, ParameterCollection parameters) where T : class, new()
		{
			IList<T> list = new List<T>();
			SetParameterCollectionDbObject(parameters);
			ExecuteStoredProcedureForList(ref list, storedProcedureName, parameters);
			return (List<T>) list;
		}

		/// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
		/// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
		/// <param name="list">               A reference to the list in which the items are to be returned.</param>
		/// <param name="storedProcedureName">The name of the stored procedure.</param>
		/// <param name="parameters">         A collection of Key-Value pairs.</param>
		public void ExecuteStoredProcedureForList<T>(ref IList<T> list, string storedProcedureName) where T : class, new()
		{
			IDataReader reader = null;
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				reader = DatabaseObjectInstance.ExecuteReader(storedProcedureName, CommandType.StoredProcedure, new());
				CreateBusinessEntity(ref list, reader);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}
		}

		/// <summary>Executes a stored procedure using the supplied Key-Value pair. Returns a List&lt;&gt;</summary>
		/// <typeparam name="T">The type of object the stored procedure returns.</typeparam>
		/// <param name="list">               A reference to the list in which the items are to be returned.</param>
		/// <param name="storedProcedureName">The name of the stored procedure.</param>
		/// <param name="parameters">         A collection of Key-Value pairs.</param>
		public void ExecuteStoredProcedureForList<T>(ref IList<T> list, string storedProcedureName, ParameterCollection parameters) where T : class, new()
		{
			IDataReader reader = null;
			try
			{
				SetParameterCollectionDbObject(parameters);
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				reader = DatabaseObjectInstance.ExecuteReader(storedProcedureName, CommandType.StoredProcedure, parameters);
				CreateBusinessEntity(ref list, reader);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}
		}

		/// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strSql">    The STR SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public List<T> ExecuteNonQueryForList<T>(string strSql) where T : class, new()
		{
			IList<T> list = new List<T>();
			ExecuteNonQueryForList(ref list, strSql, new());
			return (List<T>) list;
		}

		/// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="strSql">    The STR SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public List<T> ExecuteNonQueryForList<T>(string strSql, ParameterCollection? parameters) where T : class, new()
		{
			IList<T> list = new List<T>();
			if (parameters != null)
				SetParameterCollectionDbObject(parameters);
			ExecuteNonQueryForList(ref list, strSql, parameters);
			return (List<T>) list;
		}

		/// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">      The list.</param>
		/// <param name="strSql">    The STR SQL.</param>
		/// <param name="parameters">The parameters.</param>
		public void ExecuteNonQueryForList<T>(ref IList<T> list, string strSql) where T : class, new()
		{
			IDataReader reader = null;
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				reader = DatabaseObjectInstance.ExecuteReader(strSql, CommandType.Text, new());
				CreateBusinessEntity(ref list, reader, true);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}
		}

		/// <summary>Executes a Non Query and returns a List&lt;&gt;</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">      The list.</param>
		/// <param name="strSql">    The STR SQL.</param>
		/// <param name="parameters">The parameters.</param>
		public void ExecuteNonQueryForList<T>(ref IList<T> list, string strSql, ParameterCollection parameters) where T : class, new()
		{
			IDataReader reader = null;
			try
			{
				if (CommandTimeout > -1)
				{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
				SetParameterCollectionDbObject(parameters);
				reader = DatabaseObjectInstance.ExecuteReader(strSql, CommandType.Text, parameters);
				CreateBusinessEntity(ref list, reader, true);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}
		}

		/// <summary>Executes the SQL scaler.</summary>
		/// <param name="strSQL">The string SQL.</param>
		/// <returns></returns>
		public object ExecuteSQLScaler(string strSQL)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteScalar(strSQL, CommandType.Text, null);
		}

		/// <summary>Executes the SQL scaler.</summary>
		/// <param name="strSQL">    The STR SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public object ExecuteSQLScaler(string strSQL, CommandType commandType)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteScalar(strSQL, commandType, new ParameterCollection());
		}

		/// <summary>Executes the SQL scaler.</summary>
		/// <param name="strSQL">    The STR SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public object ExecuteSQLScaler(string strSQL, CommandType commandType, ParameterCollection parameters)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			SetParameterCollectionDbObject(parameters);
			return DatabaseObjectInstance.ExecuteScalar(strSQL, commandType, parameters);
		}

		/// <summary>Executes the non query.</summary>
		/// <param name="strSQL">The string SQL.</param>
		public int ExecuteNonQuery(string strSQL)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteNonQuery(strSQL, CommandType.Text);
		}

		/// <summary>Executes the non-query.</summary>
		/// <param name="strSQL">     The STR SQL.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <param name="parameters"> The parameters.</param>
		public int ExecuteNonQuery(string strSQL, CommandType commandType)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteNonQuery(strSQL, commandType);
		}

		/// <summary>Executes the non-query.</summary>
		/// <param name="strSQL">     The STR SQL.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <param name="parameters"> The parameters.</param>
		public int ExecuteNonQuery(string strSQL, CommandType commandType, ParameterCollection parameters)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			SetParameterCollectionDbObject(parameters);
			return DatabaseObjectInstance.ExecuteNonQuery(strSQL, commandType, parameters);
		}

		/// <summary>Executes the query and returns as a DataTable.</summary>
		/// <param name="strSQL">The STR SQL.</param>
		/// <returns></returns>
		public DataTable ExecuteDataTable(string strSQL)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteDataTable(strSQL);
		}

		/// <summary>Executes the query and returns as a DataTable.</summary>
		/// <param name="strSQL">The STR SQL.</param>
		/// <returns></returns>
		public DataTable ExecuteDataTable(string strSQL, CommandType commandType)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteDataTable(strSQL);
		}

		/// <summary>Executes the query and returns as a DataSet.</summary>
		/// <param name="strSQL">The STR SQL.</param>
		/// <returns></returns>
		public DataSet ExecuteDataSet(string strSQL)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteDataSet(strSQL);
		}

		/// <summary>Executes the query and returns as a DataSet.</summary>
		/// <param name="strSQL">     The STR SQL.</param>
		/// <param name="commandType">Type of the command.</param>
		/// <returns></returns>
		public DataSet ExecuteDataSet(string strSQL, CommandType commandType)
		{
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
			return DatabaseObjectInstance.ExecuteDataSet(strSQL, commandType);
		}

		/// <summary>Executes the query and returns as a DataSet.</summary>
		/// <param name="strSQL">The STR SQL.</param>
		/// <param name="cmd">   <see cref="Microsoft.Data.CommandType" /></param>
		/// <param name="param"> The param.</param>
		/// <returns></returns>
		public DataSet ExecuteDataSet(string strSQL, CommandType commandType, ParameterCollection? param)
		{
			param.DerivedDatabaseObject = DatabaseObjectInstance as DatabaseObject;
			if (CommandTimeout > -1)
			{ DatabaseObjectInstance.CommandTimeout = CommandTimeout; }
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
		public bool ExecuteBulkCopy(DataTable records, int notificationRecordInterval, string? destinationTable)
		{
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
		public void Transaction(Action workToDo)
		{
			try
			{
				BeginTransaction();
				workToDo();
			}
			catch (Exception ex)
			{
				Rollback();
				throw ex;
			}
			finally
			{
				Commit();
			}
		}

		/// <summary>Commits the operation.</summary>
		public void CommitOperation()
		{
			if (TransactionInProgress)
			{
				TransactionInProgress = false;
				DatabaseObjectInstance.CommitTransaction();
			}
		}

		/// <summary>Begins the transaction.</summary>
		/// <returns></returns>
		public IDbTransaction BeginTransaction()
		{
			TransactionInProgress = true;
			return DatabaseObjectInstance.BeginTransaction();
		}

		/// <summary>Commits a transaction inside of the <see cref="Transaction" /> method.</summary>
		public void Commit()
		{
			if (TransactionInProgress)
			{
				TransactionInProgress = false;
				DatabaseObjectInstance.CommitTransaction();
			}
		}

		/// <summary>Tests the connection.</summary>
		/// <returns></returns>
		public Boolean TestConnection() => DatabaseObjectInstance.TestConnection();

		/// <summary>Rollback a transaction inside of the <see cref="Transaction" /> method.</summary>
		public void Rollback()
		{
			if (TransactionInProgress)
			{
				TransactionInProgress = false;
				DatabaseObjectInstance.RollbackTransaction();
			}
		}

		/// <summary>Gets the table name from object.</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public String GetTableNameFromObject<T>()
		{
			DataTableAttribute table = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			if (String.IsNullOrEmpty(table.SchemaName))
				if (!String.IsNullOrEmpty(DefaultSchemaName))
					return $"{DefaultSchemaName}.{table.TableName}";

			return table.TableName;
		}

		public string GetTableNameFromObject(string typeName)
		{
			DataTableAttribute table = (DataTableAttribute) ORM[typeName][Type.GetType(typeName)];
			if (String.IsNullOrEmpty(table.SchemaName))
				if (!String.IsNullOrEmpty(DefaultSchemaName))
					return $"{DefaultSchemaName}.{table.TableName}";

			return table.TableName;
		}

		/// <summary>Gets the view name from object type.</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public String GetViewNameFromObject<T>()
		{
			DataTableAttribute table = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			if (table.ViewName == String.Empty)
			{ return GetTableNameFromObject<T>(); }
			if (String.IsNullOrEmpty(table.SchemaName))
				if (!String.IsNullOrEmpty(DefaultSchemaName))
					return $"{DefaultSchemaName}.{table.TableName}";

			return table.ViewName;
		}

		/// <summary>Gets the view name from the object's name.</summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public string GetViewNameFromObject(string typeName)
		{
			DataTableAttribute table = (DataTableAttribute) ORM[typeName][Type.GetType(typeName)];
			if (table.ViewName == String.Empty)
			{ return GetTableNameFromObject(typeName); }
			if (String.IsNullOrEmpty(table.SchemaName))
				if (!String.IsNullOrEmpty(DefaultSchemaName))
					return $"{DefaultSchemaName}.{table.TableName}";
			return table.ViewName;
		}

		/// <summary>Gets the source name from object.</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public String GetSourceNameFromObject<T>()
		{
			DataTableAttribute table = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			if (String.IsNullOrEmpty(table.SchemaName))
				if (!String.IsNullOrEmpty(DefaultSchemaName))
					return $"{DefaultSchemaName}.{table.SourceName}";

			return table.SourceName;
		}

		/// <summary>Gets the source name from object.</summary>
		/// <param name="typeName">Name of the type.</param>
		/// <returns></returns>
		public string GetSourceNameFromObject(string typeName)
		{
			DataTableAttribute table = (DataTableAttribute) ORM[typeName][Type.GetType(typeName)];
			if (String.IsNullOrEmpty(table.SchemaName))
				if (!String.IsNullOrEmpty(DefaultSchemaName))
					return $"{DefaultSchemaName}.{table.SourceName}";

			return table.SourceName;
		}

		/// <summary>Gets the column name from the passed property name.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public String GetColumnNameFromProperty<T>(string propertyName)
		{
			String columName = String.Empty;
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.PropertyName == propertyName)
				{
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
		public String GetPropertyNameColumn<T>(string columnName)
		{
			String propertyName = String.Empty;
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.ColumnName == columnName)
				{
					propertyName = attrColumn.PropertyName;
					break;
				}
			}
			return propertyName;
		}

		/// <summary>Gets the column data attribute.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public DataColumnAttribute GetColumnDataAttribute<T>(string propertyName)
		{
			DataColumnAttribute propAttr = null;
			object[] arrColumnAttributes;

			foreach (PropertyInfo pi in typeof(T).GetProperties())
			{
				if (pi.Name == propertyName)
				{
					arrColumnAttributes = pi.GetCustomAttributes(typeof(DataColumnAttribute), true);
					if (arrColumnAttributes.Length > 0)
					{
						propAttr = (DataColumnAttribute) arrColumnAttributes[0];
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
		public List<TableColumn> GetTableDefinition<T>(Boolean includeIdentityColumns)
		{
			try
			{
				List<TableColumn> tableDefinition = DatabaseObjectInstance.GetTableDefinition(GetTableNameFromObject<T>());

				foreach (TableColumn col in tableDefinition)
				{
					col.correspondingPropertyName = GetPropertyNameColumn<T>(col.name);
				}
				return !includeIdentityColumns
					? tableDefinition.FindAll(delegate (TableColumn c)
					{ return c.isIdentity == false; })
					: tableDefinition;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>Determines whether the property is key to the object.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns><c>true</c> if [is property key] [the specified property name]; otherwise, <c>false</c>.</returns>
		public bool IsPropertyKey<T>(string propertyName)
		{
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.PropertyName == propertyName)
				{
					return attrColumn.IsPrimaryKey;
				}
			}
			return false;
		}

		/// <summary>Gets the <see cref="DataColumnAttribute" /> marked with the primary key flag.</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Obsolete("This method is obsolete due to the naming.  Please use GetPrimaryKeyAttribute().")]
		public DataColumnAttribute GetKeyPropertyName<T>()
		{
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.IsPrimaryKey)
				{
					return attrColumn;
				}
			}
			return new DataColumnAttribute("", DbType.String);
		}

		/// <summary>Gets the <see cref="DataColumnAttribute" /> marked with the primary key flag.</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public DataColumnAttribute GetPrimaryKeyAttribute<T>()
		{
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.IsPrimaryKey)
				{
					return attrColumn;
				}
			}
			return new DataColumnAttribute("", DbType.String);
		}

		/// <summary>Makes a parameter from the supplied object property with the given value</summary>
		/// <typeparam name="T">The type of object from which to make the parameter.</typeparam>
		/// <param name="propertyName">The property of the object from which to make the parameter.</param>
		/// <param name="value">       The value to which the parameter should be set.</param>
		/// <returns></returns>
		public IDataParameter MakeParameter<T>(string propertyName, object value)
		{
			//TODO: determine if this function needs to return null
			IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.PropertyName == propertyName)
				{
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
		public IDataParameter MakeParameter<T>(string propertyName, T obj)
		{
			//TODO: determine if this function needs to return null
			IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.PropertyName == propertyName)
				{
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
		public Parameter MakeParameter<T, U>(string propertyName, object value, WhereType whereType = WhereType.AND) where U : Parameter, new()
		{
			// Convert the parameter to the type that was requested.
			U param = new()
			{
				BaseParameter = DatabaseObjectInstance.CreateEmptyParameter()
			};
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				if (attrColumn.PropertyName == propertyName)
				{
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

		/// <summary>Make a parameter from the supplied data.</summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="parameterType">The DbType of the parameter.</param>
		/// <param name="paramterValue">The value of the parameter.</param>
		/// <returns></returns>
		public IDataParameter MakeParameter(string parameterName, DbType parameterType, object paramterValue)
		{
			//TODO: determine if this function needs to return null
			IDataParameter param = DatabaseObjectInstance.CreateEmptyParameter();
			param.DbType = parameterType;
			param.Direction = ParameterDirection.Input;
			param.ParameterName = parameterName;
			param.SourceColumn = parameterName;
			param.Value = paramterValue;

			return param;
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>Gets the column names from the passed data object.</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public List<String> GetColumnNames<T>()
		{
			List<string> listColumns = new();
			foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				listColumns.Add(attrColumn.ColumnName);
			}
			return listColumns;
		}

		public List<string> GetApiParameters<T>()
		{
			List<string> listParameters = new();
			foreach (ApiDataParameterAttribute attrParam in ((ApiDataControllerAttribute) ORM[typeof(T).FullName][typeof(T)]).Properties)
			{
				listParameters.Add(attrParam.ParameterName);
			}
			return listParameters;
		}

		protected List<String> GetOrderByColumnNames<T>()
		{
			List<string> listColumns = new();
			//not supported at the moment
			//foreach (DataColumnAttribute attrColumn in ORM[typeof(T).FullName][typeof(T)].Properties)
			//{
			//	listColumns.Add(attrColumn.Name);
			//}
			return listColumns;
		}

		protected List<T> CreateBusinessEntity<T>(IDataReader reader, bool useOrdinal = false) where T : class, new()
		{
			IList<T> list = new List<T>();
			CreateBusinessEntity(ref list, reader, useOrdinal);
			return (List<T>) list;
		}

		protected List<T> CreateBusinessEntity<T>(DataTable dt, bool useOrdinal = false) where T : class, new()
		{
			IList<T> list = new List<T>();
			CreateBusinessEntity(ref list, dt, useOrdinal);
			return (List<T>) list;
		}

		protected void CreateBusinessEntity<T>(ref IList<T> list, IDataReader reader) where T : class, new()
		{
			T entity;
			DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			while (reader.Read())
			{
				entity = new T();
				for (int i = 0; i < reader.FieldCount; i++)
				{
					foreach (DataColumnAttribute attrColumn in attrTable.Properties)
					{
						if (attrColumn.ColumnName.ToLower() == reader.GetName(i).ToLower())
						{
							//extended object defined:
							// - create an instance of the extended object (must have a parameterless constructor
							// - set the extended object's value with the data from the database
							// - set the mapped property value to the etended object
							if (attrColumn.ExtendedPropertyType != null)
							{
								object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

								//type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
								if (extendedObj.GetType().IsEnum)
								{
									PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
									object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
									Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
									object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
									pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int) enumValue), null);
								}
								else
								{
									PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
									object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
									pi.SetValue(extendedObj, objValue, null);
								}
							}
							else
							{//normal property - set it directly
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

		protected void CreateBusinessEntity<T>(ref IList<T> list, DataTable dt) where T : class, new()
		{
			T entity;
			DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
			foreach (DataRow row in dt.Rows)
			{
				entity = new T();
				for (int i = 0; i < dt.Columns.Count; i++)
				{
					foreach (DataColumnAttribute attrColumn in attrTable.Properties)
					{
						if (attrColumn.ColumnName.ToLower() == dt.Columns[i].ColumnName.ToLower())
						{
							//extended object defined:
							// - create an instance of the extended object (must have a parameterless constructor
							// - set the extended object's value with the data from the database
							// - set the mapped property value to the etended object
							if (attrColumn.ExtendedPropertyType != null)
							{
								object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

								//type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
								if (extendedObj.GetType().IsEnum)
								{
									PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
									object objValue = GetReaderValueAs(row[i], pi.PropertyType);
									Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
									object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
									pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int) enumValue), null);
								}
								else
								{
									PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
									object objValue = GetReaderValueAs(row[i], pi.PropertyType);
									pi.SetValue(extendedObj, objValue, null);
								}
							}
							else
							{//normal property - set it directly
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
		protected void CreateBusinessEntity<T>(ref IList<T> list, IDataReader reader, bool useOrdinal = true) where T : class, new()
		{
			if (!useOrdinal)
			{
				CreateBusinessEntity(ref list, reader);
			}
			else
			{
				T entity;
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				while (reader.Read())
				{
					entity = new T();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						DataColumnAttribute attrColumn = attrTable.Properties.SingleOrDefault(p => p.ColumnName.ToLower() == reader.GetName(i).ToLower());
						if (attrColumn != null)
						{
							//extended object defined:
							// - create an instance of the extended object (must have a parameterless constructor
							// - set the extended object's value with the data from the database
							// - set the mapped property value to the etended object
							if (attrColumn.ExtendedPropertyType != null)
							{
								object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

								//type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
								if (extendedObj.GetType().IsEnum)
								{
									PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
									object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
									Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
									object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
									pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int) enumValue), null);
								}
								else
								{
									PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
									object objValue = GetReaderValueAs(reader.GetValue(i), pi.PropertyType);
									pi.SetValue(extendedObj, objValue, null);
								}
							}
							else
							{//normal property - set it directly
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

		protected void CreateBusinessEntity<T>(ref IList<T> list, DataTable dt, bool useOrdinal = true) where T : class, new()
		{
			if (!useOrdinal)
			{
				CreateBusinessEntity(ref list, dt);
			}
			else
			{
				T entity;
				DataTableAttribute attrTable = (DataTableAttribute) ORM[typeof(T).FullName][typeof(T)];
				foreach (DataRow row in dt.Rows)
				{
					entity = new T();
					for (int i = 0; i < dt.Columns.Count; i++)
					{
						DataColumnAttribute attrColumn = attrTable.Properties.SingleOrDefault(p => p.ColumnName.ToLower() == dt.Columns[i].ColumnName.ToLower());
						if (attrColumn != null)
						{
							//extended object defined:
							// - create an instance of the extended object (must have a parameterless constructor
							// - set the extended object's value with the data from the database
							// - set the mapped property value to the etended object
							if (attrColumn.ExtendedPropertyType != null)
							{
								object extendedObj = Activator.CreateInstance(attrColumn.ExtendedPropertyType);

								//type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
								if (extendedObj.GetType().IsEnum)
								{
									PropertyInfo pi = typeof(T).GetProperty(attrColumn.PropertyName);
									object objValue = GetReaderValueAs(row[i], pi.PropertyType);
									Type enumUnderlyingType = System.Enum.GetUnderlyingType(extendedObj.GetType());
									object enumValue = System.Convert.ChangeType(objValue, enumUnderlyingType);
									pi.SetValue(entity, Enum.ToObject(pi.PropertyType, (int) enumValue), null);
								}
								else
								{
									PropertyInfo pi = attrColumn.ExtendedPropertyType.GetProperty(attrColumn.ExtendedPropertyName);
									object objValue = GetReaderValueAs(row[i], pi.PropertyType);
									pi.SetValue(extendedObj, objValue, null);
								}
							}
							else
							{//normal property - set it directly
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
		public List<T> ConvertDataTableToList<T>(DataTable dt) where T : class, new()
		{
			try
			{
				List<T> list = new();
				return CreateBusinessEntity<T>(dt);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>Gets the reader value as [defined data type].</summary>
		/// <param name="value">       The value.</param>
		/// <param name="expectedType">The expected type.</param>
		/// <returns></returns>
		protected object GetReaderValueAs(object value, Type expectedType)
		{
			if (value != DBNull.Value)
			{
				return expectedType.GetType() == typeof(Enum)
					? value
					: expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Nullable<>)
					? ((DateTime?) value) ?? GetTypeMinValue(expectedType)
					: expectedType.Name switch
					{
						"Boolean" => Convert.ToBoolean(value),
						_ => value,
					};
			}
			else
			{
				if (expectedType.IsPrimitive
					|| expectedType.Equals(typeof(string))
					|| expectedType.Equals(typeof(bool)))
				{
					object? returnVal = null;
					Switch.On(expectedType)
						.Case(typeof(string), () => returnVal = String.Empty)
						.Case(typeof(String), () => returnVal = String.Empty)
						.Case(typeof(Char), () => returnVal = String.Empty)
						.Case(typeof(Boolean), () => returnVal = false)
						.Case(typeof(bool), () => returnVal = false)
						.Default(() => returnVal = null);

					return returnVal;
				}
				else
				{
					return GetTypeMinValue(expectedType);
				}
			}
		}

		/// <summary>Formats the value for null based on the determined type of the value.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected object FormatValueForNull(object value) => FormatValueForNull(value, value.GetType());

		/// <summary>Formats the value for null based on the value type passed.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected object FormatValueForNull(object value, Type valueType)
		{
			object returnVal = null;
			try
			{
				CodedThought.Core.Switch.On(valueType)
					.Case(typeof(Int16), () => returnVal = (Int16.MinValue == (Int16) value ? DBNull.Value : value))
					.Case(typeof(Int32), () => returnVal = (Int32.MinValue == (Int32) value ? DBNull.Value : value))
					.Case(typeof(Int64), () => returnVal = (Int64.MinValue == (Int64) value ? DBNull.Value : value))
					.Case(typeof(Boolean), () => returnVal = value)
					.Case(typeof(float), () => returnVal = (float.MinValue == (float) value ? DBNull.Value : value))
					.Case(typeof(Double), () => returnVal = (Double.MinValue == (Double) value ? DBNull.Value : value))
					.Case(typeof(Decimal), () => returnVal = (Decimal.MinValue == (Decimal) value ? DBNull.Value : value))
					.Case(typeof(String), () => returnVal = (String.IsNullOrEmpty((string) value) ? DBNull.Value : value))
					.Case(typeof(Char), () => returnVal = (String.IsNullOrEmpty((string) value) ? DBNull.Value : value))
					.Case(typeof(DateTime), () => returnVal = (DateTime.MinValue == ConvertToSafeDateTime(value) ? DBNull.Value : Convert.ToDateTime(value)))
					.Case(typeof(DateTime?), () => returnVal = (DateTime.MinValue == ConvertToSafeDateTime(value) ? DBNull.Value : Convert.ToDateTime(value)))
					.Case(typeof(Object), () => returnVal = (value ?? DBNull.Value))
					.Case(typeof(byte[]), () => returnVal = (value == null || ((byte[]) value).Length == 0 ? DBNull.Value : value));
				return returnVal;
			}
			catch (CodedThoughtException ex)
			{
				throw ex;
			}
		}

		protected DateTime ConvertToSafeDateTime(object value)
		{
			DateTime.TryParse(value.ToString(), out DateTime dateTime);
			return dateTime;
		}

		protected void SetParameterCollectionDbObject(ParameterCollection param)
		{
			if (param != null)
			{
				param.DerivedDatabaseObject ??= DatabaseObjectInstance as DatabaseObject;
			}
		}

		#endregion Private Methods

		#region Static Methods

		/// <summary>Gets the type minimum value.</summary>
		/// <param name="expectedType">The expected type.</param>
		/// <returns></returns>
		protected static object GetTypeMinValue(Type expectedType)
		{
			object returnVal = null;
			CodedThought.Core.Switch.On(expectedType)
				.Case(typeof(Int16), () => returnVal = Int16.MinValue)
				.Case(typeof(Int32), () => returnVal = Int32.MinValue)
				.Case(typeof(Int64), () => returnVal = Int64.MinValue)
				.Case(typeof(float), () => returnVal = float.MinValue)
				.Case(typeof(Double), () => returnVal = Double.MinValue)
				.Case(typeof(Decimal), () => returnVal = Decimal.MinValue)
				.Case(typeof(String), () => returnVal = String.Empty)
				.Case(typeof(Char), () => returnVal = String.Empty)
				.Case(typeof(DateTime), () => returnVal = DateTime.MinValue)
				.Case(typeof(DateTime?), () => returnVal = DateTime.MinValue)
				.Case(typeof(Object), () => returnVal = null);
			return returnVal;
		}

		#endregion Static Methods

		#region Class Mapping

		/// <summary>
		/// Loops through all types of the registed assemblies (registered via the
		/// DataTableAttribute) and generates a DictionaryCollection of each type and its DB mappings
		/// </summary>
		/// <returns></returns>
		protected void GenerateMapCollection(Dictionary<string, Dictionary<Type, Attribute>> mapCollection, Assembly assembly)
		{
			mapCollection ??= new Dictionary<string, Dictionary<Type, Attribute>>();

			foreach (Type type in assembly.GetTypes())
			{
				if (mapCollection.ContainsKey(type.FullName) || mapCollection.ContainsKey($"{type.FullName}.api"))
				{
					continue;
				}

				DataTableAttribute attrTable = type.GetDataTableAttribute(false);
				if (attrTable != null)
				{
					DataTableUsageAttribute attrUsage = type.GetDataTableUsageAttribute(attrTable.IgnoreInherited);
					attrUsage ??= new DataTableUsageAttribute();
					if (mapCollection.ContainsKey(type.FullName))
						throw new CodedThoughtException($"The type, {type.FullName}, already exists in the mapping.");
					attrTable.ClassType = type;
					attrTable.ClassName = type.Name;
					attrTable.ReadOnly = attrUsage.UseAs.HasFlag(DataTableUsage.ReadOnly);
					attrTable.IgnoreInherited = attrUsage.UseAs.HasFlag(DataTableUsage.IgnoreInherited);
					attrTable.UseView = attrUsage.UseAs.HasFlag(DataTableUsage.ViewPriority);
					MapTableColumns(type, attrTable);
					Dictionary<Type, Attribute> dbDic = new() {
						{ type, attrTable }
					};
					mapCollection.Add(type.FullName, dbDic);
				}

				// Look for API Attributes
				ApiDataControllerAttribute paramTable = type.GetApiDataControllerAttribute(false);
				if (paramTable != null)
				{
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
		protected void MapTableColumns(Type type, DataTableAttribute attrTable)
		{
			List<PropertyInfo> properties = !attrTable.IgnoreInherited
				? type.GetProperties().ToList()
				: type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
			try
			{
				foreach (PropertyInfo pi in properties)
				{
					DataColumnAttribute attr = pi.GetDataColumnAttributes(false);
					if (attr != null)
					{
						// Ignore properties not set up with the DataColumn Attribute.
						attr.PropertyType = pi.PropertyType;
						attr.PropertyName = pi.Name;
						attrTable.Properties.Add(attr);
					}
				}
			}
			catch { throw; }
		}

		/// <summary>Finds and loads all <see cref="ApiDataParameterAttribute" /> attributes into the passed <see cref="ApiDataControllerAttribute" /> attribute.</summary>
		/// <param name="type">   </param>
		/// <param name="attrApi"></param>
		protected void MapApiColumns(Type type, ApiDataControllerAttribute attrApi)
		{
			List<PropertyInfo> properties = type.GetProperties().ToList();
			try
			{
				foreach (PropertyInfo pi in properties)
				{
					ApiDataParameterAttribute param = pi.GetApiDataParametersAttributes(false);
					if (param != null)
					{
						if (attrApi.Properties == null)
							attrApi.Properties = new();
						// Determine if this attribute should override any others.
						if (attrApi.Properties.Count(p => p.ParameterName.ToLower() == param.ParameterName.ToLower()) > 0 && param.Options.HasFlag(ApiDataParameterOptions.OverridesInherited))
						{
							attrApi.Properties.ToList().ForEach(a =>
							{
								if (a.ParameterName.ToLower() == param.ParameterName.ToLower())
								{
									if (!a.Options.HasFlag(ApiDataParameterOptions.OverridesInherited))
									{
										attrApi.Properties.Remove(a);
									}
								}
								else
								{
									attrApi.Properties.Add(param);
								}
							});
						}
						else
						{
							attrApi.Properties.Add(param);
						}
					}
				}
			}
			catch { throw; }
		}
		private List<Assembly> GetDataAwareAssemblies()
		{
			try
			{
				List<Assembly> dataAwareAssemblies = [];
				List<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
				foreach (Assembly assembly in allAssemblies)
				{
					if (Attribute.GetCustomAttribute(assembly, typeof(DataAwareAssemblyAttribute)) is DataAwareAssemblyAttribute isDataAware)
					{ dataAwareAssemblies.Add(assembly); }
				}
				return dataAwareAssemblies;
			}
			catch { throw; }
		}
		#endregion Class Mapping

		#region IDBStore Members

		object IDBStore.Extract(object businessEntity, string columnName)
		{
			Type t = businessEntity.GetType();
			if (ORM.ContainsKey(t.FullName))
			{
				foreach (DataColumnAttribute attrColumn in ((DataTableAttribute) ORM[t.FullName][t]).Properties)
				{
					if (attrColumn.ColumnName == columnName)
					{
						//original function body
						//return t.GetProperty(attrColumn.PropertyName).GetValue(businessEntity, null);

						if (attrColumn.ExtendedPropertyType != null)
						{
							//the property is an extended object, so get the object first, then get the extended property from the extended object
							object oExtendedObject = t.GetProperty(attrColumn.PropertyName).GetValue(businessEntity, null);
							t = oExtendedObject.GetType();
							return t.GetProperty(attrColumn.ExtendedPropertyName).GetValue(oExtendedObject, null);
						}
						else
						{
							return t.GetProperty(attrColumn.PropertyName).GetValue(businessEntity, null);
						}
					}
				}
			}
			return null;
		}

		int IDBStore.GetPrimaryKey(object obj)
		{
			Type t = obj.GetType();
			return ORM.ContainsKey(t.FullName)
				? ((DataTableAttribute) ORM[t.FullName][t]).Key.ColumnType switch
				{
					DbType.Int16 or DbType.Int32 or DbType.Int64 or DbType.Decimal => Convert.ToInt32(t.GetProperty(((DataTableAttribute) ORM[t.FullName][t]).Key.PropertyName).GetValue(obj, null)),
					_ => 0,
				}
				: 0;
		}

		string IDBStore.GetPrimaryKeyName(object obj)
		{
			Type t = obj.GetType();
			return ORM.ContainsKey(t.FullName) ? ((DataTableAttribute) ORM[t.FullName][t]).Key.ColumnName : String.Empty;
		}

		bool IDBStore.HasKeyColumn(object obj)
		{
			Type t = obj.GetType();
			return ORM.ContainsKey(t.FullName) && ((DataTableAttribute) ORM[t.FullName][t]).Key != null;
		}

		bool IDBStore.SetPrimaryKey(object obj, int value)
		{
			Type t = obj.GetType();
			if (ORM.ContainsKey(t.FullName))
			{
				t.GetProperty(((DataTableAttribute) ORM[t.FullName][t]).Key.PropertyName).SetValue(obj, value, null);
				return true;
			}
			else
				return false;
		}

		#endregion IDBStore Members
	}
}