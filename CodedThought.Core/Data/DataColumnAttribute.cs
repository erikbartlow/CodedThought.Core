namespace CodedThought.Core.Data {

	[Flags]
	public enum DataColumnOptions {

		/// <summary>Informs the framework that the column is to be used as the primary key during database operations.</summary>
		PrimaryKey = 1,

		/// <summary>Informs the framework that the column is updateable. Note: This flag cannot be used in conjunction with the IsPrimaryKey flag.</summary>
		UpdateablePrimaryKey = 2,

		/// <summary>Causes the framework to load object described in the extended property. The extended property column must resolve to that object's key property.</summary>
		LoadExtendedProperty = 4,

		/// <summary>Causes the framework to establish this attribute in the ORM over any inherited attributes for this same property.</summary>
		OverridesInherited = 8,

		/// <summary>Causes the framework to know whether or not to allow null values based on the database struture.</summary>
		AllowNull = 16
	}

	/// <summary>Maps a property to a Database Column or XML Element</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DataColumnAttribute : Attribute {

		#region Declarations

		public enum UpdateableKey {
			NonUpdateableKey,
			UpdateableKey
		}

		#endregion Declarations

		#region Properties

		public string ColumnName { get; set; }

		public bool IsPrimaryKey { get; set; }

		public bool IsUpdateable { get; set; }

		public int Size { get; set; }

		public DbType ColumnType { get; set; }

		public Type PropertyType { get; set; }

		public string PropertyName { get; set; }

		public Type ExtendedPropertyType { get; set; }

		public string ExtendedPropertyName { get; set; }

		public bool LoadExtendedPropertyObject { get; set; }

		public bool OverridesInherited { get; set; }

		public bool AllowNulls { get; set; }

		public DataColumnOptions Options { get; set; }

		#endregion Properties

		#region Constructors

		private DataColumnAttribute() {
			ColumnName = string.Empty;
			ColumnType = DbType.Object;
			PropertyType = null;
			PropertyName = string.Empty;
			ExtendedPropertyName = string.Empty;
			ExtendedPropertyType = null;
			Size = 0;
			IsPrimaryKey = false;
			IsUpdateable = true;
			OverridesInherited = false;
			AllowNulls = true;
			LoadExtendedPropertyObject = false;
		}

		public DataColumnAttribute(string name, DbType type)
			: this() {
			ColumnName = name;
			ColumnType = type;
			PropertyType = ConvertDbTypeToSystem();
			ApplyDataColumnOptions(Options);
		}

		/// <summary>Map a property to a database column or XML element</summary>
		/// <param name="name">Name of the column or element to map</param>
		/// <param name="type">Underlying DbType of the column</param>
		public DataColumnAttribute(string name, DbType type, DataColumnOptions options)
			: this(name, type) {
			Options = options;
			ApplyDataColumnOptions(options);
		}

		public DataColumnAttribute(string name, DbType type, int size)
			: this(name, type) => Size = size;

		/// <summary>Map a property to a database column or XML element</summary>
		/// <param name="name">Name of the column or element to map</param>
		/// <param name="type">Underlying DbType of the column</param>
		/// <param name="size">Size of the column</param>
		public DataColumnAttribute(string name, DbType type, int size, DataColumnOptions options)
			: this(name, type, options) => Size = size;

		//----------
		public DataColumnAttribute(string name, DbType type, Type extendedPropertyType, string extendedPropertyName)
			: this(name, type) {
			ExtendedPropertyType = extendedPropertyType;
			ExtendedPropertyName = extendedPropertyName;
		}

		public DataColumnAttribute(string name, DbType type, Type extendedPropertyType, string extendedPropertyName, DataColumnOptions options)
			: this(name, type, extendedPropertyType, extendedPropertyName) {
			Options = options;
			ApplyDataColumnOptions(options);
		}

		public DataColumnAttribute(string name, DbType type, int size, Type extendedPropertyType, string extendedPropertyName)
			: this(name, type, extendedPropertyType, extendedPropertyName) => Size = size;

		public DataColumnAttribute(string name, DbType type, int size, Type extendedPropertyType, string extendedPropertyName, DataColumnOptions options)
			: this(name, type, extendedPropertyType, extendedPropertyName, options) => Size = size;

		#endregion Constructors

		#region Instance Methods

		public DbTypeSupported ConvertTypeToDbTypeSupported() {
			switch (ColumnType) {
				case DbType.UInt64:
				case DbType.Int64:
					return DbTypeSupported.dbInt64;

				case DbType.UInt16:
				case DbType.Int16:
					return DbTypeSupported.dbInt16;

				case DbType.Int32:
				case DbType.UInt32:
					return DbTypeSupported.dbInt32;

				case DbType.Boolean:
					return DbTypeSupported.dbBit;

				case DbType.Date:
				case DbType.DateTime:
					return DbTypeSupported.dbDateTime;

				case DbType.DateTime2:
					return DbTypeSupported.dbDateTime2;

				case DbType.Decimal:
					return DbTypeSupported.dbDecimal;

				case DbType.Double:
				case DbType.Single:
					return DbTypeSupported.dbDouble;

				case DbType.String:
					return DbTypeSupported.dbNVarChar;

				case DbType.AnsiString:
					return DbTypeSupported.dbVarChar;

				case DbType.Binary:
					return DbTypeSupported.dbBlob;

				case DbType.Object:
					return DbTypeSupported.dbImage;

				case DbType.Guid:
					return DbTypeSupported.dbGUID;

				default:
					throw new Exceptions.CodedThoughtApplicationException(string.Format("Can not convert DbType.{0} to an equivalent type in DbTypeSupported", ColumnType.ToString()));
			}
		}

		/// <summary>Converts the db type to system type.</summary>
		/// <returns></returns>
		/// <exception cref="CodedThoughtApplicationException"></exception>
		public Type ConvertDbTypeToSystem() {
			switch (ColumnType) {
				case DbType.UInt64:
				case DbType.Int64:
					return typeof(System.Int64);

				case DbType.UInt16:
				case DbType.Int16:
					return typeof(System.Int16);

				case DbType.Int32:
				case DbType.UInt32:
					return typeof(System.Int32);

				case DbType.Boolean:
					return typeof(System.Boolean);

				case DbType.Date:
				case DbType.DateTime:
				case DbType.DateTime2:
					return typeof(DateTime);

				case DbType.Decimal:
					return typeof(System.Decimal);

				case DbType.Double:
				case DbType.Single:
					return typeof(System.Double);

				case DbType.String:
				case DbType.AnsiString:
					return typeof(System.String);

				case DbType.Binary:
				case DbType.Object:
					return typeof(System.Object);

				case DbType.Guid:
					return typeof(Guid);

				default:
					throw new Exceptions.CodedThoughtApplicationException(string.Format("Can not convert DBType.{0} to an equivalent type in System Type", ColumnType.ToString()));
			}
		}

		private void ApplyDataColumnOptions(DataColumnOptions options) {
			if (options.HasFlag(DataColumnOptions.PrimaryKey)) {
				IsPrimaryKey = true;
				IsUpdateable = false;
			} else {
				IsUpdateable = true;
			}
			if (options.HasFlag(DataColumnOptions.UpdateablePrimaryKey)) {
				IsUpdateable = true;
			}
			if (options.HasFlag(DataColumnOptions.OverridesInherited)) {
				OverridesInherited = true;
			}
			if (options.HasFlag(DataColumnOptions.LoadExtendedProperty)) {
				LoadExtendedPropertyObject = true;
			}
			if (options.HasFlag(DataColumnOptions.AllowNull)) {
				AllowNulls = true;
			}
		}

		#endregion Instance Methods
	}
}