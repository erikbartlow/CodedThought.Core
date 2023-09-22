namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Uploaded file based exception class for the SmartUpload Validation component.</summary>
	public class DataBasedException : ExpressionException {

		/// <summary>Gets or sets the row in error.</summary>
		/// <value>The row in error.</value>
		public new int RowInError { get; set; }

		/// <summary>Gets or sets the column in error.</summary>
		/// <value>The column in error.</value>
		public new int ColumnInError { get; set; }

		/// <summary>Gets or sets the value that caused the error.</summary>
		/// <value>The value in error.</value>
		public new object ValueInError { get; set; }

		/// <summary>Gets or sets the name of the file.</summary>
		/// <value>The name of the file.</value>
		public string FileName { get; set; }

		/// <summary>Gets or sets the data exceptions.</summary>
		/// <value>The data exceptions.</value>
		public List<DataBasedException> DataExceptions { get; set; }

		/// <summary>Initializes a new instance of the <see cref="DataBasedException" /> class.</summary>
		public DataBasedException() : base() { DataExceptions = new List<DataBasedException>(); }

		/// <summary>Initializes a new instance of the <see cref="DataBasedException" /> class.</summary>
		/// <param name="fileName">Name of the file.</param>
		public DataBasedException(string message) : base(message) { DataExceptions = new List<DataBasedException>(); }

		/// <summary>Initializes a new instance of the <see cref="DataBasedException" /> class.</summary>
		/// <param name="fileName">Name of the file.</param>
		public DataBasedException(string message, string expression, int row, int col, object value)
			: this(message) {
			RowInError = row;
			ColumnInError = col;
			ValueInError = value;
		}
	}
}