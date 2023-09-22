using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	/// <summary>Summary description for HPDuplicateException.</summary>
	[Serializable]
	public class HPDuplicateException : HPApplicationException, IHPBaseException {

		#region Constructors

		/// <summary>Default Constructor</summary>
		public HPDuplicateException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message"></param>
		public HPDuplicateException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       </param>
		/// <param name="innerException"></param>
		public HPDuplicateException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		protected HPDuplicateException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		#endregion Constructors
	}
}