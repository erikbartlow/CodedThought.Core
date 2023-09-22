namespace CodedThought.Core.Data {

	/// <summary>Maps a class to a Database Table or XML Element</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class DataTableAttribute : Attribute {

		#region Declarations

		private string _tableName;
		private string _viewName;

		#endregion Declarations

		#region Properties

		public string TableName {
			get {
				if (String.IsNullOrEmpty(_tableName) && (!UseView && String.IsNullOrEmpty(_viewName))) {
					throw new HPMissingArguementException("The table name is not set for this object.");
				} else {
					string tblName = _tableName;
					return SchemaName != string.Empty ? $"[{SchemaName}].[{tblName}]" : $"[{tblName}]";
				}
			}
			set {
				_tableName = value;
			}
		}

		/// <summary>Gets or sets the name of the view.</summary>
		/// <value>If a view is set then it will be used during all Get methods.</value>
		/// <remarks>
		/// This is typically used when custom queries are needed to occur before the results are returned to the object for instantiation.
		/// Important:  The column names must match those in the table. To force the framework to use the view name see the <see cref="UseView" /> property.
		/// </remarks>
		public string ViewName {
			get {
				if (String.IsNullOrEmpty(TableName) && String.IsNullOrEmpty(_viewName)) {
					throw new HPMissingArguementException("The view name is not set.  If the table name is not set then a view name must be set.");
				} else {
					string tblName = _viewName;
					return String.IsNullOrEmpty(tblName) ? string.Empty : SchemaName != string.Empty ? $"[{SchemaName}].[{tblName}]" : $"[{tblName}]";
				}
			}
			set {
				_viewName = value;
			}
		}

		/// <summary>Gets or set the schema name that the table or view belongs in.  This will override the schema name specific in the connection or DefaultSchemaName property in the DatabaseObject.</summary>
		public string SchemaName { get; set; }

		/// <summary>Gets the name of the source based on the <see cref="UseView" /> property and availability of the table and view name properties.</summary>
		/// <value>The name of the source.</value>
		/// <exception cref="HPMissingArguementException">An exception will be thrown if both the table and view name are empty. At least one is required.</exception>
		public string SourceName {
			get {
				string source = (UseView ? ViewName : TableName);
				return String.IsNullOrEmpty(source)
					? throw new HPMissingArguementException("The source name for the data object is not set in either the table or view names.")
					: source;
			}
		}

		public BindingList<DataColumnAttribute> Properties { get; set; }

		public DataColumnAttribute Key { get; set; }

		public Type ClassType { get; set; }

		public string ClassName { get; set; }

		/// <summary>Gets or sets a value indicating whether to include inherited properties.</summary>
		/// <value><c>true</c> if [include inherited properties]; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// If an inherited column is causing a conflict you can set this for the entire table using the <see cref="DataTableUsageAttribute" /> attribute and the <see
		/// cref="DataTableUsage.IgnoreInherited" /> enum flag. If only one column is the issue then use the <see cref="DataColumnAttribute.OverridesInherited" /> property of the <see
		/// cref="DataColumnAttribute" /> attribute.
		/// </remarks>
		public bool IgnoreInherited { get; set; }

		/// <summary>Gets or sets a value indicating whether this instance isread only.</summary>
		/// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
		/// <remarks>This is typically set with the <see cref="DataTableUsageAttribute" /> attribute and the <see cref="DataTableUsage.ReadOnly" /> enum flag.</remarks>
		public bool ReadOnly { get; set; }

		/// <summary>Gets or sets a value indicating whether to use a view instead of the table.</summary>
		/// <value><c>true</c> if [use view]; otherwise, <c>false</c>.</value>
		/// <remarks>This is typically set with the <see cref="DataTableUsageAttribute" /> attribute and the <see cref="DataTableUsage.ViewPriority" /> enum flag.</remarks>
		public bool UseView { get; set; }

		#endregion Properties

		#region Constructors

		private DataTableAttribute() {
			TableName = string.Empty;
			ClassName = string.Empty;
			ViewName = string.Empty;
			SchemaName = string.Empty;
			Properties = new BindingList<DataColumnAttribute>();
			Properties.AddingNew += new AddingNewEventHandler(_properties_ColumnAdded);
			Properties.ListChanged += new ListChangedEventHandler(_properties_ListChanged);
			Properties.RaiseListChangedEvents = true;
			IgnoreInherited = true;
			ReadOnly = false;
			UseView = false;
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableAttribute" /> class.</summary>
		/// <param name="containerName">Name of the container.</param>
		public DataTableAttribute(string containerName)
			: this() {
			TableName = containerName;
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableAttribute" /> class.</summary>
		/// <param name="containerName">Name of the container.</param>
		/// <param name="useView">      if set to <c>true</c> then container will be set as a view.</param>
		public DataTableAttribute(string containerName, bool useView = true)
			: this() {
			UseView = useView;
			if (UseView) {
				_viewName = containerName;
				ReadOnly = true;
			} else {
				_tableName = containerName;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableAttribute" /> class.</summary>
		/// <param name="containerName">Name of the container.</param>
		/// <param name="viewName">     Name of the view container.</param>
		/// <remarks>If a view is set then it will be used during all Get methods. All columns in the view must match those in the DataColumn attributes of the data entity.</remarks>
		public DataTableAttribute(string containerName, string viewName)
			: this(containerName) {
			_tableName = containerName;
			_viewName = viewName;
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableAttribute" /> class.</summary>
		/// <param name="containerName">Name of the container.</param>
		/// <param name="viewName">     Name of the view.</param>
		/// <param name="useView">      if set to <c>true</c> then container will be set as a view.</param>
		public DataTableAttribute(string containerName, string viewName, bool useView = false) : this(containerName, useView) {
			_viewName = viewName;
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableAttribute" /> class with a specific schema name.</summary>
		/// <param name="containerName"></param>
		/// <param name="viewName">     </param>
		/// <param name="schemaName">   </param>
		public DataTableAttribute(string containerName, string viewName, string schemaName) : this(containerName, viewName) {
			SchemaName = schemaName;
		}

		#endregion Constructors

		#region Instance Methods

		private void _properties_ListChanged(object sender, ListChangedEventArgs e) {
			if (e.ListChangedType == ListChangedType.ItemAdded) {
				// Check if this new column should be overridden by a previously added column.
				for (int i = 0; i <= Properties.Count - 1; i++) {
					if (Properties[i].ColumnName == Properties[e.NewIndex].ColumnName && i != e.NewIndex) {
						if (Properties[i].OverridesInherited) {
							// There is already column with the same name set to override inherited attributes. So remove this newly added one.
							Properties.RemoveAt(e.NewIndex);
						}
					}
				}

				// Verify only one key is set.
				if (Properties[e.NewIndex].IsPrimaryKey) {
					if (Key == null) {
						Key = Properties[e.NewIndex];
					} else {
						if (Key.OverridesInherited == false) {
							// Override the current key with the one.
							Key.IsPrimaryKey = false;
							Key.Options = Key.Options & ~DataColumnOptions.PrimaryKey;
							Key = Properties[e.NewIndex];
						}
					}
				}
			}
		}

		private void _properties_ColumnAdded(object sender, AddingNewEventArgs e) {
			// Check if this new column should be overridden by a previously added column.
			for (int i = 0; i <= Properties.Count - 1; i++) {
				if (Properties[i].ColumnName == ((DataColumnAttribute)e.NewObject).ColumnName) {
					if (!Properties[i].OverridesInherited) {
						// There is already column with the same name set to override inherited attributes. So remove this newly added one.
						e.NewObject = null;
					}
				}
			}
		}

		#endregion Instance Methods
	}
}