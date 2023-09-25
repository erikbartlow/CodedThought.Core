namespace CodedThought.Core.Configuration {

	public class HPCoreRoot {
		public Dictionary<string, string> Settings { get; set; }
		public Dictionary<string, ConnectionSetting> Connections { get; set; }
	}
}