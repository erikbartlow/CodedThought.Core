using Microsoft.Extensions.Configuration;

namespace CodedThought.Core.Configuration.Validation
{

    public enum ValidationMessageKeys
    {
        Required, Equals, GreaterThan, GreaterThanEqTo, LessThan, LessThanEqTo, NotEqual, InvalidEmail, NotInList, NotBetween, NotUpper, NotLower, ExceedsMax, MinimumNotReached
    }

    /// <summary>Reads the configured validation messages, falling back to the built in defaults for any key not supplied.</summary>
    public class ValidationConfigurationProvider : IValidationMessageProvider
    {
        private readonly IConfiguration? _configuration;
        private readonly Dictionary<ValidationMessageKeys, string> _messages = [];
        private const string VALIDATION_MESSAGE_CONFIG_KEY = "AppSettings:CodedThoughtValidationMessages";
        private const string REQUIRED_MESSAGE = "This value is required.";
        private const string EQUALS_MESSAGE = "The target value does not equal comparison value.";
        private const string GREATERTHAN_MESSAGE = "The value provided is not greater than the expected value.";
        private const string GREATERTHAN_OR_EQUALTO_MESSAGE = "The value provided is neither greater than or equal to the expected value.";
        private const string LESSTHAN_MESSAGE = "The value provided is not less than the expected value.";
        private const string LESSTHAN_OR_EQUALTO_MESSAGE = "The value provided is neither less than or equal to the expected value.";
        private const string NOTEQUALTO_MESSAGE = "The value provided is not equal to the expected value.";
        private const string INVALIDEMAIL_MESSAGE = "The email message provided is not a valid email address.";
        private const string NOTINLIST_MESSAGE = "The value provided is not within the expected list of values.";
        private const string NOTBETWEEN_MESSAGE = "The value provided is not between the expected values.";
        private const string NOTUPPER_MESSAGE = "The value provided must be all uppercase.";
        private const string NOTLOWER_MESSAGE = "The value provided must be all lowercase.";
        private const string MAXEXCEEDED_MESSAGE = "The value provided has exceeded the set maximum value.";
        private const string MINIMUMNOTREACHED_MESSAGE = "The value provided is not at or above the minimum value set.";

        /// <summary>Creates a provider that serves only the built in default messages.</summary>
        public ValidationConfigurationProvider() { }

        /// <summary>Creates a provider and reads the configured messages.</summary>
        /// <param name="configuration"></param>
        public ValidationConfigurationProvider(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _configuration = configuration;
            Load();
        }

        /// <summary>Reads the configured messages, replacing anything read previously.</summary>
        public void Load()
        {
            _messages.Clear();
            if (_configuration is null)
                return;

            foreach (IConfigurationSection msg in _configuration.GetSection(VALIDATION_MESSAGE_CONFIG_KEY).GetChildren())
            {
                // Blank values mean "use the default", so they are not recorded.
                if (Enum.TryParse(msg.Key, ignoreCase: true, out ValidationMessageKeys currentKey) && !String.IsNullOrWhiteSpace(msg.Value))
                    _messages[currentKey] = msg.Value;
            }
        }

        public bool TryGetMessage(ValidationMessageKeys key, out string message)
        {
            if (_messages.TryGetValue(key, out string? configured))
            {
                message = configured;
                return true;
            }

            message = GetDefaultValidationMessage(key);
            return false;
        }

        public string GetMessage(ValidationMessageKeys key)
        {
            TryGetMessage(key, out string message);
            return message;
        }

        private static string GetDefaultValidationMessage(ValidationMessageKeys messageKey)
        {
            switch (messageKey)
            {
                case ValidationMessageKeys.Equals:
                    return EQUALS_MESSAGE;

                case ValidationMessageKeys.GreaterThan:
                    return GREATERTHAN_MESSAGE;

                case ValidationMessageKeys.GreaterThanEqTo:
                    return GREATERTHAN_OR_EQUALTO_MESSAGE;

                case ValidationMessageKeys.InvalidEmail:
                    return INVALIDEMAIL_MESSAGE;

                case ValidationMessageKeys.LessThan:
                    return LESSTHAN_MESSAGE;

                case ValidationMessageKeys.LessThanEqTo:
                    return LESSTHAN_OR_EQUALTO_MESSAGE;

                case ValidationMessageKeys.NotBetween:
                    return NOTBETWEEN_MESSAGE;

                case ValidationMessageKeys.NotEqual:
                    return NOTEQUALTO_MESSAGE;

                case ValidationMessageKeys.NotInList:
                    return NOTINLIST_MESSAGE;

                case ValidationMessageKeys.ExceedsMax:
                    return MAXEXCEEDED_MESSAGE;

                case ValidationMessageKeys.MinimumNotReached:
                    return MINIMUMNOTREACHED_MESSAGE;

                case ValidationMessageKeys.NotUpper:
                    return NOTUPPER_MESSAGE;

                case ValidationMessageKeys.NotLower:
                    return NOTLOWER_MESSAGE;

                case ValidationMessageKeys.Required:
                    return REQUIRED_MESSAGE;

                default:
                    return string.Empty;
            }
        }
    }
}