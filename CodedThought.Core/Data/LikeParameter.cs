using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data {

	public class LikeParameter : Parameter, IParameter {

		public LikeParameter() {
		}

		public LikeParameter(IDataParameter baseParam)
			: base(baseParam) {
		}

		/// <summary>Gets or sets the wildcard character.</summary>
		/// <value>The wildcard character.</value>
		public string WildcardCharacter { get; set; }

		public override string ToParameterString(string ParameterConnector, bool firstInGroup = true) => $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} LIKE '{WildcardCharacter}{_dbParam.Value}{WildcardCharacter}'";
	}
}