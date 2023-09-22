using System.Text.RegularExpressions;

namespace CodedThought.Core.Validation.Expressions {

	public sealed class Evaluator {
		private static Evaluator _instance;
		private static readonly object Padlock = new();

		private const string groupJoinExpressions = @"\)(&&)\(|\)(\|\|)\(";
		private const string expressionJoin = @"(\|\||\&\&+?)";
		private const string joinMultiGroup = @"\)(&&)\(|\)(\|\|)\(";
		private const string expressionsMultiGroup = @"(\[.+?\](\|\||\&\&)\[.+?\])";
		private const string expressionsWithinAGroup = @"(?<expression>\[.+?\])(?<operand>\|\||\&\&)?";
		private const string expressionContents = @"\[(.+)\]+?";
		private const string expressionModifer = @"\|(?<modifiers>(?<modifer>[ulrie]|mx|mn|ru|rd)\((?<param>[\d]+)\)\,*?(?<flags>[f])*))+\]";
		private const string expressionModiferAndParam = @".+\|{1}([ulire]|mx|mn)\((\d+)\)";
		private const string expressionPattern = @"\[(?<operand>[=><%]|!=)+?\""(?<target>[a-zA-Z]+?)\""(?<modifiers>(?:\|)+?(?<modifier>(?:\|)[ulire]|mx|mn|ru|rd\((?<param>\d+)\)\,*?(?<optional>[fr])*))*?\]";
		private const string expressionThisPattern = @"\[(?<group>(?<target>[this]+)\|(?<modifiers>(?<modifier>[ulrie]|mx|mn|ru|rd)\((?<param>[\d]+)\)\,*?(?<flags>[f])*))+\]";
		private const string expressionOperandTarget = @"\[([=><%!=]).?(.+).+\|.*\]";

