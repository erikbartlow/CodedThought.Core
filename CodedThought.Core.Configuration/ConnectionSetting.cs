using System.Collections.Frozen;

namespace CodedThought.Core.Configuration
{

    public class ConnectionSetting : IConnectionSetting
    {
        /// <summary>The provider types the framework ships a data provider for.</summary>
        private static readonly FrozenSet<string> _supportedProviders = new[]
        {
            "SqlServer", "Oracle", "MongoDb", "OleDb", "MySql", "ApiServer", "PostgreSQL"
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        /// <summary>Primary constructor for the CoreSetting object.</summary>
        public ConnectionSetting()
        {
            Name = String.Empty;
            ConnectionString = String.Empty;
            DefaultSchema = "dbo";
            Primary = false;
            ProviderName = "SqlServer";
            ProviderType = "SqlServer";
            Timeout = 30;
        }

        /// <summary>Gets the provider types accepted by <see cref="ProviderType" />.</summary>
        public static IReadOnlyCollection<string> SupportedProviders => _supportedProviders;

        /// <summary>
        /// The name is used when retrieving the provider object from cache.
        /// </summary>
        /// <summary>Gets or sets the connection name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets whether this connection is the app's primary connection.</summary>
        public bool Primary { get; set; }

        /// <summary>Gets or sets the database provider type.</summary>
        /// <remarks>
        /// The value is not validated on assignment so that a malformed configuration file surfaces through the options validator with the offending connection name rather than as a binding failure.
        /// </remarks>
        public string ProviderType { get; set; }

        public string DefaultSchema { get; set; }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
        public Int32 Timeout { get; set; }

        /// <summary>Checks this connection's <see cref="ProviderType" /> against the supported types.</summary>
        /// <exception cref="ArgumentException"></exception>
        public void CheckProvider() => CheckProvider(ProviderType);

        /// <summary>Checks the passed provider type against the supported types.</summary>
        /// <exception cref="ArgumentException"></exception>
        public void CheckProvider(string providerType)
        {
            if (IsProviderSupported(providerType))
                return;

            string supported = string.Join(", ", _supportedProviders);
            throw String.IsNullOrWhiteSpace(providerType)
                ? new ArgumentException($"The connection provider is required.  Please use one of the following providers: {supported}.", nameof(providerType))
                : new ArgumentException($"The connection provider, {providerType}, is not supported.  Please use one of the following providers: {supported}.", nameof(providerType));
        }

        /// <summary>Gets whether the passed provider type is one the framework supports.</summary>
        public static bool IsProviderSupported(string? providerType) => !String.IsNullOrWhiteSpace(providerType) && _supportedProviders.Contains(providerType);
    }
}