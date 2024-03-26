using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data {

	public class InParameter : Parameter, IParameter {

		public InParameter() : base() => NegateClause = false;

		public InParameter(IDataParameter baseParam)
			: base(baseParam) => NegateClause = false;

		public List<IDataParameter> InParameters { get; set; }
		public List<string> ParamValues;
		public List<string> ParamNames;

		public bool NegateClause { get; set; }

		public override string ToParameterString(string ParameterConnector, bool firstInGroup = true) {
			InParameters = new List<IDataParameter>();
			InParameters.AddRange((List<IDataParameter>)BaseParameter.Value);
			for (int i = 0; i < InParameters.Count; i++) {
				InParameters[i].ParameterName = ParameterConnector + "Arg" + i.ToString();
			}
			return $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} {(NegateClause ? "NOT" : "")} IN({InParameters.Select(p => p.ParameterName).Aggregate((c, n) => c + ", " + n)})";
		}
	}
}