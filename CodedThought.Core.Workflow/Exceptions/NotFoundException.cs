using CodedThought.Core.Validation.Exceptions;

namespace CodedThought.Core.Workflow.Exceptions {

	public class NotFoundException : ValidationException, IValidationException {

		public NotFoundException() {
		}

		public NotFoundException(string message) : base(message) {
		}
	}
}