using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CodedThought.Core.Configuration {

	public static class HPCoreSettingsExtensions {

		/// <summary>
		/// Loads the default hpsettings.json file to the builder.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IConfigurationBuilder AddHPCoreSettingsConfiguration(this IConfigurationBuilder builder) {
			builder.AddJsonFile("hpsettings.json", optional: false, reloadOnChange: true);
			return builder;
		}
        /// <summary>
        /// Loads the default hpsettings.json file and the corresponding environment instance of the hpsettings.{env}.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        /// <remarks>The naming of the settings json file is case sensitive and should be match the environment name in the ASPNETCORE_ENVIRONMENT variable.</remarks>
        public static IConfigurationBuilder AddHPCoreSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env) {
			AddHPCoreSettingsConfiguration(builder);
            builder.AddJsonFile($"hpsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            return builder;
        }
        /// <summary>
        /// Loads the default appsettings.json file to the builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder) {
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder;
        }
        /// <summary>
        /// Loads the default appsettings.json file and the corresponding environment instance of the appsettings.{env}.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        /// <remarks>The naming of the settings json file is case sensitive and should be match the environment name in the ASPNETCORE_ENVIRONMENT variable.</remarks>
        public static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env) {
            AddAppSettingsConfiguration(builder);
            builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            return builder;
        }
        public static HPConnectionSetting GetHPCorePrimaryConnectionString(this IConfiguration configuration) {
			HPCoreSettings options = new();
			configuration.GetSection(nameof(HPCoreSettings)).Bind(options);
			return options.Connections.FirstOrDefault(x => x.Primary == true);
		}

		public static HPConnectionSetting GetDatabaseConnection(this IConfiguration configuration, string name) {
			HPCoreSettings options = new();
			configuration.GetSection(nameof(HPCoreSettings)).Bind(options);
			return options.Connections.FirstOrDefault(x => x.Name.ToUpper() == name.ToUpper());
		}
	}
}