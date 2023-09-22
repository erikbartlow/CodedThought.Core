namespace CodedThought.Core.Workflow {

	public class WorkflowResult {

		public WorkflowResult() {
		}

		public WorkflowResult(WorkflowResultTypes result) {
			Result = result;
		}

		public WorkflowResultTypes Result { get; set; }

		public Exceptions.WorkflowException Exception { get; set; }
	}
}