namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Asserts an exception based on a mismatch of column numbers based on the template.</summary>
	public class InvalidTemplateVersionException : FileBasedException {

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateVersionException" /> class.</summary>
		public InvalidTemplateVersionException() : base() { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateColumnCountException" /> class.</summary>
		/// <param name="message"></param>
		public InvalidTemplateVersionException(string message) : base(message) { }

		/// <summary>Initializes a new instance of the <see cref="InvalidTemplateColumnCountException" /> class.</summary>
		/// <param name="message"> </param>
		/// <param name="fileName">Name of the file.</param>
		public InvalidTemplateVersionException(string message, string fileName) : base(message, fileName) { }
	}
}