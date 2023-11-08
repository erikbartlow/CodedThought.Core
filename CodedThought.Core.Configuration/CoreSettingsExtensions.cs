using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CodedThought.Core.Configuration
{

	public static class CoreSettingsExtensions
	{

		/// <summary>
		/// Loads the default hpsettings.json file to the builder.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		/// <remarks>The default name for the settings file is ctSettings.json</remarks>
		public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder) => builder.AddCoreSettingsConfiguration("ctSettings.json");
		/// <summary>
		/// Loads the default hpsettings.json file to the builder.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, string settingsFileName)
		{
			builder.AddJsonFile(settingsFileName, optional: false, reloadOnChange: true);
			return builder;
		}
		/// <summary>
		/// Loads the default hpsettings.json file and the corresponding environment instance of the hpsettings.{env}.json file.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="env"></param>
		/// <returns></returns>
		/// <remarks>The naming of the settings json file is case sensitive and should be match the environment name in the ASPNETCORE_ENVIRONMENT variable.</remarks>
		public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env) => builder.AddCoreSettingsConfiguration(env, "ctSettings.json");

		/// <summary>
		/// Loads the default hpsettings.json file and the corresponding environment instance of the hpsettings.{env}.json file.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="env"></param>
		/// <returns></returns>
		/// <remarks>The naming of the settings json file is case sensitive and should be match the environment name in the ASPNETCORE_ENVIRONMENT variable.</remarks>
		public static IConfigurationBuilder AddCoreSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env, string settingsFileName)
		{
			AddCoreSettingsConfiguration(builder);
			builder.AddJsonFile($"{settingsFileName}.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
			return builder;
		}
		/// <summary>
		/// Loads the default appsettings.json file to the builder.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder)
		{
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
		public static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder, IHostEnvironment env)
		{
			AddAppSettingsConfiguration(builder);
			builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
			return builder;
		}
		public static ConnectionSetting GetHPCorePrimaryConnectionString(this IConfiguration configuration)
		{
			CoreSettings options = new();
			configuration.GetSection(nameof(CoreSettings)).Bind(options);
			return options.Connections.FirstOrDefault(x => x.Primary == true);
		}

		public static ConnectionSetting GetDatabaseConnection(this IConfiguration configuration, string name)
		{
			CoreSettings options = new();
			configuration.GetSection(nameof(CoreSettings)).Bind(options);
			return options.Connections.FirstOrDefault(x => x.Name.ToUpper() == name.ToUpper());
		}
	}
}