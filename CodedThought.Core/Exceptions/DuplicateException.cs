using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	/// <summary>Summary description for DuplicateException.</summary>
	[Serializable]
	public class DuplicateException : CodedThoughtApplicationException, IBaseException {

		#region Constructors

		/// <summary>Default Constructor</summary>
		public DuplicateException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message"></param>
		public DuplicateException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       </param>
		/// <param name="innerException"></param>
		public DuplicateException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		protected DuplicateException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		#endregion Constructors
	}
}