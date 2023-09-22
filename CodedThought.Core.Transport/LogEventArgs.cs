namespace CodedThought.Core.Transport {

	public class LogEventArgs {

		/// <summary>Enum LogLevel</summary>
		public enum LogLevel {

			/// <summary>The debug</summary>
			DEBUG = 0,

			/// <summary>The information</summary>
			INFO = 1,

			/// <summary>The error</summary>
			ERROR = 2
		}

		public LogEventArgs(string? source, string? message, LogLevel level) {
			Source = source;
			Message = message;
			Level = level;
		}

		public LogEventArgs(string? source, string? message, LogLevel level, Exception? exception) {
			Source = source;
			Message = message;
			Level = level;
			Exception = exception;
		}

		public string? Source { get; set; }
		public string? Message { get; set; }
		public LogLevel Level { get; set; }
		public Exception? Exception { get; set; }
	}
}