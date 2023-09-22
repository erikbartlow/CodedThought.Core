using CodedThought.Core.Validation.Exceptions;

namespace CodedThought.Core.Workflow.Exceptions {

	public class MissingParameterException : ValidationException, IValidationException {

		public MissingParameterException() {
		}

		public MissingParameterException(string message) : base(message) {
		}
	}
}