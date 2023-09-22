using CodedThought.Core.Validation;
using CodedThought.Core.Validation.Exceptions;

namespace CodedThought.Core.Workflow.Exceptions {

	public class WorkflowException : ValidationException, IValidationException {
		public new ValidationResultTypes ResultType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}