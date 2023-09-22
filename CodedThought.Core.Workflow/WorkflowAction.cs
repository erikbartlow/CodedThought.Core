namespace CodedThought.Core.Workflow {

	public class WorkflowAction : IAction {
		private WorkflowResult _result;
		private StepTranslation _translation;

		/// <summary>Gets or sets the target.</summary>
		/// <value>The target.</value>
		public object Target { get; set; }

		/// <summary>Gets or sets the expression reference.</summary>
		/// <value>The expression reference.</value>
		public string ExpressionRef { get; set; }

		/// <summary>Gets or sets a value indicating whether this <see cref="WorkflowAction" /> applies a "Not" to the action.</summary>
		/// <value><c>true</c> if negative; otherwise, <c>false</c>.</value>
		public bool Negative { get; set; }

		public bool IsTestAction { get; set; }

		/// <summary>Gets the get modifier.</summary>
		/// <value>The get modifier.</value>
		public WorkflowExpressionModifier Modifier { get; set; }

		/// <summary>Gets or sets the validation result.</summary>
		/// <value>The validation result.</value>
		public WorkflowResult WorkflowResult {
			get {
				if (Negative) {
					switch (_result.Result) {
						case WorkflowResultTypes.Complete:
							_result.Result = WorkflowResultTypes.Fail;
							break;

						case WorkflowResultTypes.Fail:
							_result.Result = WorkflowResultTypes.Complete;
							break;
					}
				}
				return _result;
			}
			set {
				_result = value;
			}
		}

		public StepTranslation WorkflowTranslation {
			get {
				if (Negative && _translation != null) {
					_translation.Translation = $"NOT {_translation.Translation}";
				}
				return _translation;
			}
			set {
				_translation = value;
			}
		}

		/// <summary>Gets or sets the modifier parameters for the action.</summary>
		/// <value>The modifier parameters.</value>
		public List<object> ModifierParameters { get; set; }

		/// <summary>Gets or sets the exceptions if the <c>Test</c> method results in a <c>false</c>.</summary>
		/// <value>The exceptions.</value>
		public List<Exceptions.WorkflowException> Exceptions { get; set; }

		public Func<object, List<object>, WorkflowResult> Action { get; set; }

		public Func<object, List<object>, StepTranslation> Translation { get; set; }

		public void SetAction(Func<object, List<object>, WorkflowResult> action) {
			try {
				Action = action;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Executes the action delegate.</summary>
		/// <param name="action">The action.</param>
		public WorkflowResult ExecuteAction() {
			try {
				WorkflowResult = Action(Target, ModifierParameters);
				return WorkflowResult;
			} catch (Exception ex) {
				throw ex;
			}
		}

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="WorkflowAction" /> class.</summary>
		/// <remarks>Not setting the target will instruct the expression to use the value being validated.</remarks>
		public WorkflowAction() {
			Exceptions = new List<Exceptions.WorkflowException>();
			WorkflowResult = new WorkflowResult(WorkflowResultTypes.New);
			Negative = false;
			IsTestAction = false;
		}

		/// <summary>Initializes a new instance of the <see cref="WorkflowAction" /> class.</summary>
		/// <param name="expressionRef">The expression reference.</param>
		public WorkflowAction(String expressionRef) : this() {
			ExpressionRef = expressionRef;
		}

		/// <summary>Initializes a new instance of the <see cref="WorkflowAction" /> class.</summary>
		/// <param name="expression">The expression.</param>
		/// <param name="modifier">  The modifier.</param>
		/// <param name="parameters">The parameters.</param>
		public WorkflowAction(string expression, WorkflowExpressionModifier modifier, params object[] parameters) : this(expression) {
			Modifier = modifier;
			ModifierParameters = parameters.ToList();
		}

		#endregion Constructors
	}
}