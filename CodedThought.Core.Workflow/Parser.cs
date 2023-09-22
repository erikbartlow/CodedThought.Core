using System.Text.RegularExpressions;

namespace CodedThought.Core.Workflow {

	public class Parser {
		private static Parser _instance;
		private static readonly object Padlock = new();

		/// <summary>Matches the basic structure of an expression to capture the target an the modifiers section.</summary>
		private const string expressionContents = @"\[(?<target>this|.*)\|{1}(?<modifiers>.*)\]";

		/// <summary>Matches an expression with a test/action pair.</summary>
		private const string expressionTestActionModifier = @"(?<test>.*)=>{1}(?<action>.*)";

		/// <summary>Matches an expression with a test/action/else pair.</summary>
		private const string expressionTestActionElseModifier = @"(?<test>.*)=>{1}(?<action>.*)=>{1}(?<actionelse>.*)";

		/// <summary>Matches an expression with an action only. The TestActionModifier expression should be test first since this pattern will match the action within the TestActionModifier</summary>
		//private const string expressionActionModifier = @"(?<action>IFTTT|ATTACH|SET|CHECK|GET|EMAIL|WAIT|WF|ZIP|EACH|LIST)+?\((?<params>(\d+)(,\s*\d+)*)\)";
		private const string expressionActionModifier = @"(?<not>[\!])*(?<action>IFTTT|ATTACH|SET|CHECK|GET|GOTO|END|EMAIL|WAIT|WF|ZIP|EACH|LIST|SWITCH|gt|lt|eq|gte|lte|bw|len|in|EOM|EOQ|EOFY|LWDOM|LWDOQ|LWDOFY|FWDOM|FWDOQ|FWDOFY)\((?<params>.+)\)";

		/// <summary>The expression sub modifier for extra processing like greater than, less than, equal to</summary>
		private const string expressionSubModifier = @"(?<not>[\!])*(?<modifier>gt|lt|eq|gte|lte|bw|len|in|eom|eoq|eofy|LWDOM|LWDOQ|LWDOFY|FWDOM|FWDOQ|FWDOFY)\((?<params>.+)\)";

		private const string expressionActionParameter = @"\[(?<target>.+)\|(?<obj>.+)\.(?<var>.*)\]";

		//private const string expressionMultiModifier = @"([\d]+)\,*\s*([\d]+|.*\({1}[\d\w]+[\,\s\d\w]*\){1})+";
		//private const string expressionParameters = @"(?:\d+|\w+|[^,\s*](?:[a-zA-Z\d]*\(\s?.+?\s?\))|[^,\s*](?:.+\([a-zA-Z\d]*(?:,\s*[a-zA-Z\d]*\)))|\[.+\|.+\..*\])+";
		private const string expressionParameters = @"(?:\d+|\w+|[^,]\s?(?:[a-zA-Z\d]+\(\s?.+?\s?\))|[^,]\s?(?:.+\([a-zA-Z\d]*(?:,\s*[a-zA-Z\d]*\)))|\[[a-zA-Z\d]*\|(?:[a-zA-Z\d]*\.[a-zA-Z\d]*|\[[a-zA-Z\d]*\:[a-zA-Z\d]*\:[a-zA-Z\d]*\])*\])+";

		public static Parser Instance {
			get {
				Parser result;
				lock (Parser.Padlock) {
					Parser arg;
					if ((arg = Parser._instance) == null) {
						arg = (Parser._instance = new Parser());
					}
					result = arg;
				}
				return result;
			}
		}

