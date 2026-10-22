using Microsoft.Extensions.Configuration;

namespace CodedThought.Core.Configuration
{

    /// <summary>Accessors for the connections declared in the CoreSettings section.</summary>
    /// <remarks>
    /// Each accessor binds the section on call.  Applications that resolve connections repeatedly should register the section through AddCoreSettings and inject
    /// <see cref="Microsoft.Extensions.Options.IOptionsMonitor{TOptions}" /> instead.
    /// </remarks>
    public static class CoreConnectionExtensions
    {

        [Obsolete("This method is obsolete as it was renamed to GetCorePrimaryConnection.")]
        public static ConnectionSetting? GetCorePrimaryConnectionString(this IConfiguration configuration) => configuration.GetCorePrimaryConnection();

        /// <summary>
        /// Gets a distinct list of ProviderType values from each connection in the Connections section.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>An empty list when no connections are configured.</returns>
        public static List<string> GetCoreDatabaseProviders(this IConfiguration configuration) => [.. configuration.GetCoreSettings().Connections.Select(cn => cn.ProviderType).Distinct(StringComparer.OrdinalIgnoreCase)];

        /// <summary>
        /// Gets the database connection from the ctSettings file with the Primary=true setting.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns><c>null</c> when no connection is marked primary.</returns>
        public static ConnectionSetting? GetCorePrimaryConnection(this IConfiguration configuration) => configuration.GetCoreSettings().Connections.FirstOrDefault(x => x.Primary);

        /// <summary>
        /// Gets the database connection from the ctSettings file matching the passed name.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="name">The connection name.  Matching ignores case.</param>
        /// <returns><c>null</c> when no connection carries the passed name.</returns>
        public static ConnectionSetting? GetDatabaseConnection(this IConfiguration configuration, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return configuration.GetCoreSettings().Connections.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the connection marked Primary=true, throwing when none is configured.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ConnectionSetting GetRequiredCorePrimaryConnection(this IConfiguration configuration)
        {
            CoreSettings settings = configuration.GetCoreSettings();
            return settings.Connections.FirstOrDefault(x => x.Primary)
                ?? throw new InvalidOperationException($"No connection in the {nameof(CoreSettings)} section is marked Primary.  {DescribeConfiguredConnections(settings)}");
        }

        /// <summary>
        /// Gets the connection matching the passed name, throwing when it is not configured.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="name">The connection name.  Matching ignores case.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ConnectionSetting GetRequiredDatabaseConnection(this IConfiguration configuration, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            CoreSettings settings = configuration.GetCoreSettings();
            return settings.Connections.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"No connection named '{name}' exists in the {nameof(CoreSettings)} section.  {DescribeConfiguredConnections(settings)}");
        }

        /// <summary>Binds the CoreSettings section into a new instance.</summary>
        public static CoreSettings GetCoreSettings(this IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            CoreSettings settings = new();
            configuration.GetSection(nameof(CoreSettings)).Bind(settings);
            return settings;
        }

        /// <summary>Lists the configured connection names so a lookup failure names what is available.</summary>
        private static string DescribeConfiguredConnections(CoreSettings settings) => settings.Connections.Count == 0
            ? "No connections are configured."
            : $"Configured connections: {string.Join(", ", settings.Connections.Select(c => String.IsNullOrWhiteSpace(c.Name) ? "(unnamed)" : c.Name))}.";
    }
}
