namespace CodedThought.Core.Data {

	/// <summary>Parameter for comparing a date range</summary>
	public class BetweenParameter : Parameter, IParameter {
		private IDataParameter _dbParam2;

		/// <summary>Initializes a new instance of the <see cref="BetweenParameter" /> class.</summary>
		public BetweenParameter() : base() {
		}

		public BetweenParameter(IDataParameter startParam, IDataParameter endParam)
			: base(startParam) {
			_dbParam2 = endParam;
			ParameterName = startParam.ParameterName;
		}

		#region Properties

		public IDataParameter Parameter2 {
			get { return _dbParam2; }
			set { _dbParam2 = value; }
		}

		/// <summary>Gets and sets the DbType of the parameter.</summary>
		public new DbType DbType {
			get {
				return _dbParam.DbType;
			}
			set {
				_dbParam.DbType = value;
				_dbParam2.DbType = value;
			}
		}

		/// <summary>Gets and sets the direction of the parameter</summary>
		public new ParameterDirection Direction {
			get {
				return _dbParam.Direction;
			}
			set {
				_dbParam.Direction = value;
				_dbParam2.Direction = value;
			}
		}

		/// <summary>Gets and sets the parameter Name</summary>
		public new string ParameterName {
			get {
				return _dbParam.ParameterName;
			}
			set {
				_dbParam.ParameterName = value + "_1";
				_dbParam2.ParameterName = value + "_2";
			}
		}

		/// <summary>Gets and sets the source column</summary>
		public new string SourceColumn {
			get {
				return _dbParam.SourceColumn;
			}
			set {
				_dbParam.SourceColumn = value;
				_dbParam2.SourceColumn = value;
			}
		}

		/// <summary>Gets and sets the Source Version</summary>
		public new DataRowVersion SourceVersion {
			get {
				return _dbParam.SourceVersion;
			}
			set {
				_dbParam.SourceVersion = value;
				_dbParam2.SourceVersion = value;
			}
		}

		/// <summary>Gets and sets the second date for the between clause</summary>
		public object Value2 {
			get { return _dbParam2.Value; }
			set { _dbParam2.Value = value; }
		}

		#endregion Properties

		/// <summary>Converts to a parameter string.</summary>
		/// <param name="ParameterConnector">The parameter connector.</param>
		/// <returns></returns>
		public override string ToParameterString(string ParameterConnector, bool firstInGroup = true) {
			// Add single quotes if necessary.
			switch (_dbParam.DbType) {
				case DbType.String:
					_dbParam.Value = $"'{_dbParam.Value}'";
					_dbParam2.Value = $"'{_dbParam2.Value}'";
					_dbParam.DbType = DbType.String;
					_dbParam2.DbType = DbType.String;
					break;

				case DbType.Date:
				case DbType.DateTime:
				case DbType.DateTime2:
					// Convert the values to string to match the to_date function.
					_dbParam.Value = $"'{Convert.ToDateTime(_dbParam.Value).ToString("MM/dd/yyyy")}'";
					_dbParam2.Value = $"'{Convert.ToDateTime(_dbParam2.Value).ToString("MM/dd/yyyy")}'";
					_dbParam.DbType = DbType.String;
					_dbParam2.DbType = DbType.String;
					break;
			}
			return $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} BETWEEN {_dbParam.Value} AND {_dbParam2.Value}";
		}
	}
}