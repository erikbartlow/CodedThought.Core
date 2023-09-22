namespace CodedThought.Core.Configuration {

	public sealed class ValidationMessageElement {
		public string Key { get; set; }
		public string Value { get; set; }

		public ValidationMessageElement() {
			Key = string.Empty;
			Value = string.Empty;
		}

		public ValidationMessageElement(string key, string value) {
			this.Key = key;
			Value = value;
		}
	}
}