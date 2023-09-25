namespace CodedThought.Core.Exceptions {

	/// <summary>Exception class used throw an exception on a missing property or arguement in the system.</summary>
	/// <seealso cref="CodedThoughtException" />
	/// <seealso cref="IBaseException" />
	public class MissingArguementException : CodedThoughtException, IBaseException {

		public MissingArguementException(string message) : base(message) {
		}
	}
}