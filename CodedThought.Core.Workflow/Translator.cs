namespace CodedThought.Core.Workflow {

	public class Translator : ITranslator {

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Translator" /> class.</summary>
		public Translator() {
			WorkflowSteps = new List<Step>();
			SetTranslations();
		}

		/// <summary>Initializes a new instance of the <see cref="Translator" /> class.</summary>
		/// <param name="stepsToTranslate">The steps to translate.</param>
		public Translator(List<Step> stepsToTranslate) : this() => WorkflowSteps = stepsToTranslate;

		#endregion Constructors

		#region Properties

		public string WorkflowName { get; set; }

		/// <summary>Gets or sets the workflow steps.</summary>
		/// <value>The workflow steps.</value>
		public List<Step> WorkflowSteps { get; set; }

		/// <summary>Gets the translations.</summary>
		/// <value>The translations.</value>
		public List<StepTranslation> Translations { get; set; }

		/// <summary>Gets or sets the modifier translations.</summary>
		/// <value>The modifier translations.</value>
		public List<ModifierTranslation> ModifierTranslations { get; set; }

		#endregion Properties

		#region Methods

		/// <summary>Sets the translations.</summary>
		public void SetTranslations() {
			ModifierTranslations = new List<ModifierTranslation>();
			foreach (WorkflowExpressionModifier modifier in Enum.GetValues(typeof(WorkflowExpressionModifier))) {
				ModifierTranslation thisTranslation = new();
				switch (modifier) {
					/// <summary>Performs a basic If This Then That function.</summary>
					case WorkflowExpressionModifier.IFTTT:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} Then {}" });
						break;
					/// <summary>When used in conjunction with the Email modifier informs the process to attach to the email whatever the target is.</summary>
					case WorkflowExpressionModifier.Attach:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Attach" });
						break;
					/// <summary>Peforms a set function of the first arguement with the value of the second arguement.</summary>
					case WorkflowExpressionModifier.Set:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Set" });
						break;
					/// <summary>Performs a check function much like the Set modifier except returns a true/false if stored value is equal to the second arguement.</summary>
					case WorkflowExpressionModifier.Check:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Check" });
						break;
					/// <summary>Performs a get function to return the stored value of the passed field.</summary>
					case WorkflowExpressionModifier.Get:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Get" });
						break;
					/// <summary>Peforms an email function.</summary>
					case WorkflowExpressionModifier.Email:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Send Email" });
						break;
					/// <summary>Instructs the workflow to wait for when the passed expression is satisfied.</summary>
					case WorkflowExpressionModifier.Wait:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Wait for" });
						break;
					/// <summary>Instructs the process to execute the named workflow.</summary>
					case WorkflowExpressionModifier.WF:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Execute Workflow" });
						break;
					/// <summary>Performs a zip compression on the target.</summary>
					case WorkflowExpressionModifier.Zip:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Zip Files" });
						break;
					/// <summary>Instructs the process to iterate through the referenced list.</summary>
					case WorkflowExpressionModifier.Each:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "For Each" });
						break;
					/// <summary>A referenced list of work entities.</summary>
					case WorkflowExpressionModifier.List:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "List" });
						break;
					/// <summary>Performs a conversion of the target to the destination.</summary>
					case WorkflowExpressionModifier.ConvertTo:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Convert {} to {}" });
						break;
					/// <summary>Performs a Greater Than function against the target based on the passed parameter.</summary>
					case WorkflowExpressionModifier.gt:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} is greater than {}" });
						break;
					/// <summary>Performs a Greater Than or Equal to function against the target based on the passed parameter.</summary>
					case WorkflowExpressionModifier.gte:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} is greater than or equal to {}" });
						break;
					/// <summary>Performs a Less Than or Equal to function against the target based on the passed parameter.</summary>
					case WorkflowExpressionModifier.lte:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} is greater than or equal to {}" });
						break;
					/// <summary>Performs a Less Than function against the target based on the passed parameter.</summary>
					case WorkflowExpressionModifier.lt:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} is less than {}" });
						break;
					/// <summary>Performs an Equal To function against the target based on the passed parameter.</summary>
					case WorkflowExpressionModifier.eq:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} is euqal to {}" });
						break;
					/// <summary>Performs a Between function against the target based on the two passed parameters.</summary>
					case WorkflowExpressionModifier.bw:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} is between {} and {}" });
						break;
					/// <summary>Performs a Length function against the target. If no arguements are passed it is the equivalent to checking the target is empty or not.</summary>
					case WorkflowExpressionModifier.len:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "If {} has a length at least {}" });
						break;
					/// <summary>Special modifier used to set the target to the date value of today.</summary>
					case WorkflowExpressionModifier.Today:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Today" });
						break;
					/// <summary>Special modifier used to set the target to the date value of the end of the month.</summary>
					case WorkflowExpressionModifier.EOM:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "End of the Month" });
						break;
					/// <summary>Special modifier used to set the target to the date value of end of the current quarter.</summary>
					case WorkflowExpressionModifier.EOQ:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "End of the Quarter" });
						break;
					/// <summary>Special modifier used to set the target to the date value of the current fiscal year.</summary>
					case WorkflowExpressionModifier.EOFY:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "End of the Fiscal Year" });
						break;
					/// <summary>Performs a check to see if the target is found in the list of parameters.</summary>
					case WorkflowExpressionModifier.IN:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Values are one of these {}" });
						break;
					/// <summary>Performs a case statement type switch function against the target based on the passed parameter.</summary>
					case WorkflowExpressionModifier.Switch:
						ModifierTranslations.Add(new ModifierTranslation() { Modifier = modifier, Translation = "Apply the right case depending on {} as the value" });
						break;

					/// <summary>No modifier selected.</summary>
					case WorkflowExpressionModifier.None:
						break;
				}
			}
		}

		#endregion Methods
	}
}