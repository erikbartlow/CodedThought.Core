using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	/// <summary>Summary description for HPFolderException.</summary>
	[Serializable]
	public class HPFolderException : HPApplicationException, IHPBaseException {

		#region Constructors

		/// <summary>Default Constructor</summary>
		public HPFolderException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message"></param>
		public HPFolderException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       </param>
		/// <param name="innerException"></param>
		public HPFolderException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		protected HPFolderException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		#endregion Constructors
	}
}