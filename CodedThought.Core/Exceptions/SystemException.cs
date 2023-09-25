using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	/// <summary>
	/// This class inherits from the BaseApplicationException it benefits from the contextual information provided by the base class exception classes that derive from this class should look identical
	/// except for the name of the class and constructors deriving classes will also most likely provide custom messages within the call to base(message) such as for a logon exception - base:("You
	/// failed to login with the userName: " + message) message having been passed in from calling code as the userName that failed
	/// </summary>
	[Serializable]
	public class SystemException : System.SystemException, IBaseException {

		#region Declaration

		public CodedThoughtExceptionExtensions.ExceptionCodes _code;
		private Int32 _drillDownDepth = 3;

		#endregion Declaration

		#region Properties

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

		#region Methods

		/// <summary>Gets the exception messages.</summary>
		/// <param name="ex">The ex.</param>
		/// <returns></returns>
		private string GetExceptionMessage(Exception ex) {
			StringBuilder sb = new();
			sb.Append(ex.Message);
			if (ex.InnerException != null) {
				sb.AppendFormat("\r\n\r\nInner Exception:  {0}", ex.InnerException.Message);
				if (ex.InnerException.InnerException != null) {
					sb.AppendFormat("\r\n\r\nInner Exception:  {0}", ex.InnerException.InnerException.Message);
				}
			}
			return sb.ToString();
		}

		#endregion Methods

		#region Constructors

		/// <summary>Default Constructor</summary>
		public SystemException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message">The message.</param>
		public SystemException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public SystemException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		/// <param name="info">   The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null.</exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0).</exception>
		protected SystemException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		/// <summary>Initializes a new instance of the <see cref="CodedThoughtApplicationException" /> class.</summary>
		/// <param name="message">The message.</param>
		/// <param name="code">   The code.</param>
		public SystemException(string message, CodedThoughtExceptionExtensions.ExceptionCodes code)
			: base(message) {
			this._code = code;
		}

		/// <summary>Initializes a new instance of the <see cref="CodedThoughtApplicationException" /> class.</summary>
		/// <param name="message">       The message.</param>
		/// <param name="innerException">The inner exception.</param>
		/// <param name="code">          The code.</param>
		public SystemException(String message, Exception innerException, CodedThoughtExceptionExtensions.ExceptionCodes code)
			: base(message, innerException) {
			this._code = code;
		}

		/// <summary>Returns a <see cref="System.String" /> that represents this instance and any inner exceptions thay may exist.</summary>
		/// <returns>A <see cref="System.String" /> that represents this instance and any inner exceptions thay may exist.</returns>
		/// <PermissionSet>
		/// <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
		/// </PermissionSet>
		public override string ToString() {
			return GetExceptionMessage(this);
		}

		#endregion Constructors
	}
}