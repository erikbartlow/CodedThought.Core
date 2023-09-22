namespace CodedThought.Core.Data.ExcelSystem.Exceptions {

	public class BiffRecordException : Exception {

		public BiffRecordException() {
		}

		public BiffRecordException(string message)
			: base(message) {
		}

		public BiffRecordException(string message, Exception innerException)
			: base(message, innerException) {
		}
	}
}