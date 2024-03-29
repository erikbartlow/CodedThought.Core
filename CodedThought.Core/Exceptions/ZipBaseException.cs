#if !NETCF_1_0 && !NETCF_2_0

using System.Runtime.Serialization;

#endif

namespace CodedThought.Core.Exceptions {

	/// <summary>ZipBaseException is the base exception class for the SharpZipLibrary. All library exceptions are derived from this.</summary>
	/// <remarks>
	/// NOTE: Not all exceptions thrown will be derived from this class. A variety of other exceptions are possible for example <see cref="ArgumentNullException"></see>
	/// </remarks>
	[Serializable]
	public class ZipBaseException : CodedThoughtApplicationException, IBaseException {

		#region Constructors

		/// <summary>Deserialization constructor</summary>
		/// <param name="info">   <see cref="SerializationInfo" /> for this constructor</param>
		/// <param name="context"><see cref="StreamingContext" /> for this constructor</param>
		protected ZipBaseException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

		/// <summary>Initializes a new instance of the ZipBaseException class.</summary>
		public ZipBaseException() {
		}

		/// <summary>Initializes a new instance of the ZipBaseException class with a specified error message.</summary>
		/// <param name="message">A message describing the exception.</param>
		public ZipBaseException(string message)
			: base(message) {
		}

		/// <summary>Initializes a new instance of the ZipBaseException class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
		/// <param name="message">       A message describing the exception.</param>
		/// <param name="innerException">The inner exception</param>
		public ZipBaseException(string message, Exception innerException)
			: base(message, innerException) {
		}

		#endregion Constructors
	}
}