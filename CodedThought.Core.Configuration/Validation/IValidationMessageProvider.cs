namespace CodedThought.Core.Configuration.Validation
{

    /// <summary>Supplies the text used when a validation expression fails.</summary>
    public interface IValidationMessageProvider
    {

        /// <summary>Gets the configured message for the passed key, falling back to the built in default.</summary>
        string GetMessage(ValidationMessageKeys key);

        /// <summary>Gets the configured message for the passed key.</summary>
        /// <param name="key">    The message key.</param>
        /// <param name="message">The configured message when one was supplied, otherwise the built in default.</param>
        /// <returns><c>true</c> when the message came from configuration rather than the built in default.</returns>
        bool TryGetMessage(ValidationMessageKeys key, out string message);
    }
}
