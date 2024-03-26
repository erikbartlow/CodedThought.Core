namespace CodedThought.Core.Configuration {

	public class CoreSettings {

		public CoreSettings() {
			Connections = new();
			Settings = new();
		}

		public required Dictionary<string, string> Settings { get; set; }
		public required List<ConnectionSetting> Connections { get; set; }

		/// <summary>Gets whether this application instance is running on http or local.</summary>
		public Boolean IsWinForm {
			get {
				try {
					bool isWinform = false;
					foreach (KeyValuePair<string, string> setting in Settings) {
						if (setting.Key.ToUpper() == "WINFORM") isWinform = Boolean.Parse(setting.Value);
						break;
					}
					return isWinform;
				} catch {
					return false;
				}
			}
		}

		/// <summary>Gets the configured application cookie name.</summary>
		public string ApplicationCookieName {
			get {
				try {
					string cookieName = string.Empty;
					foreach (KeyValuePair<string, string> setting in Settings) {
						if (setting.Key.ToUpper() == "APPLICATIONCOOKIENAME") cookieName = setting.Value;
						break;
					}
					return cookieName;
				} catch {
					return string.Empty;
				}
			}
		}
	}
}