		//private const string expressionPatternComplete = @"\[(?<operand>[=><%]|!=)*?(?:\""?)(?<target>this|\w+)?(?:\""?)\|?(?<modifier>[urliebURLIEB]|MX|MN|IN|INDB|RU|RD|mx|mn|in|indb|ru|rd)?(?:\()*?(?<params>(?:\w+|,|\.|_)*)*?(?:\))*?(?<flags>\s-\w.*)*?\]";
		//This version of the RegEx better captures all groups including the new IN AND INDB modifier and multple parameters.
		private const string expressionPatternComplete = @"\[(?<operand>[=><%]|!=)*?(?:\""?)(?<target>this|[a-zA-Z0-9_\/]+)?(?:\""?)\|?(?<modifier>[urliebURLIEB]|MX|MN|IN|INDB|RU|RD|mx|mn|in|indb|ru|rd)?(?:\()*?(?<params>(?:[a-zA-Z0-9_\/]+|,)*)*?(?<paramsdb>([a-zA-Z0-9_\/]+[\.|=][a-zA-Z0-9_\/]+,?)+)*?(?:\))*?(?<flags>\s-\w.*)*?\]";

		#region Members.

		/// <summary>Gets an instance of the Evaluator class.</summary>
		/// <value>The instance.</value>
		public static Evaluator Instance {
			get {
				Evaluator result;
				lock (Evaluator.Padlock) {
					Evaluator arg;
					if ((arg = Evaluator._instance) == null) {
						arg = (Evaluator._instance = new Evaluator());
					}
					result = arg;
				}
				return result;
			}
		}

		/// <summary>Compiles the expression.</summary>
		/// <param name="expression">The expression.</param>
		/// <returns></returns>
		public Expression CompileExpression(string expression) {
			try {
				Expression parsedExpression = new();
				parsedExpression.ExpressionRef = expression;
				// Determine if this expression has multiple groups.
				Match matchGroups;
				Match multiGroupJoinMatches = Regex.Match(expression, joinMultiGroup, RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
				if (multiGroupJoinMatches.Success) {
					// Get the sub group matches.
					matchGroups = Regex.Match(expression, expressionsMultiGroup);
					// Multiple Groups of Expressions.
					var groupList = matchGroups.Groups.Cast<Match>();
					parsedExpression.Groups.AddRange(groupList.Select(g => new ExpressionGroup() {
						JoinType = GetTypeOfJoin(multiGroupJoinMatches.Groups[0].Value),
						Expressions = g.Groups.Cast<Match>().Select(e => ParseExpression(e.Groups[1].Value)).ToList()
					}));
				} else {
					// Single group.
					MatchCollection matchedExpressions = Regex.Matches(expression, expressionsWithinAGroup, RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
					if (matchedExpressions.Count == 1) {
						// A single expression.
						parsedExpression = ParseExpression(expression);
					} else {
						// Multiple expressions.
						ExpressionGroup grp = new();
						string joinValue = string.Empty;
						foreach (Match expressionMatch in matchedExpressions) {
							// Get the expression.
							string expressionValue;
							try { expressionValue = expressionMatch.Groups["expression"].Value; } catch { expressionValue = string.Empty; }
							if (joinValue == String.Empty)
								try {
									joinValue = expressionMatch.Groups["operand"].Value;
									grp.JoinType = GetTypeOfJoin(joinValue);
								} catch {
									// Ignore additional operands.
								}
							if (expressionValue != string.Empty)
								grp.Expressions.Add(ParseExpression(expressionValue));
						}
						parsedExpression.Groups.Add(grp);
					}
				}

				return parsedExpression;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Parses the expression.</summary>
		/// <param name="expression">The expression.</param>
		/// <returns></returns>
		public Expression ParseExpression(string expression) {
			string operand;
			string[] modifier, flags;
			object[] param = new object[0];
			Expression newExpression = new(expression);
			// Check if this expression targets "this".
			//Match targetMatch = Regex.Match(expression, expressionThisPattern);
			Match targetMatch = Regex.Match(expression, expressionPatternComplete);
			newExpression.Target = targetMatch.Groups["target"].Value;
			// Capture the matched groups.
			try { operand = targetMatch.Groups["operand"].Value; } catch { operand = string.Empty; }
			try { modifier = new[] { targetMatch.Groups["modifier"].Value }; } catch { modifier = new string[0]; }
			try { flags = new[] { targetMatch.Groups["flags"].Value }; } catch { flags = new string[0]; }

			if (targetMatch.Groups["params"].Success) {
				param = targetMatch.Groups["params"].ToString().Split(",".ToCharArray());
			}
			if (targetMatch.Groups["paramsdb"].Success) {
				param = targetMatch.Groups["paramsdb"].ToString().Split(",".ToCharArray());
			}
			if (targetMatch.Groups["flags"].Success) {
				flags = targetMatch.Groups["flags"].ToString().Split("-".ToCharArray());
			}

			if (operand != String.Empty) newExpression.Operand = GetTypeOfOperand(operand);
			if (param.Length > 0) {
				if (modifier.Length != 0) newExpression.SetModifier(GetModifiers(modifier), param);
			} else {
				if (modifier.Length != 0) newExpression.SetModifier(GetModifiers(modifier));
			}
			if (flags.Length != 0) newExpression.SetFlag(GetFlags(flags));

			return newExpression;
		}

		/// <summary>Gets the type of join.</summary>
		/// <param name="joinChar">The join character.</param>
		/// <returns></returns>
		private ExpressionJoin GetTypeOfJoin(string joinChar) {
			switch (joinChar) {
				case "||":
					return ExpressionJoin.OR;

				case "&&":
				default:
					return ExpressionJoin.AND;
			}
		}

		/// <summary>Gets the type of operand.</summary>
		/// <param name="operandChar">The operand character.</param>
		/// <returns></returns>
		private ExpressionOperands GetTypeOfOperand(string operandChar) {
			switch (operandChar) {
				case "=":
					return ExpressionOperands.Equals;

				case "<":
					return ExpressionOperands.LessThan;

				case ">":
					return ExpressionOperands.GreaterThan;

				case ">=":
					return ExpressionOperands.GreaterThanOrEqualTo;

				case "<=":
					return ExpressionOperands.LessThanOrEqualTo;

				case "<>":
				case "!=":
					return ExpressionOperands.NotEqualTo;

				default:
					return ExpressionOperands.None;
			}
		}

		/// <summary>Gets the modifiers.</summary>
		/// <param name="modifierChars">The modifier chars.</param>
		/// <returns></returns>
		private ExpressionModifiers GetModifiers(string[] modifierChars) {
			ExpressionModifiers returnModifer = ExpressionModifiers.None;
			for (int i = 0; i <= modifierChars.Length - 1; i++) {
				CodedThought.Core.Switch.On(modifierChars[i].ToLower())
					.Case("u", () => returnModifer |= ExpressionModifiers.Uppercase)
					.Case("l", () => returnModifer |= ExpressionModifiers.Lowercase)
					.Case("i", () => returnModifer |= ExpressionModifiers.CaseInsensitive)
					.Case("ru", () => returnModifer |= ExpressionModifiers.RoundUp)
					.Case("rd", () => returnModifer |= ExpressionModifiers.RoundDown)
					.Case("mx", () => returnModifer |= ExpressionModifiers.Max)
					.Case("mn", () => returnModifer |= ExpressionModifiers.Min)
					.Case("b", () => returnModifer |= ExpressionModifiers.Between)
					.Case("e", () => returnModifer |= ExpressionModifiers.Email)
					.Case("r", () => returnModifer |= ExpressionModifiers.Required)
					.Case("in", () => returnModifer |= ExpressionModifiers.In)
					.Case("indb", () => returnModifer |= ExpressionModifiers.InDB);
			}
			// Remove the NONE modifier if one was set.
			if (modifierChars.Length > 0) returnModifer &= ~ExpressionModifiers.None;
			return returnModifer;
		}

		/// <summary>Gets the flags.</summary>
		/// <param name="flagChars">The flag chars.</param>
		/// <returns></returns>
		private ExpressionFlags GetFlags(string[] flagChars) {
			ExpressionFlags returnFlag = ExpressionFlags.None;
			for (int i = 0; i <= flagChars.Length - 1; i++) {
				CodedThought.Core.Switch.On(flagChars[i].ToLower())
					.Case("f", () => returnFlag |= ExpressionFlags.Force);
			}
			if (flagChars.Length > 0) returnFlag &= ~ExpressionFlags.None;
			return returnFlag;
		}

		#endregion Members.
	}
}