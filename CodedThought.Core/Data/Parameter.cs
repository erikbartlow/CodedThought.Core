namespace CodedThought.Core.Data {

	/// <summary>Parameter provides a base parameter class that can be used to create parameters for any DB.</summary>
	public class Parameter : IDataParameter, IParameter {

		#region Data

		/// <summary></summary>
		protected IDataParameter _dbParam;

		protected WhereType _whereType;

		#endregion Data

		#region Constructors

		/// <summary>Default Constructor</summary>
		public Parameter() {
			_whereType = WhereType.AND;
		}

		/// <summary>Initializes a new instance of the <see cref="Parameter" /> class.</summary>
		/// <param name="param">The parameter.</param>
		public Parameter(IDataParameter param) : this() {
			_dbParam = param;
		}

		/// <summary>Initializes a new instance of the <see cref="Parameter" /> class.</summary>
		/// <param name="param">    The parameter.</param>
		/// <param name="whereType">Type of the where.</param>
		public Parameter(IDataParameter param, WhereType whereType) : this(param) {
			_whereType = whereType;
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets and sets the DbType of the parameter.</summary>
		public DbType DbType {
			get { return _dbParam.DbType; }
			set { _dbParam.DbType = value; }
		}

		/// <summary>Gets and sets the direction of the parameter</summary>
		public ParameterDirection Direction {
			get { return _dbParam.Direction; }
			set { _dbParam.Direction = value; }
		}

		/// <summary>Gets a value indicating if the parameter accepts nulls</summary>
		public bool IsNullable {
			get { return _dbParam.IsNullable; }
		}

		/// <summary>Gets and sets the parameter Name</summary>
		public string ParameterName {
			get { return _dbParam.ParameterName; }
			set { _dbParam.ParameterName = value; }
		}

		/// <summary>Gets and sets the source column</summary>
		public string SourceColumn {
			get { return _dbParam.SourceColumn; }
			set { _dbParam.SourceColumn = value; }
		}

		/// <summary>Gets and sets the Source Version</summary>
		public DataRowVersion SourceVersion {
			get { return _dbParam.SourceVersion; }
			set { _dbParam.SourceVersion = value; }
		}

		/// <summary>gets and sets the value of the parameter</summary>
		public object Value {
			get { return _dbParam.Value; }
			set { _dbParam.Value = value; }
		}

		/// <summary>Gets the underlying database specific Parameter</summary>
		public IDataParameter BaseParameter {
			get { return _dbParam; }
			set { _dbParam = value; }
		}

		/// <summary>Gets or sets the type of the where clause.</summary>
		/// <value>The type of the where.</value>
		public WhereType WhereType {
			get { return _whereType; }
			set { _whereType = value; }
		}

		#endregion Properties

		#region Public Methods

		/// <summary>Toes the parameter string.</summary>
		/// <param name="ParameterConnector">The parameter connector.</param>
		/// <returns></returns>
		public virtual string ToParameterString(string ParameterConnector, bool firstInGroup = true) {
			return $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} = {ParameterConnector}{_dbParam.ParameterName}";
		}

		#endregion Public Methods
	}
}