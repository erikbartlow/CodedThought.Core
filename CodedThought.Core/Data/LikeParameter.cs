namespace CodedThought.Core.Data {

	public class LikeParameter : Parameter, IParameter {

		public LikeParameter() {
		}

		public LikeParameter(IDataParameter baseParam)
			: base(baseParam) {
		}

		private string _wildcard;

		/// <summary>Gets or sets the wildcard character.</summary>
		/// <value>The wildcard character.</value>
		public string WildcardCharacter {
			get { return _wildcard; }
			set { _wildcard = value; }
		}

		public override string ToParameterString(string ParameterConnector, bool firstInGroup = true) {
			return $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} LIKE '{_wildcard}{_dbParam.Value}{_wildcard}'";
		}
	}
}