using CodedThought.Core.Validation.Exceptions;

namespace CodedThought.Core.Validation.Expressions {

	public class ValidationResult {
		public ValidationResultTypes Result { get; set; }

		public ValidationException Exception { get; set; }
	}
}