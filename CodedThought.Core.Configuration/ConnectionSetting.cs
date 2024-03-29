﻿namespace CodedThought.Core.Configuration
{

	public class ConnectionSetting
	{
		private string _providerType;
		/// <summary>Primary constructor for the CoreSetting object.</summary>
		public ConnectionSetting()
		{
			Name = String.Empty;
			ConnectionString = String.Empty;
			DefaultSchema = "dbo";
			Primary = false;
			ProviderName = "SqlServer";
			ProviderType = "SqlServer";
			Timeout = 0;
		}
		/// <summary>
		/// The name is used when retrieving the provider object from cache.
		/// </summary>
		/// <summary>Gets or sets the connection name.</summary>
		public string Name { get; set; }

		/// <summary>Gets or sets whether this connection is the app's primary connection.</summary>
		public bool Primary { get; set; }

		/// <summary>Gets or sets the database provider type.</summary>
		public string ProviderType
		{
			get => _providerType;
			set
			{
				try
				{
					CheckProvider(value);
					_providerType = value;
				}
				catch { throw; }
			}
		}

		public string DefaultSchema { get; set; }
		public string ConnectionString { get; set; }
		public string ProviderName { get; set; }
		public Int32 Timeout { get; set; }

		/// <summary>
		/// Checks the provider for supported types.
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void CheckProvider()
		{
			string providerValue = ProviderType;
			List<string> validProviders = new();
			validProviders.AddRange("SqlServer,Oracle,MongoDb,OleDb,MySql,ApiServer".ToLower().Split(",".ToCharArray()));

			if (String.IsNullOrEmpty(providerValue))
			{
				throw new Exception($"The connection provider is required.  Please use one of the following provider, {string.Join(", ", [.. validProviders])}");

			}
			else
			{
				throw new Exception($"The connection provider, {providerValue}, is not supported.  Please use one of the following provider, {string.Join(", ", validProviders.ToArray())}");
			}
		}
		/// <summary>Checks the provider for supported types.</summary>
		public void CheckProvider(string providerType)
		{
			string providerValue = providerType;
			List<string> validProviders = new();
			validProviders.AddRange("SqlServer,Oracle,MongoDb,OleDb,MySql,ApiServer".ToLower().Split(",".ToCharArray()));

			if (!validProviders.Contains(providerValue.ToLower()))
			{
				if (String.IsNullOrEmpty(providerValue))
				{
					throw new Exception($"The connection provider is required.  Please use one of the following provider, {string.Join(", ", [.. validProviders])}");

				}
				else
				{
					throw new Exception($"The connection provider, {providerValue}, is not supported.  Please use one of the following provider, {string.Join(", ", validProviders.ToArray())}");
				}
			}
		}
	}
}