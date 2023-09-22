using CodedThought.Core.Configuration.Validation;

namespace CodedThought.Core.Validation.Exceptions {

	public class ExceptionMessages {
		public static ValidationConfigurationProvider ConfigurationProvider { get; set; }

		public ExceptionMessages() {
			ConfigurationProvider = new ValidationConfigurationProvider();
			ConfigurationProvider.Load();
		}

		public static string RequiredMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.Required);

		public static string EqualsMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.Equals);

		public static string GreaterThanMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.GreaterThan);

		public static string GreaterThanOrEqualToMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.GreaterThanEqTo);

		public static string LessThanMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.LessThan);

		public static string LessThanOrEqualToMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.LessThanEqTo);

		public static string NotEqualToMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.NotEqual);

		public static string InvalidEmailMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.InvalidEmail);

		public static string NotInListMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.NotInList);

		public static string NotBetweenMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.NotBetween);

		public static string NotUpperCaseMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.NotUpper);

		public static string NotLowerCaseMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.NotLower);

		public static string ExceedsMaxMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.ExceedsMax);

		public static string MinimumNotReachedMessage => ConfigurationProvider.GetMessage(ValidationMessageKeys.MinimumNotReached);
	}
}