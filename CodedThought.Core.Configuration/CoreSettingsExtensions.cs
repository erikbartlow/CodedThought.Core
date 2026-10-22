using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CodedThought.Core.Configuration
{

    public static class CoreSettingsExtensions
    {

        /// <summary>The default name of the CodedThought.Core settings file.</summary>
        public const string DefaultSettingsFileName = "ctSettings.json";

        /// <summary>
        /// Loads the default ctSettings.json file to the builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <param name="includeEnvironmentVariables">Appends the environment variable provider so settings can be overridden per host.</param>
        /// <returns></returns>
        /// <remarks>The default name for the settings file is ctSettings.json</remarks>
        public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, bool optional = true, bool reloadOnChange = true, bool includeEnvironmentVariables = true) => builder.AddCoreSettingsConfiguration(DefaultSettingsFileName, optional, reloadOnChange, includeEnvironmentVariables);

        /// <summary>
        /// Loads the CodedThought configuration json file to the builder using the passed file name.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="settingsFileName"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <param name="includeEnvironmentVariables">Appends the environment variable provider so settings can be overridden per host.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, string settingsFileName, bool optional = true, bool reloadOnChange = true, bool includeEnvironmentVariables = true)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrWhiteSpace(settingsFileName);

            builder.AddJsonFile(settingsFileName, optional, reloadOnChange);
            if (includeEnvironmentVariables)
                builder.AddEnvironmentVariables();

            return builder;
        }

        /// <summary>
        /// Loads the default ctSettings.json file and the corresponding environment instance of the ctSettings.{env}.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <param name="includeEnvironmentVariables">Appends the environment variable provider so settings can be overridden per host.</param>
        /// <returns></returns>
        /// <remarks>The naming of the settings json file is case sensitive and should match the environment name in the ASPNETCORE_ENVIRONMENT variable.</remarks>
        public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env, bool optional = true, bool reloadOnChange = true, bool includeEnvironmentVariables = true) => builder.AddCoreSettingsConfiguration(env, DefaultSettingsFileName, optional, reloadOnChange, includeEnvironmentVariables);

        /// <summary>
        /// Loads the CodedThought.Core settings file using the passed file name and the corresponding environment instance of the {settings file name}.{env}.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <param name="settingsFileName"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <param name="includeEnvironmentVariables">Appends the environment variable provider so settings can be overridden per host.</param>
        /// <returns></returns>
        /// <remarks>
        /// The environment specific file is always added and is always optional, so its presence alone determines whether it contributes.  The naming of the settings json file is case
        /// sensitive and should match the environment name in the ASPNETCORE_ENVIRONMENT variable.
        /// </remarks>
        public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env, string settingsFileName, bool optional = true, bool reloadOnChange = true, bool includeEnvironmentVariables = true)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(env);
            ArgumentException.ThrowIfNullOrWhiteSpace(settingsFileName);

            builder.AddJsonFile(settingsFileName, optional, reloadOnChange);
            builder.AddJsonFile(BuildEnvironmentFileName(settingsFileName, env.EnvironmentName), optional: true, reloadOnChange);
            if (includeEnvironmentVariables)
                builder.AddEnvironmentVariables();

            return builder;
        }

        /// <summary>
        /// Loads the default appsettings.json file to the builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder, bool optional = false, bool reloadOnChange = true)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.AddJsonFile("appsettings.json", optional, reloadOnChange);
            return builder;
        }

        /// <summary>
        /// Loads the default appsettings.json file and the corresponding environment instance of the appsettings.{env}.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        /// <remarks>
        /// The environment specific file is always added and is always optional, so its presence alone determines whether it contributes.  The naming of the settings json file is case
        /// sensitive and should match the environment name in the ASPNETCORE_ENVIRONMENT variable.
        /// </remarks>
        public static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env, bool optional = true, bool reloadOnChange = true)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(env);

            builder.AddAppSettingsConfiguration(optional, reloadOnChange);
            builder.AddJsonFile(BuildEnvironmentFileName("appsettings.json", env.EnvironmentName), optional: true, reloadOnChange);
            return builder;
        }

        /// <summary>Builds the environment specific name for the passed settings file, preserving any directory in the path.</summary>
        private static string BuildEnvironmentFileName(string settingsFileName, string environmentName)
        {
            string? directory = Path.GetDirectoryName(settingsFileName);
            string environmentFile = $"{Path.GetFileNameWithoutExtension(settingsFileName)}.{environmentName}{Path.GetExtension(settingsFileName)}";
            return String.IsNullOrEmpty(directory) ? environmentFile : Path.Combine(directory, environmentFile);
        }
    }
}