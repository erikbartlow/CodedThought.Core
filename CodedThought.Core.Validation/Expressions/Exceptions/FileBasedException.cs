namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Uploaded file based exception class for the SmartUpload Validation component.</summary>
	public class FileBasedException : ExpressionException {

		/// <summary>Gets or sets the name of the file.</summary>
		/// <value>The name of the file.</value>
		public string FileName { get; set; }

		/// <summary>Initializes a new instance of the <see cref="FileBasedException" /> class.</summary>
		public FileBasedException() : base() { }

		/// <summary>Initializes a new instance of the <see cref="FileBasedException" /> class.</summary>
		/// <param name="fileName">Name of the file.</param>
		public FileBasedException(string message) : base(message) { }

		/// <summary>Initializes a new instance of the <see cref="FileBasedException" /> class.</summary>
		/// <param name="fileName">Name of the file.</param>
		public FileBasedException(string message, string fileName) : this(message) { FileName = fileName; }
	}
}