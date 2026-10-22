using Microsoft.Extensions.Options;

namespace CodedThought.Core.Configuration
{

    /// <summary>Validates the CoreSettings section so a misconfigured host fails at startup rather than on its first query.</summary>
    public sealed class CoreSettingsValidator : IValidateOptions<CoreSettings>
    {

        public ValidateOptionsResult Validate(string? name, CoreSettings options)
        {
            if (options is null)
                return ValidateOptionsResult.Fail($"The {nameof(CoreSettings)} section is missing.");

            List<string> failures = [];

            if (options.Connections.Count == 0)
                failures.Add($"The {nameof(CoreSettings)} section declares no connections.");

            int primaryCount = options.Connections.Count(c => c.Primary);
            if (options.Connections.Count > 0 && primaryCount != 1)
                failures.Add($"Exactly one connection must be marked Primary.  {primaryCount} are.");

            foreach (IGrouping<string, ConnectionSetting> duplicate in options.Connections
                .Where(c => !String.IsNullOrWhiteSpace(c.Name))
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1))
            {
                failures.Add($"The connection name '{duplicate.Key}' is declared {duplicate.Count()} times.  Connection names must be unique.");
            }

            for (int i = 0; i < options.Connections.Count; i++)
            {
                ConnectionSetting connection = options.Connections[i];
                string label = String.IsNullOrWhiteSpace(connection.Name) ? $"Connections[{i}]" : $"Connection '{connection.Name}'";

                if (String.IsNullOrWhiteSpace(connection.Name))
                    failures.Add($"{label} has no Name.  The name is how the framework resolves the connection.");

                if (String.IsNullOrWhiteSpace(connection.ConnectionString))
                    failures.Add($"{label} has no ConnectionString.");

                if (!ConnectionSetting.IsProviderSupported(connection.ProviderType))
                    failures.Add($"{label} declares the unsupported ProviderType '{connection.ProviderType}'.  Supported providers: {string.Join(", ", ConnectionSetting.SupportedProviders)}.");

                if (connection.Timeout < 0)
                    failures.Add($"{label} declares a negative Timeout of {connection.Timeout}.");
            }

            return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
        }
    }
}
