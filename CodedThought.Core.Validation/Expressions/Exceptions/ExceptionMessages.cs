using CodedThought.Core.Configuration.Validation;

using Microsoft.Extensions.Configuration;

namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Exposes the validation messages supplied by an <see cref="IValidationMessageProvider" />.</summary>
	/// <remarks>Instances are immutable and safe to share across threads. Resolve one from the container, or use <see cref="Default" /> for the built in messages.</remarks>
	public class ExceptionMessages {
		private readonly IValidationMessageProvider _provider;

		/// <summary>Gets the instance serving the built in default messages.</summary>
		public static ExceptionMessages Default { get; } = new(new ValidationConfigurationProvider());

		public ExceptionMessages(IValidationMessageProvider provider) {
			ArgumentNullException.ThrowIfNull(provider);
			_provider = provider;
		}

		public ExceptionMessages(IConfiguration configuration) : this(new ValidationConfigurationProvider(configuration)) {
		}

		public string RequiredMessage => _provider.GetMessage(ValidationMessageKeys.Required);

		public string EqualsMessage => _provider.GetMessage(ValidationMessageKeys.Equals);

		public string GreaterThanMessage => _provider.GetMessage(ValidationMessageKeys.GreaterThan);

		public string GreaterThanOrEqualToMessage => _provider.GetMessage(ValidationMessageKeys.GreaterThanEqTo);

		public string LessThanMessage => _provider.GetMessage(ValidationMessageKeys.LessThan);

		public string LessThanOrEqualToMessage => _provider.GetMessage(ValidationMessageKeys.LessThanEqTo);

		public string NotEqualToMessage => _provider.GetMessage(ValidationMessageKeys.NotEqual);

		public string InvalidEmailMessage => _provider.GetMessage(ValidationMessageKeys.InvalidEmail);

		public string NotInListMessage => _provider.GetMessage(ValidationMessageKeys.NotInList);

		public string NotBetweenMessage => _provider.GetMessage(ValidationMessageKeys.NotBetween);

		public string NotUpperCaseMessage => _provider.GetMessage(ValidationMessageKeys.NotUpper);

		public string NotLowerCaseMessage => _provider.GetMessage(ValidationMessageKeys.NotLower);

		public string ExceedsMaxMessage => _provider.GetMessage(ValidationMessageKeys.ExceedsMax);

		public string MinimumNotReachedMessage => _provider.GetMessage(ValidationMessageKeys.MinimumNotReached);
	}
}