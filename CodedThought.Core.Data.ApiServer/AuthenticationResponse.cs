using Newtonsoft.Json;

namespace CodedThought.Core.Data.ApiServer {

	internal class AuthenticationResponse : BaseResponse {
		public AuthenticationResponse() { Data = new(); }
		internal class ResponseData {
			public ResponseData() { ApiToken = string.Empty; }

			[JsonProperty("apiToken")]
			public string ApiToken { get; set; }
		}

		[JsonProperty("data")]
		public ResponseData Data { get; set; }
	}
}