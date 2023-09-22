using CodedThought.Core.Validation.Exceptions;

namespace CodedThought.Core.Workflow.Exceptions {

	public class InvalidModelException : ValidationException, IValidationException {

		public InvalidModelException() {
		}

		public InvalidModelException(string message, Type incorrectModel) : base(message) {
			IncorrectModelUsed = incorrectModel;
		}

		public Type IncorrectModelUsed { get; set; }
	}
}