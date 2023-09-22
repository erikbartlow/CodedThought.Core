namespace CodedThought.Core.Validation.Exceptions {

	/// <summary>Base exception class for the SmartUpload Validation component.</summary>
	public class ExpressionException : ValidationException {

		/// <summary>Gets or sets the row in error.</summary>
		/// <value>The row in error.</value>
		public int RowInError { get; set; }

		/// <summary>Gets or sets the column in error.</summary>
		/// <value>The column in error.</value>
		public int ColumnInError { get; set; }

		/// <summary>Gets or sets the value that caused the error.</summary>
		/// <value>The value in error.</value>
		public object ValueInError { get; set; }

		/// <summary>Gets or sets the base raw expression string.</summary>
		/// <value>The expression.</value>
		public string Expression { get; set; }

		/// <summary>Initializes a new instance of the <see cref="ExpressionException" /> class.</summary>
		public ExpressionException() : base() { Expression = String.Empty; }

		/// <summary>Initializes a new instance of the <see cref="ExpressionException" /> class.</summary>
		/// <param name="message">The message that describes the error.</param>
		public ExpressionException(string message) : base(message) { Expression = string.Empty; }

		/// <summary>Initializes a new instance of the <see cref="ExpressionException" /> class.</summary>
		/// <param name="expression">The expression.</param>
		public ExpressionException(string message, string expression) : this(message) { Expression = expression; }

		/// <summary>Initializes a new instance of the <see cref="ExpressionException" /> class.</summary>
		/// <param name="message">   The message.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="row">       The row.</param>
		/// <param name="col">       The col.</param>
		/// <param name="value">     The value.</param>
		public ExpressionException(string message, string expression, int row, int col, object value) : this(message, expression) {
			RowInError = row;
			ColumnInError = col;
			ValueInError = value;
		}
	}
}