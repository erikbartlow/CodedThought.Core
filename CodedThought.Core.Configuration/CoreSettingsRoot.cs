namespace CodedThought.Core.Configuration {

	public class CoreSettingsRoot {
		public required Dictionary<string, string> Settings { get; set; }
		public required Dictionary<string, ConnectionSetting> Connections { get; set; }
	}
}