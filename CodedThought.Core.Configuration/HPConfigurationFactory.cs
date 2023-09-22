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

		private static IConfigurationBuilder Configure(IConfigurationBuilder config, string environmentName) {
			return config
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{environmentName}.json", true, true)
				.AddJsonFile("hpsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"hpsettings.{environmentName}.json", true, true)
				.AddEnvironmentVariables();
		}

		/// <summary>
		/// Use for .NET Core Console applications.
		/// </summary>
		/// <returns></returns>
		public static IConfiguration CreateConfiguration() {
			var env = new HostingEnvironment {
				EnvironmentName = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT") ?? "Production",
				ApplicationName = AppDomain.CurrentDomain.FriendlyName,
				ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
				ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
			};

			var config = new ConfigurationBuilder();
			var configured = Configure(config, env);
			return configured.Build();
		}
	}
}
