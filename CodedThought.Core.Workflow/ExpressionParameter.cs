namespace CodedThought.Core.Workflow {

	public class ExpressionParameter {
		public string Target { get; set; }

		public WorkflowAction Action { get; set; }

		public WorkflowExpressionModifier Modifier { get; set; }

		public string ObjectClass { get; set; }

		public string ObjectClassProperty { get; set; }
	}
}