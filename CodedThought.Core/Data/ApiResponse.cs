using System.Net;

namespace CodedThought.Core.Data {

	public class ApiResponse<T> {

		public ApiResponse() {
			Data = new();
			Value = null;
			StatusCode = HttpStatusCode.OK;
		}

		public ApiResponse(List<T> data, string? message) {
			Data = data;
			Message = message;
			Value = null;
		}

		public List<T> Data { get; set; }
		public object? Value { get; set; }
		public string? Message { get; set; }
		public HttpStatusCode StatusCode { get; set; }
	}
}