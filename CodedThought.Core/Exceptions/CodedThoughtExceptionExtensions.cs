namespace CodedThought.Core.Exceptions {

	public static class CodedThoughtExceptionExtensions {

		public enum ExceptionCodes {
			ItemNotFound = 0,
			Unathorized = 1,
			DatabaseError = 2,
			Unhandled = 3
		}

		/// <summary>Gets the exception messages.</summary>
		/// <param name="ex">The ex.</param>
		/// <returns></returns>
		public static string GetExceptionMessage<T>(this Exception ex) {
			StringBuilder sb = new();
			sb.Append(ex.Message);
			if (ex.InnerException != null) {
				sb.AppendFormat("\r\n\tInner Exception:  {0}", ex.InnerException.Message);
				if (ex.InnerException.InnerException != null) {
					sb.AppendFormat("\r\n\t\tInner Exception:  {0}", ex.InnerException.InnerException.Message);
				}
			}
			return sb.ToString();
		}

		/// <summary>Gets the exception message recursively including any inner exceptions.</summary>
		/// <param name="ex">The ex.</param>
		/// <returns></returns>
		public static string GetAllMessages(this Exception ex, string separator = "\r\nInnerException: ") => ex.InnerException == null ? ex.Message : ex.Message + separator + GetAllMessages(ex.InnerException, separator);
	}
}