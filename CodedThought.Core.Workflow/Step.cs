using CodedThought.Core.Data;

namespace CodedThought.Core.Workflow {

	public class Step : IStep {

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Step" /> class.</summary>
		public Step() {
			Steps = new List<IStep>();
			Actions = new List<WorkflowAction>();
			CascadeResultType = WorkflowSubstepResultCascadeType.AllMustCompleteParent;
		}

		/// <summary>Initializes a new instance of the <see cref="Step" /> class.</summary>
		/// <param name="expression">The expression.</param>
		public Step(string expression) : this() {
			ExpressionRef = expression;
			Actions = Parser.Instance.ParseExpression(expression);
		}

		/// <summary>Initializes a new instance of the <see cref="Step" /> class.</summary>
		/// <param name="step">The step.</param>
		public Step(object target, IStep step) : this(step.ExpressionRef) {
			ExpressionRef = step.ExpressionRef;
			StepIndex = ((Step)step).StepIndex;
			Target = target;
			// Propagate the target down to the actions.
			step.Actions.ForEach(a => a.Target = target);
			step.Steps.ForEach(s => Steps.Add(new Step(target, s)));
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets or sets the target.</summary>
		/// <value>The target.</value>
		public object Target { get; set; }

		/// <summary>Gets or sets the index of the step.</summary>
		/// <value>The index of the step.</value>
		public int StepIndex { get; set; }

		/// <summary>Gets or sets the step execution order.</summary>
		/// <value>The step order.</value>
		public int ExecutionOrder { get; set; }

		/// <summary>Gets or sets the expression reference.</summary>
		/// <value>The expression reference.</value>
		public string ExpressionRef { get; set; }

		/// <summary>Gets or sets the actions.</summary>
		/// <value>The actions.</value>
		public List<WorkflowAction> Actions { get; set; }

		/// <summary>Gets or sets the steps.</summary>
		/// <value>The steps.</value>
		public List<IStep> Steps { get; set; }

		/// <summary>Gets or sets the result of the action execution.</summary>
		/// <value>The result.</value>
		public WorkflowResult Result { get; set; }

		public WorkflowSubstepResultCascadeType CascadeResultType { get; set; }

		public StepTranslation Translation { get; set; }

		/// <summary>Gets or sets the description for this step.</summary>
		/// <value>The description.</value>
		/// <remarks>If a description is provided it is used during Wordification.</remarks>
		public string Description { get; set; }

		#endregion Properties

		/// <summary>Executes this instance.</summary>
		/// <exception cref="NotImplementedException"></exception>
		public WorkflowResult Execute() {
			WorkflowResult overallResult = new(WorkflowResultTypes.Fail);
			foreach (WorkflowAction action in Actions) {
				if (action.IsTestAction) {
					WorkflowAction trueAction = new();
					WorkflowAction falseAction = new();
					WorkflowResult testResult = new();

					switch (Actions.Count) {
						case 3:
							trueAction = Actions[1];
							falseAction = Actions[2];
							action.WorkflowResult = action.ModifierParameters[0] is ExpressionParameter exp
								? action.Action(Target, action.ModifierParameters)
								: action.Action(action.Target, action.ModifierParameters);

							if (action.WorkflowResult.Result == WorkflowResultTypes.Complete) {
								trueAction.WorkflowResult = trueAction.ModifierParameters[0] is ExpressionParameter exp1
									? trueAction.Action(Target, trueAction.ModifierParameters)
									: trueAction.Action(action.Target, trueAction.ModifierParameters);
								overallResult = trueAction.WorkflowResult;
							} else if (action.WorkflowResult.Result == WorkflowResultTypes.Fail) {
								falseAction.WorkflowResult = falseAction.ModifierParameters[0] is ExpressionParameter exp2
									? falseAction.Action(Target, falseAction.ModifierParameters)
									: falseAction.Action(falseAction.Target, falseAction.ModifierParameters);
								overallResult = falseAction.WorkflowResult;
							}

							break;

						case 2:
						default:
							trueAction = Actions[1];
							action.WorkflowResult = action.ModifierParameters[0] is ExpressionParameter
								? action.Action(Target, action.ModifierParameters)
								: action.Action(action.Target, action.ModifierParameters);

							if (action.WorkflowResult.Result == WorkflowResultTypes.Complete) {
								trueAction.WorkflowResult = trueAction.ModifierParameters[0] is ExpressionParameter exp1
									? trueAction.Action(Target, trueAction.ModifierParameters)
									: trueAction.Action(action.Target, trueAction.ModifierParameters);
								overallResult = trueAction.WorkflowResult;
							}
							break;
					}
					// Exit the for..loop
					break;
				} else {
					action.WorkflowResult = action.ModifierParameters[0] is ExpressionParameter exp
						? action.Action(Target, action.ModifierParameters)
						: action.Action(action.Target, action.ModifierParameters);
					overallResult = action.WorkflowResult;
					switch (overallResult.Result) {
						case WorkflowResultTypes.Fail:
							return overallResult;

						case WorkflowResultTypes.Complete:
						default:
							// Allow the next action to execute.
							continue;
					}
				}
			}
			switch (overallResult.Result) {
				case WorkflowResultTypes.Fail:
					return overallResult;

				case WorkflowResultTypes.Complete:
				default:
					if (Steps.Count > 0) {
						// Allow the next action(s) to execute.
						List<IStep> substeps = Steps.OrderBy(o => o.ExecutionOrder).ToList();
						substeps.ForEach(s => overallResult = s.Result = s.Execute());
						// Determine the overall result based on the cascade type.
						overallResult = new WorkflowResult(DetermineOverallResult(CascadeResultType));
					}
					break;
			}
			return overallResult;
		}

		public StepTranslation Wordify() {
			StepTranslation overallTranslation = new();
			overallTranslation.WorkflowExpression = this.ExpressionRef;
			if (!String.IsNullOrEmpty(Description)) {
				overallTranslation.Translation = Description;
			} else {
				foreach (WorkflowAction action in Actions) {
					StepTranslation actionTranslation = new(action.ExpressionRef);
					action.WorkflowTranslation = action.ModifierParameters[0] is ExpressionParameter exp
						? action.Translation(Target, action.ModifierParameters)
						: action.Translation(action.Target, action.ModifierParameters);
					overallTranslation.Translations.Add(action.WorkflowTranslation);
				}
				if (Actions.Count == 1 && overallTranslation.Translations.Count == 1) {
					StepTranslation thisTranslation = overallTranslation.Translations[0];
					overallTranslation.Translation = thisTranslation.Translation;
					overallTranslation.Translations.Clear();
				} else if (Actions.Count == 2 && overallTranslation.Translations.Count == 2) {
					if (Actions[0].IsTestAction) {
						StepTranslation thisTranslation = overallTranslation.Translations[0];
						overallTranslation.Translation = thisTranslation.Translation;
						overallTranslation.Translations.RemoveAt(0);
					}
				} else
					overallTranslation = new StepTranslation();
			}
			if (Steps.Count > 0) {
				// Allow the next action(s) to execute.
				List<IStep> substeps = Steps.OrderBy(o => o.ExecutionOrder).ToList();
				substeps.ForEach(s => overallTranslation.Translations.Add(s.Wordify()));
			}
			Translation = overallTranslation;
			return Translation;
		}

		public WorkflowResultTypes DetermineOverallResult(WorkflowSubstepResultCascadeType cascadeType) {
			WorkflowResultTypes overallResult = Result.Result;
			if (Steps.Count > 0) {
				// Get the result of any sub steps.
				Steps.ForEach(s => overallResult = s.DetermineOverallResult(cascadeType));

				switch (cascadeType) {
					case WorkflowSubstepResultCascadeType.AnyOneFailsParent:
						return Steps.Any(s => (s.Result.Result == WorkflowResultTypes.Fail)) ? WorkflowResultTypes.Fail : WorkflowResultTypes.Complete;
					case WorkflowSubstepResultCascadeType.AnyOneCompletesParent:
						return Steps.Any(s => (s.Result.Result == WorkflowResultTypes.Complete)) ? WorkflowResultTypes.Complete : WorkflowResultTypes.Fail;
					case WorkflowSubstepResultCascadeType.AllMustCompleteParent:
						return Steps.All(s => (s.Result.Result == WorkflowResultTypes.Complete)) ? WorkflowResultTypes.Complete : WorkflowResultTypes.Fail;
					case WorkflowSubstepResultCascadeType.AllMustFailParent:
						return Steps.All(s => (s.Result.Result == WorkflowResultTypes.Fail)) ? WorkflowResultTypes.Complete : WorkflowResultTypes.Fail;
				}
			}

			return overallResult;
		}
	}
}