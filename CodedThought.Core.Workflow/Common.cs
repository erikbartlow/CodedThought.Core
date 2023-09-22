using System.ComponentModel;

namespace CodedThought.Core.Workflow {

	public enum WorkflowType {
		Standalone = 0,
		Instance = 1
	}

	public enum WorkflowSubstepResultCascadeType {
		AnyOneFailsParent = 1,
		AnyOneCompletesParent = 2,
		AllMustCompleteParent = 3,
		AllMustFailParent = 4
	}

	public enum WorkflowExpressionModifier {

		/// <summary>Performs a basic If This Then That function.</summary>
		IFTTT = 1,

		/// <summary>When used in conjunction with the Email modifier informs the process to attach to the email whatever the target is.</summary>
		Attach = 2,

		/// <summary>Performs a set function of the first argument with the value of the second argument.</summary>
		Set = 3,

		/// <summary>Performs a check function much like the Set modifier except returns a true/false if stored value is equal to the second arguement.</summary>
		Check = 4,

		/// <summary>Performs a get function to return the stored value of the passed field.</summary>
		Get = 5,

		/// <summary>Performs an email function.</summary>
		Email = 6,

		/// <summary>Instructs the workflow to wait for when the passed expression is satisfied.</summary>
		Wait = 7,

		/// <summary>Instructs the process to execute the named workflow.</summary>
		WF = 8,

		/// <summary>Performs a zip compression on the target.</summary>
		Zip = 9,

		/// <summary>Instructs the process to iterate through the referenced list.</summary>
		Each = 10,

		/// <summary>A referenced list of work entities.</summary>
		List = 11,

		/// <summary>Performs a conversion of the target to the destination.</summary>
		ConvertTo = 12,

		/// <summary>Performs a Greater Than function against the target based on the passed parameter.</summary>
		gt = 13,

		/// <summary>Performs a Less Than function against the target based on the passed parameter.</summary>
		lt = 14,

		/// <summary>Performs an Equal To function against the target based on the passed parameter.</summary>
		eq = 15,

		/// <summary>Performs a Greater Than or Equal To function agains the target based on the passed parameter.</summary>
		gte = 16,

		/// <summary>Performs a Less Than or Equal To function agains the target based on the passed parameter.</summary>
		lte = 17,

		/// <summary>Performs a Between function against the target based on the two passed parameters.</summary>
		bw = 18,

		/// <summary>Performs a Length function against the target. If no arguements are passed it is the equivalent to checking the target is empty or not.</summary>
		len = 19,

		/// <summary>Special modifier used to set the target to the date value of today.</summary>
		Today = 20,

		/// <summary>Special modifier used to set the target to the date value of the end of the month.</summary>
		EOM = 21,

		/// <summary>Special modifier used to set the target to the date value of end of the current quarter.</summary>
		EOQ = 22,

		/// <summary>Special modifier used to set the target to the date value of the current fiscal year.</summary>
		EOFY = 23,

		/// <summary>Performs a check to see if the target is found in the list of parameters.</summary>
		IN = 24,

		/// <summary>Performs a case statement type switch based on the target value.</summary>
		Switch = 25,

		/// <summary>Performs a GOTO statement that used the argument to locate a specific step.</summary>
		GOTO = 26,

		/// <summary>Ends the workflow at that point.</summary>
		END = 27,

		/// <summary>No modifier selected.</summary>
		None = 28,

		LWDOM = 29,
		LWDOQ = 30,
		LWDOFY = 31,
		FWDOM = 32,
		FWDOQ = 33,
		FWDOFY = 34
	}

	public enum WorkflowResultTypes {

		[Description("No Action Needed")]
		NoAction = 0,

		/// <summary>A new unprocessed workflow step.</summary>
		[Description("New")]
		New = 1,

		/// <summary>An evaluated workflow step pending execution.</summary>
		[Description("Pending")]
		Pending = 2,

		/// <summary>A currently active workflow step.</summary>
		[Description("Active")]
		Active = 3,

		/// <summary>A completed workflow step.</summary>
		[Description("Complete")]
		Complete = 4,

		/// <summary>The workflow step failed.</summary>
		[Description("Failed")]
		Fail = 5
	}

	public static class Common {

		public static WorkflowExpressionModifier ConvertParsedModifierToEnum(string modifier) {
			try {
				WorkflowExpressionModifier _modifier = WorkflowExpressionModifier.None;
				WorkflowExpressionModifier _foundModifier = WorkflowExpressionModifier.None;

				foreach (WorkflowExpressionModifier mod in CodedThought.Core.Validation.Expressions.Extensions.EnumToList<WorkflowExpressionModifier>()) {
					CodedThought.Core.Switch.On(modifier.ToUpper())
						.Case(WorkflowExpressionModifier.IFTTT.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.IFTTT)
						.Case(WorkflowExpressionModifier.Attach.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Attach)
						.Case(WorkflowExpressionModifier.Set.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Set)
						.Case(WorkflowExpressionModifier.Check.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Check)
						.Case(WorkflowExpressionModifier.Get.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Get)
						.Case(WorkflowExpressionModifier.Email.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Email)
						.Case(WorkflowExpressionModifier.Wait.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Wait)
						.Case(WorkflowExpressionModifier.WF.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.WF)
						.Case(WorkflowExpressionModifier.Zip.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Zip)
						.Case(WorkflowExpressionModifier.Each.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Each)
						.Case(WorkflowExpressionModifier.List.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.List)
						.Case(WorkflowExpressionModifier.ConvertTo.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.ConvertTo)
						.Case(WorkflowExpressionModifier.gt.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.gt)
						.Case(WorkflowExpressionModifier.lt.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.lt)
						.Case(WorkflowExpressionModifier.eq.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.eq)
						.Case(WorkflowExpressionModifier.gte.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.gte)
						.Case(WorkflowExpressionModifier.lte.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.lte)
						.Case(WorkflowExpressionModifier.len.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.len)
						.Case(WorkflowExpressionModifier.IN.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.IN)
						.Case(WorkflowExpressionModifier.bw.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.bw)
						.Case(WorkflowExpressionModifier.Today.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Today)
						.Case(WorkflowExpressionModifier.EOM.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.EOM)
						.Case(WorkflowExpressionModifier.EOQ.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.EOQ)
						.Case(WorkflowExpressionModifier.EOFY.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.EOFY)
						.Case(WorkflowExpressionModifier.Switch.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.Switch)
						.Case(WorkflowExpressionModifier.GOTO.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.GOTO)
						.Case(WorkflowExpressionModifier.END.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.END)
						.Case(WorkflowExpressionModifier.None.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.None)
						.Case(WorkflowExpressionModifier.LWDOM.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.LWDOM)
						.Case(WorkflowExpressionModifier.LWDOQ.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.LWDOQ)
						.Case(WorkflowExpressionModifier.LWDOFY.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.LWDOFY)
						.Case(WorkflowExpressionModifier.FWDOM.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.FWDOM)
						.Case(WorkflowExpressionModifier.FWDOQ.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.FWDOQ)
						.Case(WorkflowExpressionModifier.FWDOFY.ToString().ToUpper(), () => _foundModifier = WorkflowExpressionModifier.FWDOFY);
					if (_modifier != _foundModifier) break;
				}

				return _foundModifier;
			} catch (Exception ex) {
				throw ex;
			}
		}
	}
}