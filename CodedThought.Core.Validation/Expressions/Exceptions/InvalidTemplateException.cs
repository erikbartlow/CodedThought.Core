namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Asserts an exception based on a mismatch of column numbers based on the template.</summary>
	public class InvalidTemplateException : FileBasedException {

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateException" /> class.</summary>
		public InvalidTemplateException() : base() { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateException" /> class.</summary>
		/// <param name="message"></param>
		public InvalidTemplateException(string message) : base(message) { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateException" /> class.</summary>
		/// <param name="message"> </param>
		/// <param name="fileName">Name of the file.</param>
		public InvalidTemplateException(string message, string fileName) : base(message, fileName) { }
	}
}