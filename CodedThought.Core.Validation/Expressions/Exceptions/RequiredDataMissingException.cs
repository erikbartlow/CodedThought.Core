namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Asserts an exception based on a mismatch of column numbers based on the template.</summary>
	public class RequiredDataMissingException : DataBasedException {

		/// <summary>Initializes a new instance of the <see cref="RequiredDataMissingException" /> class.</summary>
		public RequiredDataMissingException() : base() { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateException" /> class.</summary>
		/// <param name="message"></param>
		public RequiredDataMissingException(string message) : base(message) { }
	}
}