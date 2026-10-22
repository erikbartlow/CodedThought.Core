using CodedThought.Core.Configuration.Validation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CodedThought.Core.Configuration
{

    public static class CoreSettingsServiceCollectionExtensions
    {

        /// <summary>
        /// Registers the CoreSettings section as options and validates it when the host starts.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">The configuration root holding the CoreSettings section.</param>
        /// <returns></returns>
        /// <remarks>
        /// Consumers inject <see cref="IOptionsMonitor{TOptions}" /> to observe changes made to the settings file, or <see cref="IOptionsSnapshot{TOptions}" /> for a value fixed for
        /// the lifetime of a request.
        /// </remarks>
        public static IServiceCollection AddCoreSettings(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<CoreSettings>()
                .Bind(configuration.GetSection(nameof(CoreSettings)))
                .ValidateOnStart();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<CoreSettings>, CoreSettingsValidator>());
            return services;
        }

        /// <summary>
        /// Registers the validation message provider backed by the passed configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <remarks>Messages are read once at registration time; absent keys fall back to the built in defaults.</remarks>
        public static IServiceCollection AddCoreValidationMessages(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.TryAddSingleton<IValidationMessageProvider>(_ => new ValidationConfigurationProvider(configuration));
            return services;
        }
    }
}
