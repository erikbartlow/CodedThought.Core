using System.Text.RegularExpressions;

namespace CodedThought.Core.Workflow {

	public class Execution {
		public List<object> Actions { get; set; }
	}

	public class Test {
		private const string parseExpression = @"(?<test>(?<left>(.*))=>(?<right>(.*)))";

		public string Expression { get; set; }

		public Action Left { get; set; }

		public Action Right { get; set; }

		public bool ParseSuccess { get; set; }

		public bool Parse(string expression) {
			try {
				bool success = false;
				Match expressionMatch = Regex.Match(expression.Trim(), parseExpression);
				success = expressionMatch.Success;
				if (success) {
					Group mainGroup = expressionMatch.Groups["test"];
					if (mainGroup.Success) {
						Group left = expressionMatch.Groups["left"];
						Group right = expressionMatch.Groups["right"];
						if (left.Success) { Left = new Action(left.Value.Trim()); }
						if (right.Success) { Right = new Action(right.Value.Trim()); }
						success = mainGroup.Success && left.Success && right.Success;
					}
				}
				return success;
			} catch (Exception ex) {
				throw ex;
			}
		}

		public Test() {
			Expression = string.Empty; ParseSuccess = false;
		}

		public Test(string expression) : this() {
			Expression = expression; ParseSuccess = Parse(expression);
		}
	}

	public class Action {
		private const string parseExpression = @"(?<action>SET|CHECK|GET|EMAIL|SWITCH|gt|lt|eq|gte|lte|btw|len)\((?<param>.*)\)";

		public int Index { get; set; }

		public int Order { get; set; }

		public string Expression { get; set; }

		public WorkflowExpressionModifier Modifier { get; set; }

		public object TargetArgument { get; set; }

		public List<object> Arguments { get; set; }

		public Func<object, List<object>, WorkflowResult> Run { get; set; }

		public List<Action> SubActions { get; set; }

		public bool ParseSuccess { get; set; }

		public bool Parse(string expression) {
			try {
				bool success = false;
				Match expressionMatch = Regex.Match(expression.Trim(), parseExpression);
				success = expressionMatch.Success;
				if (success) {
					Group mainGroup = expressionMatch.Groups["action"];
					if (mainGroup.Success) {
						Modifier = Common.ConvertParsedModifierToEnum(mainGroup.Value);
						Group arguments = expressionMatch.Groups["param"];

						if (arguments.Success) {
							// Split the value by commas. Each position is an argument as ValueOf or Value.
							string[] args = arguments.Value.Split(",".ToCharArray());

							for (int i = 0; i < args.Length - 1; i++) {
								// Handle the target argument.
								if (i == 0) {
									ValueOf targetValue = new(args[i]);
									if (!targetValue.ParseSuccess) {
										// The argument must be a straight value and not a ValueOf.
										TargetArgument = args[i];
									} else {
										TargetArgument = targetValue;
									}
								} else {
									ValueOf thisArg = new(args[i]);
									if (thisArg.ParseSuccess) {
										Arguments.Add(thisArg);
									} else {
										Arguments.Add(args[i]);
									}
								}
							}
						}
						success = mainGroup.Success;
					}
				}
				return success;
			} catch (Exception ex) {
				throw ex;
			}
		}

		public Action() {
			Expression = string.Empty; ParseSuccess = false; SubActions = new List<Action>();
		}

		public Action(string expression) : this() {
			Expression = expression; ParseSuccess = Parse(expression);
		}
	}

	public class ValueOf {
		private const string parseExpression = @"\<(?<valueOf>(?<obj>[A-z0-9]+?)\.(?<prop>[A-z0-9]+?))\>"; //@"\<(?<val>[0-9]+?)\>";

		public string Expression { get; set; }

		public string ObjectName { get; set; }

		public string PropertyName { get; set; }

		public bool ParseSuccess { get; set; }

		public bool Parse(string expression) {
			try {
				bool success = false;
				Match expressionMatch = Regex.Match(expression.Trim(), parseExpression);
				success = expressionMatch.Success;
				if (success) {
					Group mainGroup = expressionMatch.Groups["valueOf"];
					if (mainGroup.Success) {
						Group obj = expressionMatch.Groups["obj"];
						if (obj.Success) {
							ObjectName = obj.Value;
						}
						Group prop = expressionMatch.Groups["prop"];
						if (prop.Success) {
							PropertyName = prop.Value;
						}
						success = mainGroup.Success && obj.Success && prop.Success;
					}
				}
				return success;
			} catch (Exception ex) {
				throw ex;
			}
		}

		public ValueOf() {
			Expression = string.Empty; ParseSuccess = false;
		}

		public ValueOf(string expression) : this() {
			Expression = expression; ParseSuccess = Parse(expression);
		}
	}
}