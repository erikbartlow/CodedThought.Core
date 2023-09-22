namespace CodedThought.Core.Data.ApiServer {

	public class ApiToken {

		public ApiToken(string token) {
			Value = token;
		}

		public String? Value { get; private set; }
	}
}