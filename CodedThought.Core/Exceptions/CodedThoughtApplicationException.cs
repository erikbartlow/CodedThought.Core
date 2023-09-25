using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	/// <summary>
	/// This class inherits from the BaseApplicationException it benefits from the contextual information provided by the base class exception classes that derive from this class should look identical
	/// except for the name of the class and constructors deriving classes will also most likely provide custom messages within the call to base(message) such as for a logon exception - base:("You
	/// failed to login with the userName: " + message) message having been passed in from calling code as the userName that failed
	/// </summary>
	[Serializable]
	public class CodedThoughtApplicationException : ApplicationException, IBaseException {

		#region Declaration

		public CodedThoughtExceptionExtensions.ExceptionCodes _code;
		private Int32 _drillDownDepth = 3;

		#endregion Declaration

		#region Properties

		/// <summary>Gets a message that describes the current exception.</summary>
		/// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
		public new String Message {
			get { return base.Message; }
		}

		/// <summary>Gets the <see cref="T:System.Exception" /> instance that caused the current exception.</summary>
		/// <returns>
		/// An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null
		/// reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.
		/// </returns>
		public new Exception InnerException {
			get { return base.InnerException; }
		}

		/// <summary>Gets or sets the exception code.</summary>
		/// <value>The exception code.</value>
		public CodedThoughtExceptionExtensions.ExceptionCodes ExceptionCode {
			get { return _code; }
			set { _code = value; }
		}

		/// <summary>Gets or sets the drill down depth for inner exceptions when getting messages.</summary>
		/// <value>The drill down depth.</value>
		public Int32 DrillDownDepth {
			get { return _drillDownDepth; }
			set { _drillDownDepth = value; }
		}

		#endregion Properties

		#region Constructors

		/// <summary>Default Constructor</summary>
		public CodedThoughtApplicationException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message"></param>
		public CodedThoughtApplicationException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       </param>
		/// <param name="innerException"></param>
		public CodedThoughtApplicationException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		protected CodedThoughtApplicationException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		#endregion Constructors
	}
}