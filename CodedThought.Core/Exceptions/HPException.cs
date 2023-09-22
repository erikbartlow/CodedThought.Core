using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	[Serializable]
	public class HPException : Exception, IHPBaseException {

		#region Declaration

		public HPExceptionExtensions.ExceptionCodes _code;
		private Int32 _drillDownDepth = 3;

		#endregion Declaration

		#region Properties

		/// <summary>Gets a message that describes the current exception.</summary>
		/// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
		public new String Message {
			get {
				StringBuilder sb = new StringBuilder();
				sb.Append(base.Message);
				if (base.InnerException != null) {
					sb.AppendFormat("\r\n\tInner Exception:  {0}", base.InnerException.Message);
					if (base.InnerException.InnerException != null) {
						sb.AppendFormat("\r\n\t\tInner Exception:  {0}", base.InnerException.InnerException.Message);
					}
				}
				return sb.ToString();
			}
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
		public HPExceptionExtensions.ExceptionCodes ExceptionCode {
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
		public HPException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message"></param>
		public HPException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       </param>
		/// <param name="innerException"></param>
		public HPException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		protected HPException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		#endregion Constructors
	}
}