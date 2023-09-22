namespace CodedThought.Core.Workflow {

	public class Workflow : IWorkflow {

		/// <summary>Initializes a new instance of the <see cref="Workflow" /> class.</summary>
		public Workflow() => Steps = new List<Step>();

		/// <summary>Initializes a new instance of the <see cref="Workflow" /> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="steps"> The steps.</param>
		public Workflow(object target, List<IStep> steps) {
			Target = target;
			steps.ForEach(s => Steps.Add(new Step(target, s)));
		}

		/// <summary>Adds the step.</summary>
		/// <param name="step">The step.</param>
		public void AddStep(Step step) => Steps.Add(step);

		/// <summary>Adds the steps.</summary>
		/// <param name="steps">The steps.</param>
		public void AddSteps(List<Step> steps) => Steps.AddRange(steps);

		/// <summary>Gets or sets the name.</summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>Gets or sets the target.</summary>
		/// <value>The target.</value>
		public object Target { get; set; }

		public WorkflowType TypeOfWorkflow { get; set; }

		/// <summary>Gets or sets the steps.</summary>
		/// <value>The steps.</value>
		public List<Step> Steps { get; set; }
	}
}