		public List<WorkflowAction> ParseExpression(string expression) {
			try {
				List<WorkflowAction> wfEx = new();
				String modifierTest, modifierAction, modifierActionElse, modifierNegative;
				bool isNegative = false;
				object[] modifierParameters = new object[0];

				// Check if the modifier consists of a test/action or simply an action.
				Match modifierMatch = Regex.Match(expression.Trim(), expressionTestActionElseModifier);
				if (modifierMatch.Groups.Count > 1) {
					modifierTest = modifierMatch.Groups["test"].Value;
					modifierAction = modifierMatch.Groups["action"].Value;
					modifierActionElse = modifierMatch.Groups["actionelse"].Value;
					wfEx.AddRange(ParseExpression(modifierTest));
					wfEx[0].IsTestAction = true;
					wfEx.AddRange(ParseExpression(modifierAction));
					if (modifierActionElse != null) wfEx.AddRange(ParseExpression(modifierActionElse));
				} else {
					modifierMatch = Regex.Match(expression.Trim(), expressionTestActionModifier);
					if (modifierMatch.Groups.Count > 1) {
						modifierTest = modifierMatch.Groups["test"].Value;
						modifierAction = modifierMatch.Groups["action"].Value;
						wfEx.AddRange(ParseExpression(modifierTest));
						wfEx[0].IsTestAction = true;
						wfEx.AddRange(ParseExpression(modifierAction));
					} else {
						modifierMatch = Regex.Match(expression.Trim(), expressionActionModifier);
						modifierNegative = modifierMatch.Groups["not"].Value;
						if (!String.IsNullOrEmpty(modifierNegative)) { if (modifierNegative == "!" || modifierNegative == "^") isNegative = true; }
						modifierAction = modifierMatch.Groups["action"].Value;
						MatchCollection parameterMatches = Regex.Matches(modifierMatch.Groups["params"].Value, expressionParameters);
						if (parameterMatches.Count > 0) {
							modifierParameters = new object[parameterMatches.Count];
							for (int i = 0; i <= parameterMatches.Count - 1; i++) {
								modifierParameters[i] = ParseExpressionParameter(parameterMatches[i].Value);
							}
						}
						WorkflowAction wfa = new(expression, Common.ConvertParsedModifierToEnum(modifierAction), modifierParameters);
						wfa.Negative = isNegative;
						wfEx.Add(wfa);
					}
				}
				return wfEx;
			} catch (Exception ex) {
				throw ex;
			}
		}

		private object ParseExpressionParameter(string param) {
			try {
				param = param.Trim();
				Match modifierMatch = Regex.Match(param, expressionSubModifier);
				if (modifierMatch.Success) {
					return ParseExpression(param);
				} else {
					modifierMatch = Regex.Match(param, expressionActionParameter);
					if (modifierMatch.Success) {
						string target, obj, var;
						target = modifierMatch.Groups["target"].Value.Trim();
						obj = modifierMatch.Groups["obj"].Value.Trim();
						var = modifierMatch.Groups["var"].Value.Trim();
						return new ExpressionParameter() { Target = target, ObjectClass = obj, ObjectClassProperty = var };
					} else {
						// No expression based parameter found.
						return param;
					}
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		public static WorkflowExpressionModifier CompareNumericValues(object target, List<object> parameters) {
			// Explanation of the comparison below.
			//================================================
			// Value                | Meaning
			//================================================
			// Less than zero       | a is less than b.
			// Zero                 | a equals b.
			// Greater than zero    | a is greater than b.

			if (((IComparable)target).CompareTo((IComparable)parameters[0]) > 0) return WorkflowExpressionModifier.gt;
			if (((IComparable)target).CompareTo((IComparable)parameters[0]) < 0) return WorkflowExpressionModifier.lt;
			if (((IComparable)target).CompareTo((IComparable)parameters[0]) >= 0) return WorkflowExpressionModifier.gte;
			if (((IComparable)target).CompareTo((IComparable)parameters[0]) <= 0) return WorkflowExpressionModifier.lte;
			// Two values triggers a between comparison.
			if (parameters.Count == 2) {
				// First check if the target is equal to either post value.
				if (((IComparable)target).CompareTo((IComparable)parameters[0]) == 0 || ((IComparable)target).CompareTo((IComparable)parameters[1]) == 0) {
					return WorkflowExpressionModifier.gte;
				} else {
					// Second check if the target is greater than the first value and less than the second value.
					if (((IComparable)target).CompareTo((IComparable)parameters[0]) > 0 && ((IComparable)target).CompareTo((IComparable)parameters[1]) < 0) {
						return WorkflowExpressionModifier.gte;
					}
				}
			}
			if (((IComparable)target).CompareTo((IComparable)parameters[0]) == 0) return WorkflowExpressionModifier.eq;
			return ((IComparable)target).CompareTo((IComparable)parameters[0]) > 0 && ((IComparable)target).CompareTo((IComparable)parameters[0]) < 0
				? WorkflowExpressionModifier.bw
				: WorkflowExpressionModifier.None;
		}

		public static WorkflowExpressionModifier CompareNumericValue(object target, int compareWith) {
			// Explanation of the comparison below.
			//================================================
			// Value                | Meaning
			//================================================
			// Less than zero       | a is less than b.
			// Zero                 | a equals b.
			// Greater than zero    | a is greater than b.

			if (((IComparable)target).CompareTo((IComparable)compareWith) > 0) return WorkflowExpressionModifier.gt;
			if (((IComparable)target).CompareTo((IComparable)compareWith) < 0) return WorkflowExpressionModifier.lt;
			return ((IComparable)target).CompareTo((IComparable)compareWith) == 0
				? WorkflowExpressionModifier.eq
				: WorkflowExpressionModifier.None;
		}
	}
}