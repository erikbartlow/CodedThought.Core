using CodedThought.Core.Workflow.Exceptions;

namespace CodedThought.Core.Workflow {

	public interface IAction {
		Func<object, List<object>, WorkflowResult> Action { get; set; }

		List<WorkflowException> Exceptions { get; set; }

		string ExpressionRef { get; set; }

		bool IsTestAction { get; set; }

		WorkflowExpressionModifier Modifier { get; set; }

		List<object> ModifierParameters { get; set; }

		bool Negative { get; set; }

		object Target { get; set; }

		Func<object, List<object>, StepTranslation> Translation { get; set; }

		WorkflowResult WorkflowResult { get; set; }

		StepTranslation WorkflowTranslation { get; set; }

		WorkflowResult ExecuteAction();

		void SetAction(Func<object, List<object>, WorkflowResult> action);
	}
}