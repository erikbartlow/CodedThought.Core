namespace CodedThought.Core.Validation.Exceptions {

	public interface IValidationException {
		ValidationResultTypes ResultType { get; set; }
	}
}