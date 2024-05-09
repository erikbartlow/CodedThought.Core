using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
namespace CodedThought.Core.Configuration {
	public static class ConfigurationFactory {


		/// <summary>
		/// Use for .NET Core Console applications.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="env"></param>
		/// <returns></returns>
		private static IConfigurationBuilder Configure(IConfigurationBuilder config, Microsoft.Extensions.Hosting.IHostEnvironment env) {
			return Configure(config, env.EnvironmentName);
		}

        private static IConfigurationBuilder Configure(IConfigurationBuilder config, string environmentName) => config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddJsonFile("ctsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"ctsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();

        /// <summary>
        /// Use for .NET Core Console applications.
        /// </summary>
        /// <returns></returns>
        public static IConfiguration CreateConfiguration() {
            HostingEnvironment env = new()
            {
				EnvironmentName = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT") ?? "Production",
				ApplicationName = AppDomain.CurrentDomain.FriendlyName,
				ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
				ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
			};

            ConfigurationBuilder config = new();
            IConfigurationBuilder configured = Configure(config, env);
			return configured.Build();
		}
	}
}
