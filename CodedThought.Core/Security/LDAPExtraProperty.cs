namespace CodedThought.Core.Security {

	[Obsolete("Please use these components from the CodedThought.Core.Security.EnterpriseDirectory library.")]
	public class LDAPExtraProperty {

		public LDAPExtraProperty() {
			Key = string.Empty;
			Value = string.Empty;
		}

		public LDAPExtraProperty(string key, string value) : this() {
			Key = key;
			Value = value;
		}

		public string Key { get; set; }

		public string Value { get; set; }
	}
}