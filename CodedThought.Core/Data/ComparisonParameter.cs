namespace CodedThought.Core.Data {

	public enum ComparisonParameterType {
		Equal,
		NotEqual,
		GreaterThan,
		GreatherThanOrEqual,
		LessThan,
		LessThanOrEqual,
		Like
	}

	public enum ComparisonOrigin {
		ValueBased,
		ColumnBased
	}

	public class ComparisonParameter : Parameter {

		public ComparisonParameter() : base() {
		}

		public ComparisonParameter(IDataParameter baseParam)
			: base(baseParam) {
		}

		public ComparisonParameter(IDataParameter baseParam, ComparisonParameterType comparisonType, ComparisonOrigin origin)
			: this(baseParam) {
			ComparisonParameterType = comparisonType;
			ComparisonOrigin = origin;
		}

		/// <summary>Gets or sets the type of the comparison parameter.</summary>
		/// <value>The type of the comparison parameter.</value>
		public ComparisonParameterType ComparisonParameterType { get; set; }

		/// <summary>
		/// Gets or sets the comparison origin. In some cases, using a comparison parameter might be directly related to a property in your object. Therefore, you can select to use the ValueBased
		/// origin type.
		/// </summary>
		/// <value>The comparison origin.</value>
		public ComparisonOrigin ComparisonOrigin { get; set; }

		public override string ToParameterString(string ParameterConnector, bool firstInGroup = true) {
			string strOperator = string.Empty;
			// Add single quotes if necessary.
			switch (_dbParam.DbType) {
				case DbType.String:
					_dbParam.Value = $"'{_dbParam.Value}'";
					_dbParam.DbType = DbType.String;
					break;

				case DbType.Date:
				case DbType.DateTime:
				case DbType.DateTime2:
					// Convert the values to string to match the to_date function.
					_dbParam.Value = $"'{Convert.ToDateTime(_dbParam.Value).ToString("MM/dd/yyyy")}'";
					_dbParam.DbType = DbType.String;
					break;
			}
			switch (ComparisonParameterType) {
				case ComparisonParameterType.Equal:
					strOperator = "=";
					break;

				case ComparisonParameterType.NotEqual:
					strOperator = "<>";
					break;

				case ComparisonParameterType.GreaterThan:
					strOperator = ">";
					break;

				case ComparisonParameterType.GreatherThanOrEqual:
					strOperator = ">=";
					break;

				case ComparisonParameterType.LessThan:
					strOperator = "<";
					break;

				case ComparisonParameterType.LessThanOrEqual:
					strOperator = "<=";
					break;

				case ComparisonParameterType.Like:
					strOperator = "LIKE";
					break;
			}
			switch (ComparisonOrigin) {
				case ComparisonOrigin.ColumnBased:
					return $"  {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} {strOperator} {ParameterConnector}{_dbParam.ParameterName}";

				case ComparisonOrigin.ValueBased:
				default:
					return $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} {strOperator} {_dbParam.Value}";
			}
		}
	}
}