namespace CodedThought.Core.Configuration
{

    /// <summary>A <see cref="ConnectionSetting" /> whose connection string describes a REST Api endpoint rather than a database server.</summary>
    public class ApiConnectionSetting : ConnectionSetting, IConnectionSetting
    {

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="ApiConnectionSetting" /> class from an existing connection setting.</summary>
        /// <param name="connectionSetting">The configured connection to parse the Api details from.</param>
        public ApiConnectionSetting(ConnectionSetting connectionSetting) : base() => PopulateCurrentConnection(connectionSetting);

        #endregion Constructors

        #region Properties

        /// <summary>Gets or sets the port the Api listens on, when one is supplied in the connection string.</summary>
        protected int Port { get; set; }

        /// <summary>Gets or sets the source URL for the Api.</summary>
        public string? SourceUrl { get; set; }

        /// <summary>Gets or sets the username to be used with the Api.</summary>
        public string? Username { get; set; }

        /// <summary>Gets or sets the password to be used with the Api.</summary>
        public string? Password { get; set; }

        #endregion Properties

        #region Private Methods

        /// <summary>Copies the passed connection and parses its REST Api connection string.</summary>
        /// <param name="connectionSetting">The configured connection to parse.</param>
        private void PopulateCurrentConnection(ConnectionSetting connectionSetting)
        {
            ArgumentNullException.ThrowIfNull(connectionSetting);

            Name = connectionSetting.Name;
            ProviderName = connectionSetting.ProviderName;
            ProviderType = connectionSetting.ProviderType;
            ConnectionString = connectionSetting.ConnectionString;
            Primary = connectionSetting.Primary;
            Timeout = connectionSetting.Timeout;
            DefaultSchema = connectionSetting.DefaultSchema;

            string[] urlParts = connectionSetting.ConnectionString.Split(";".ToCharArray());
            for (int i = 0; i <= urlParts.Length - 1; i++)
            {
                string[] connectionParameter = urlParts[i].Split("=".ToCharArray());
                switch (connectionParameter[0].ToLower())
                {
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

        #endregion Private Methods
    }
}
