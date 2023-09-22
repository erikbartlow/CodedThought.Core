using System.Configuration;

namespace CodedThought.Core.Data {

	/// <summary>Summary description for DatabaseConnection. Created by: bartlowe Created on: 6/29/2011 17:22:15</summary>
	public class DatabaseConnection {

		#region Properties

		/// <summary>Gets or sets the connection name found in the configuration file.</summary>
		/// <value>The name of the connection.</value>
		public String? ConnectionName { get; set; }

		/// <summary>Gets or sets the connection string.</summary>
		/// <value>The connection string.</value>
		public String? ConnectionString { get; set; }

		/// <summary>Gets or sets the type of the database.</summary>
		/// <value>The type of the database.</value>
		public DBSupported DatabaseType { get; set; }

		/// <summary>Gets or sets the name of the schema.</summary>
		/// <value>The name of the schema.</value>
		public String? SchemaName { get; set; }

		/// <summary>Gets or sets the command timeout.</summary>
		public Int32 CommandTimeout { get; set; }

		public List<string> CoreDatabaseParameters {
			get {
				List<String> cnArray = new();
				cnArray.AddRange(ConfigurationManager.AppSettings["CoreDatabaseParameters"].Split(" | ".ToCharArray()));
				return cnArray;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>Decrypts the encryption string.</summary>
		/// <param name="encryptedString">The encrypted string.</param>
		/// <returns></returns>
		public String DecryptString(string encryptedString, string encryptionKey) {
			try {
				return CodedThought.Core.Security.HPEncryption.DecryptPassword(encryptedString);
			} catch {
				return string.Empty;
			}
		}

		#endregion Methods

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="DatabaseConnection" /> class.</summary>
		public DatabaseConnection() {
			CommandTimeout = 0;
		}

		/// <summary>Initial constructor for DatabaseConnection</summary>
		public DatabaseConnection(HPConnectionSetting connection) {
			ConnectionName = connection.Name;
			ConnectionString = connection.ConnectionString;
			string dbType = connection.ProviderType;
			DatabaseType = (DBSupported)Enum.Parse(typeof(DBSupported), dbType);
			SchemaName = connection.DefaultSchema;
			CommandTimeout = 30;  // Default 30 second timeout.
		}

		/// <summary>Initializes a new instance of the <see cref="DatabaseConnection" /> class.</summary>
		/// <param name="connectionName">Name of the connection.</param>
		/// <param name="timeout">       The timeout.</param>
		public DatabaseConnection(HPConnectionSetting connection, Int32 timeout) : this(connection) {
			CommandTimeout = timeout;
		}

		/// <summary>Initializes a new instance of the <see cref="DatabaseConnection" /> class.</summary>
		/// <param name="connectionName">Name of the connection.</param>
		/// <param name="timeout">       The timeout.</param>
		public DatabaseConnection(HPConnectionSetting connection, string schemaName, Int32 timeout) : this(connection, timeout) {
			SchemaName = schemaName;
			CommandTimeout = timeout;
		}

		/// <summary>Initializes a new instance of the <see cref="DatabaseConnection" /> class.</summary>
		/// <param name="connectionName">Name of the connection.</param>
		/// <param name="decryptionKey"> The decryption key.</param>
		public DatabaseConnection(HPConnectionSetting connection, string decryptionKey) : this(connection) {
			ConnectionString = DecryptString(connection.ConnectionString, decryptionKey);
		}

		/// <summary>Initializes a new instance of the <see cref="DatabaseConnection" /> class.</summary>
		/// <param name="connectionName">  Name of the connection.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="dbType">          Type of the db.</param>
		/// <param name="schemaName">      Name of the schema.</param>
		public DatabaseConnection(string connectionName, string connectionString, DBSupported dbType, string schemaName) {
			ConnectionName = connectionName;
			ConnectionString = connectionString;
			DatabaseType = dbType;
			SchemaName = schemaName;
		}

		/// <summary>Initializes a new instance of the <see cref="DatabaseConnection" /> class.</summary>
		/// <param name="connectionName">  Name of the connection.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="dbType">          Type of the db.</param>
		/// <param name="schemaName">      Name of the schema.</param>
		/// <param name="decryptKey">      The decrypt key.</param>
		public DatabaseConnection(string connectionName, string connectionString, DBSupported dbType, string schemaName, string decryptKey) : this(connectionName, connectionString, dbType, schemaName) {
			ConnectionString = DecryptString(connectionString, decryptKey);
		}

		#endregion Constructors
	}
}