using System.Runtime.Serialization;

namespace CodedThought.Core.Exceptions {

	/// <summary>Summary description for FolderException.</summary>
	[Serializable]
	public class FolderException : CodedThoughtApplicationException, IBaseException {

		#region Constructors

		/// <summary>Default Constructor</summary>
		public FolderException()
			: base() {
		}

		/// <summary>Constructor that takes a message</summary>
		/// <param name="message"></param>
		public FolderException(string message)
			: base(message) {
		}

		/// <summary>Constructor that takes a message and an inner exception</summary>
		/// <param name="message">       </param>
		/// <param name="innerException"></param>
		public FolderException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <summary>Protected constructor to de-serialize data</summary>
		protected FolderException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		#endregion Constructors
	}
}