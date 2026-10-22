namespace CodedThought.Core.Configuration {

	public class CoreSettings {

		private const string APPLICATION_COOKIE_NAME_KEY = "ApplicationCookieName";

		public CoreSettings() {
			Connections = [];
			Settings = new(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>Gets or sets the free form application settings.  Keys are matched without regard to case.</summary>
		public Dictionary<string, string> Settings { get; set; }

		/// <summary>Gets or sets the configured database and API connections.</summary>
		public List<ConnectionSetting> Connections { get; set; }

		/// <summary>Gets the configured application cookie name, or an empty string when the setting is absent.</summary>
		public string ApplicationCookieName => TryGetSetting(APPLICATION_COOKIE_NAME_KEY, out string? value) ? value! : string.Empty;

		/// <summary>Gets the setting matching the passed key.</summary>
		/// <param name="key">  The setting key.  Matching ignores case.</param>
		/// <param name="value">The setting value when found.</param>
		/// <returns><c>true</c> when the key is present.</returns>
		public bool TryGetSetting(string key, out string? value) => Settings.TryGetValue(key, out value);

		/// <summary>Gets the setting matching the passed key, or the passed default when the setting is absent.</summary>
		public string GetSetting(string key, string defaultValue = "") => TryGetSetting(key, out string? value) ? value! : defaultValue;
	}
}