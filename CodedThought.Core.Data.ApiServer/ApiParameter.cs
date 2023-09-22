using System.ComponentModel;
using System.Data;

namespace CodedThought.Core.Data.ApiServer {

	public class ApiParameter : IDataParameter {

		public ApiParameter() {
			DbType = DbType.String;
			Direction = ParameterDirection.Input;
			ParameterName = string.Empty;
			Value = string.Empty;
		}

		public ApiParameter(DbType dbType, ParameterDirection direction, string parameterName, object? value) {
			DbType = dbType;
			Direction = direction;
			ParameterName = parameterName;
			Value = value;
			
		}

		public DbType DbType { get; set; }
		public ParameterDirection Direction { get; set; }
		/// <summary>
		/// Gets a value indicating whether the parameter accepts null values.
		/// </summary>
		public bool IsNullable {get; set; }
		/// <summary>
		/// The name of the parameter to use in the API call.
		/// </summary>
		public string ParameterName { get; set; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)] 
		public string SourceColumn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)] 
		public DataRowVersion SourceVersion { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		/// <summary>
		/// The value of the parameter.
		/// </summary>
		public object? Value { get; set; }
	}
}