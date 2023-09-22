using Newtonsoft.Json;

namespace CodedThought.Core.Data.ApiServer {

	internal class BaseResponse {

		[JsonProperty("success")]
		internal bool Success { get; set; }

		[JsonProperty("message")]
		internal string? Message { get; set; }
	}
}