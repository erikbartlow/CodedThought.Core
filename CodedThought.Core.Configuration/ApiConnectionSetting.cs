namespace CodedThought.Core.Configuration {
	public class ApiConnectionSetting : ConnectionSetting {

		public ApiConnectionSetting(ConnectionSetting hpConnectionSetting) : base() => PopulateCurrentConnection(hpConnectionSetting);

		#region Properties
		protected int Port { get; set; }
		/// <summary>
		/// Source URL for the Api.
		/// </summary>
		public string? SourceUrl { get; set; }
		/// <summary>
		/// Username to be used with Api.
		/// </summary>
		public string? Username { get; set; }
		/// <summary>
		/// Password to be used with the Api.
		/// </summary>
		public string? Password { get; set; }

		#endregion Properties

		/// <summary>
		/// Parses the passed REST Api connection string.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		private void PopulateCurrentConnection(ConnectionSetting connectionSetting) {

			Name = connectionSetting.Name;

			string[] urlParts = connectionSetting.ConnectionString.Split(";".ToCharArray());
			for (int i = 0; i <= urlParts.Length - 1; i++) {
				string[] connectionParameter = urlParts[i].Split("=".ToCharArray());
				switch (connectionParameter[0].ToLower()) {
					case "api url":
					case "data source":
						SourceUrl = connectionParameter[1]; break;
					case "port":
						Port = int.Parse(connectionParameter[1]); break;
					case "user id":
						Username = connectionParameter[1]; break;
					case "password":
						Password = connectionParameter[1]; break;
				}
			}
		}
	}
}
