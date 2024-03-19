namespace CodedThought.Core.Data.Interfaces {

	public interface IParameter {
		IDataParameter BaseParameter { get; set; }

		DbType DbType { get; set; }

		ParameterDirection Direction { get; set; }

		bool IsNullable { get; }

		string ParameterName { get; set; }

		string SourceColumn { get; set; }

		DataRowVersion SourceVersion { get; set; }

		object Value { get; set; }

		WhereType WhereType { get; set; }

		string ToParameterString(string ParameterConnector, bool firstInGroup = true);
	}
}