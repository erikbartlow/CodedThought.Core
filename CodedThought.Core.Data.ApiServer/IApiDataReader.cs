using CodedThought.Core.Configuration;
using Newtonsoft.Json;
using System.Data;

namespace CodedThought.Core.Data.ApiServer {

	public class ApiDataReader : IDataReader {

		#region Properties

		private DatabaseConnection? _connectionSetting;
		protected static string? SourceUrl { get; set; }
		protected static string? Controller { get; set; }
		protected static string? Action { get; set; }
		protected static string? Username { get; set; }
		protected static string? Password { get; set; }
		int IDataReader.Depth => 1;
		int IDataReader.RecordsAffected => data.Data.Count;
		int IDataRecord.FieldCount => throw new NotImplementedException();
		private dynamic data;
		private bool disposedValue;

		#endregion Properties

		#region Constructors

		public ApiDataReader(HPConnectionSetting connectionSetting) {
			PopulateCurrentConnection(connectionSetting);
		}

		#endregion Constructors

		/// <summary>Executes a call to the configured Api Source with the passed action and parameters.</summary>
		/// <param name="action">    </param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public async Task<dynamic> ExecuteReader(string controller, string action, ParameterCollection parameters) {
			try {
				string actionParameters = string.Empty;
				foreach (Parameter parameter in parameters) {
					actionParameters += $"{parameter.ParameterName}&{parameter.Value}";
				}
				using (var httpClient = new HttpClient()) {
					using (var apiCall = await httpClient.GetAsync($"{SourceUrl}/{controller}{action}?{actionParameters}")) {
						string apiResponse = await apiCall.Content.ReadAsStringAsync();
						data = JsonConvert.DeserializeObject<dynamic>(apiResponse);
						return data;
					}
				}
			} catch (Exception) {
				throw;
			}
		}

		/// <summary>Parses the passed REST Api connection string.</summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		private void PopulateCurrentConnection(HPConnectionSetting connectionSetting) {
			_connectionSetting = new() {
				ConnectionName = connectionSetting.Name,
				ConnectionString = connectionSetting.ConnectionString,
				CommandTimeout = 0,
				DatabaseType = DBSupported.ApiServer,
				SchemaName = connectionSetting.DefaultSchema
			};
			string[] urlParts = connectionSetting.ConnectionString.Split(";".ToCharArray());
			for (int i = 0; i <= urlParts.Length - 1; i++) {
				string[] connectionParameter = urlParts[i].Split("=".ToCharArray());
				switch (connectionParameter[0]) {
					case "Api Url":
						SourceUrl = connectionParameter[1]; break;
					case "User Id":
						Username = connectionParameter[1]; break;
					case "Password":
						Password = connectionParameter[1]; break;
				}
			}
		}

		#region Unimplemented Methods

		bool IDataReader.IsClosed => throw new NotImplementedException();

		void IDataReader.Close() => throw new NotImplementedException();

		bool IDataReader.Read() => throw new NotImplementedException();

		object IDataRecord.this[int i] => throw new NotImplementedException();
		object IDataRecord.this[string name] => throw new NotImplementedException();

		bool IDataRecord.GetBoolean(int i) => throw new NotImplementedException();

		byte IDataRecord.GetByte(int i) => throw new NotImplementedException();

		long IDataRecord.GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

		char IDataRecord.GetChar(int i) => throw new NotImplementedException();

		long IDataRecord.GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

		IDataReader IDataRecord.GetData(int i) => throw new NotImplementedException();

		string IDataRecord.GetDataTypeName(int i) => throw new NotImplementedException();

		DateTime IDataRecord.GetDateTime(int i) => throw new NotImplementedException();

		decimal IDataRecord.GetDecimal(int i) => throw new NotImplementedException();

		double IDataRecord.GetDouble(int i) => throw new NotImplementedException();

		Type IDataRecord.GetFieldType(int i) => throw new NotImplementedException();

		float IDataRecord.GetFloat(int i) => throw new NotImplementedException();

		Guid IDataRecord.GetGuid(int i) => throw new NotImplementedException();

		short IDataRecord.GetInt16(int i) => throw new NotImplementedException();

		int IDataRecord.GetInt32(int i) => throw new NotImplementedException();

		long IDataRecord.GetInt64(int i) => throw new NotImplementedException();

		string IDataRecord.GetName(int i) => throw new NotImplementedException();

		int IDataRecord.GetOrdinal(string name) => throw new NotImplementedException();

		DataTable? IDataReader.GetSchemaTable() => throw new NotImplementedException();

		string IDataRecord.GetString(int i) => throw new NotImplementedException();

		object IDataRecord.GetValue(int i) => throw new NotImplementedException();

		int IDataRecord.GetValues(object[] values) => throw new NotImplementedException();

		bool IDataRecord.IsDBNull(int i) => throw new NotImplementedException();

		bool IDataReader.NextResult() => throw new NotImplementedException();

		#endregion Unimplemented Methods

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					data = null;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		void IDisposable.Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}