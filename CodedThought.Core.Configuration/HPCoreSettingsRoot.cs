namespace CodedThought.Core.Configuration {

	public class HPCoreRoot {
		public Dictionary<string, string> Settings { get; set; }
		public Dictionary<string, HPConnectionSetting> Connections { get; set; }
	}
}