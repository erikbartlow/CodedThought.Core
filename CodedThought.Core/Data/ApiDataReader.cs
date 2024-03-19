using System.Net;

namespace CodedThought.Core.Data {

	public class ApiDataReader : IDataReader {

		#region Properties

		private DatabaseConnection? _connectionSetting;
		protected static string? SourceUrl { get; set; }
		protected static string? Controller { get; set; }
		protected static string? Username { get; set; }
		protected static string? Password { get; set; }
		int IDataReader.Depth => 1;
		int IDataReader.RecordsAffected => -1;
		int IDataRecord.FieldCount => throw new NotImplementedException();
		public dynamic Data;
		private bool disposedValue;
		public string Raw { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string? ErrorMessage { get; set; }
		public int Timeout { get; set; }
		#endregion Properties

		#region Constructors

		public ApiDataReader(string connectionString) {
			PopulateCurrentConnection(connectionString);
			Timeout = 0;
		}

		#endregion Constructors

		/// <summary>Executes a call to the configured Api Source with the passed action and parameters.</summary>
		/// <param name="action">    </param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public async Task<ApiDataReader> ExecuteReader(string controller, string action, ParameterCollection parameters) {
			try {

				Dictionary<string, string> values = new();
				if (parameters != null) {
					foreach (IDataParameter parameter in parameters) {
						values.Add(parameter.ParameterName, parameter.Value.ToString());
					}
				}
				FormUrlEncodedContent content = new(values);

				using (HttpClient httpClient = new()) {
					if (Timeout > 0)
						httpClient.Timeout = new TimeSpan(0, 0, Timeout);
					Uri baseUri = new(SourceUrl);
					httpClient.BaseAddress = baseUri;
					httpClient.DefaultRequestHeaders.Clear();
					if (Username != null && Password != null)
						httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {EncodeBasicAuthCredentials(Username, Password)}");

					HttpRequestMessage requestMessage;
					if (!string.IsNullOrEmpty(controller)) {
						requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{SourceUrl}/{controller}/{action}{values.ToQueryString()}") {
							Content = content
						};
					} else {
						requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{SourceUrl}/{action}{values.ToQueryString()}") {
							Content = content
						};
					}
					using (HttpResponseMessage apiCall = await httpClient.SendAsync(requestMessage)) {
						Raw = await apiCall.Content.ReadAsStringAsync();
						if (apiCall.StatusCode == System.Net.HttpStatusCode.OK) {
							StatusCode = apiCall.StatusCode;
							Data = JsonConvert.DeserializeObject<dynamic>(Raw);
						} else {
							StatusCode = apiCall.StatusCode;
							ErrorMessage = apiCall.ReasonPhrase;
						}
					}
				}
				return this;
			} catch (Exception) {
				throw;
			}
		}

		/// <summary>Parses the passed REST Api connection string.</summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		private void PopulateCurrentConnection(string connectionSetting) {
			string[] urlParts = connectionSetting.Split(";".ToCharArray());
			for (int i = 0; i <= urlParts.Length - 1; i++) {
				string[] connectionParameter = urlParts[i].Split("=".ToCharArray());
				switch (connectionParameter[0]) {
					case "Api Url":
					case "Data Source":
						SourceUrl = connectionParameter[1];
						break;
					case "User Id":
						Username = connectionParameter[1];
						break;
					case "Password":
						Password = connectionParameter[1];
						break;
				}
			}
		}

		/// <summary>Encodes the basic authentication to pass in HttpClient web calls.</summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		private string EncodeBasicAuthCredentials(string username, string password) => Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes($"Basic {username}:{password}"));

		#region Unimplemented Methods

		bool IDataReader.IsClosed => throw new NotImplementedException();

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

		void IDataReader.Close() => Data = null;

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					Data = null;
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