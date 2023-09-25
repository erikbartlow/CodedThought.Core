namespace CodedThought.Core.Exceptions {

	/// <summary>
	/// This class inherits from the BaseApplicationException it benefits from the contextual information provided by the base class exception classes that derive from this class should look identical
	/// except for the name of the class and constructors deriving classes will also most likely provide custom messages within the call to base(message) such as for a logon exception - base:("You
	/// failed to login with the userName: " + message) message having been passed in from calling code as the userName that failed
	/// </summary>
	public interface IBaseException {

		#region Properties

		/// <summary>Gets or sets the message.</summary>
		/// <value>The message.</value>
		String Message { get; }

		/// <summary>Gets or sets the inner exception.</summary>
		/// <value>The inner exception.</value>
		Exception InnerException { get; }

		/// <summary>Gets or sets the drill down depth.</summary>
		/// <value>The drill down depth.</value>
		Int32 DrillDownDepth { get; set; }

		/// <summary>Gets or sets the exception code.</summary>
		/// <value>The exception code.</value>
		CodedThoughtExceptionExtensions.ExceptionCodes ExceptionCode { get; set; }

		#endregion Properties
	}
}