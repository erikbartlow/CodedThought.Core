namespace CodedThought.Core.Exceptions {

	/// <summary>Exception class used throw an exception on a missing property or arguement in the system.</summary>
	/// <seealso cref="HPException" />
	/// <seealso cref="IHPBaseException" />
	public class HPMissingArguementException : HPException, IHPBaseException {

		public HPMissingArguementException(string message) : base(message) {
		}
	}
}