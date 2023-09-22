using Newtonsoft.Json;

namespace CodedThought.Core.Workflow {

	public interface ITranslator {
		List<Step> WorkflowSteps { get; set; }

		List<StepTranslation> Translations { get; set; }

		List<ModifierTranslation> ModifierTranslations { get; set; }

		void SetTranslations();
	}

	[JsonObject("translation")]
	public class StepTranslation {

		public StepTranslation() {
			Translations = new List<StepTranslation>();
			StepResult = new WorkflowResult(WorkflowResultTypes.New);
		}

		public StepTranslation(string expression) : this() {
			WorkflowExpression = expression;
			Parser parser = new();
			ParsedActions = parser.ParseExpression(WorkflowExpression);
		}

		[JsonProperty("expression")]
		public String WorkflowExpression { get; set; }

		[JsonIgnore]
		public List<WorkflowAction> ParsedActions { get; set; }

		[JsonIgnore]
		public WorkflowResult StepResult { get; set; }

		[JsonProperty("translationText")]
		public String Translation { get; set; }

		[JsonProperty("translationState")]
		public String CurrentState => StepResult.Result.ToString();

		[JsonProperty("translationStateValue")]
		public Int32 CurrentStateValue => (int)StepResult.Result;

		[JsonProperty("translations")]
		public List<StepTranslation> Translations { get; set; }
	}

	public class ModifierTranslation {
		public WorkflowExpressionModifier Modifier { get; set; }

		public String Translation { get; set; }
	}
}