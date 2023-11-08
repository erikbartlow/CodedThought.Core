using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

using System.Text.Json;

namespace CodedThought.Core.Configuration.Validation
{

	public enum ValidationMessageKeys
	{
		Required, Equals, GreaterThan, GreaterThanEqTo, LessThan, LessThanEqTo, NotEqual, InvalidEmail, NotInList, NotBetween, NotUpper, NotLower, ExceedsMax, MinimumNotReached
	}

	public class ValidationConfigurationProvider : IConfigurationProvider
	{
		private readonly IConfiguration _configuration;
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

		public ValidationConfigurationProvider()
		{
			ValidationMessages = new();
		}

		private List<ValidationMessageElement> ValidationMessages { get; set; }

		public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
		{
			throw new NotImplementedException();
		}

		public IChangeToken GetReloadToken()
		{
			throw new NotImplementedException();
		}

		public void Load()
		{
			IConfigurationSection messagesSection = _configuration.GetSection(VALIDATION_MESSAGE_CONFIG_KEY);
			IEnumerable<IConfigurationSection> msgArray = messagesSection.GetChildren();
			foreach (var msg in msgArray)
			{
				if (Enum.TryParse<ValidationMessageKeys>(msg.Key, out ValidationMessageKeys currentKey))
				{
					ValidationMessages.Add(new(msg.Key, String.IsNullOrEmpty(msg.Value) ? GetDefaultValidationMessage(currentKey) : msg.Value));
				}
			}
		}

		public void Set(string key, string value)
		{
			throw new NotImplementedException();
		}

		public bool TryGet(string key, out string value)
		{
			ValidationMessageElement val = ValidationMessages.FirstOrDefault<ValidationMessageElement>(v => v.Key == key);
			if (val == null)
			{
				string defaultMsg = GetDefaultValidationMessage(Enum.Parse<ValidationMessageKeys>(key));
				value = defaultMsg;
			} else
			{
				value = val.Value;
			}
			// We always return true here since the default message will be returned if a custom one isn't found in the configuration file.
			return true;
		}

		public string GetMessage(ValidationMessageKeys key)
		{
			string msg;
			TryGet(key.ToString(), out msg);
			return msg;
		}

		private string GetDefaultValidationMessage(ValidationMessageKeys messageKey)
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