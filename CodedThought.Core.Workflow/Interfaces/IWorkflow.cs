namespace CodedThought.Core.Workflow {

	public interface IWorkflow {
		string Name { get; set; }

		List<Step> Steps { get; set; }
	}
}