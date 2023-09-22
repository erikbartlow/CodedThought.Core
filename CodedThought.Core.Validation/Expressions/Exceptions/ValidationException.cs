namespace CodedThought.Core.Validation.Exceptions {

	public class ValidationException : Exception, IValidationException {

		public ValidationException() {
		}

		public ValidationException(string message) : base(message) {
		}

		public ValidationResultTypes ResultType { get; set; }
	}
}