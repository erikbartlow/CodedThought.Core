namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Asserts an exception based on a mismatch of column numbers based on the template.</summary>
	public class InvalidTemplateNoDataException : FileBasedException {

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateNoDataException" /> class.</summary>
		public InvalidTemplateNoDataException() : base() { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateNoDataException" /> class.</summary>
		/// <param name="message"></param>
		public InvalidTemplateNoDataException(string message) : base(message) { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateNoDataException" /> class.</summary>
		/// <param name="message"> </param>
		/// <param name="fileName">Name of the file.</param>
		public InvalidTemplateNoDataException(string message, string fileName) : base(message, fileName) { }
	}
}