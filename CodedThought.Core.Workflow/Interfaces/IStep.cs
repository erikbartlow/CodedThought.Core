namespace CodedThought.Core.Workflow {

	public interface IStep {
		int StepIndex { get; set; }

		/// <summary>Gets or sets the step execution order.</summary>
		/// <value>The step order.</value>
		int ExecutionOrder { get; set; }

		/// <summary>Gets or sets the expression reference.</summary>
		/// <value>The expression reference.</value>
		string ExpressionRef { get; set; }

		/// <summary>Gets or sets the actions.</summary>
		/// <value>The actions.</value>
		List<WorkflowAction> Actions { get; set; }

		/// <summary>Gets or sets the steps.</summary>
		/// <value>The steps.</value>
		List<IStep> Steps { get; set; }

		/// <summary>Executes this instance.</summary>
		/// <returns></returns>
		WorkflowResult Execute();

		/// <summary>Wordifies this instance using the translation engine.</summary>
		/// <returns></returns>
		StepTranslation Wordify();

		/// <summary>Gets or sets the result.</summary>
		/// <value>The result.</value>
		WorkflowResult Result { get; set; }

		/// <summary>Gets or sets the translation.</summary>
		/// <value>The translation.</value>
		StepTranslation Translation { get; set; }

		/// <summary>Gets or sets the type of the cascade result.</summary>
		/// <value>The type of the cascade result.</value>
		WorkflowSubstepResultCascadeType CascadeResultType { get; set; }

		/// <summary>Determines the overall result.</summary>
		/// <param name="cascadeType">Type of the cascade.</param>
		/// <returns></returns>
		WorkflowResultTypes DetermineOverallResult(WorkflowSubstepResultCascadeType cascadeType);
	}
}