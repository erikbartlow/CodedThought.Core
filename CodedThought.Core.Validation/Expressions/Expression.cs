using CodedThought.Core.Validation.Exceptions;
using System.Text;

namespace CodedThought.Core.Validation.Expressions {

	public class Expression {
		private ExpressionModifiers _modifier = ExpressionModifiers.None;
		private ExpressionFlags _flags = ExpressionFlags.None;
		private List<object> _modifierParameters = new();
		private ExpressionOperands _operand = ExpressionOperands.Equals;

		/// <summary>Gets a value indicating whether this instance refers to "this" instead of a passed target value.</summary>
		/// <value><c>true</c> if this instance is this; otherwise, <c>false</c>.</value>
		protected bool IsThis {
			get { return (Target.ToString().ToLower() == "this"); }
		}

		/// <summary>Gets or sets the target.</summary>
		/// <value>The target.</value>
		public object Target { get; set; }

		public void SetModifier(ExpressionModifiers modifier) {
			_modifier = modifier;
			_modifier &= ~ExpressionModifiers.None;
		}

		/// <summary>Gets or sets the modifier.</summary>
		/// <value>The modifier.</value>
		public void SetModifier(ExpressionModifiers modifier, params object[] parameters) {
			SetModifier(modifier);
			_modifierParameters = parameters.ToList<object>();
		}

		/// <summary>Sets the flags.</summary>
		/// <param name="flag">The flag.</param>
		public void SetFlag(ExpressionFlags flag) {
			_flags = flag;
			_flags &= ~ExpressionFlags.None;
		}

		/// <summary>Gets the get modifier.</summary>
		/// <value>The get modifier.</value>
		public ExpressionModifiers Modifier { get { return _modifier; } }

		/// <summary>Gets the flags.</summary>
		/// <value>The flags.</value>
		public ExpressionFlags Flags { get { return _flags; } }

		/// <summary>Gets the modifier parameters.</summary>
		/// <value>The modifier parameters.</value>
		public object[] ModifierParameters {
			get {
				return _modifierParameters.Count == 0 ? (new object[0]) : _modifierParameters.ToArray<object>();
			}
		}

		/// <summary>Gets or sets the operand.</summary>
		/// <value>The operand.</value>
		public ExpressionOperands Operand { get { return _operand; } set { _operand = value; } }

		/// <summary>Gets or sets the expression's original pattern string.</summary>
		/// <value>The expression reference.</value>
		public String ExpressionRef { get; set; }

		/// <summary>Gets or sets the validation result.</summary>
		/// <value>The validation result.</value>
		public List<ValidationResult> ValidationResult { get; set; }

		/// <summary>Gets or sets the expressions.</summary>
		/// <value>The expressions.</value>
		public List<ExpressionGroup> Groups { get; set; }

		/// <summary>Gets or sets the exceptions if the <c>Test</c> method results in a <c>false</c>.</summary>
		/// <value>The exceptions.</value>
		public List<ExpressionException> Exceptions { get; set; }

		/// <summary>Gets a value indicating whether this instance is valid based on the expression.</summary>
		/// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>. If <c>false</c> then the Exceptions list property will have 1 or more exceptions.</value>
		public Boolean Test(object value) {
			TargetTypeEnum targetType = Common.GetValueType(value);
			if (targetType == TargetTypeEnum.Text) { value = value.ToString().Trim(); }
			if (IsThis) { Target = value; }
			// If there are no groups then this is a single expression.
			if (Groups.Count == 0) {
				return TestExpression(value);
			} else {
				// Process the groups first if any.
				List<bool> groupResults = new();
				foreach (ExpressionGroup group in Groups) { groupResults.Add(group.Test(value)); }
				return groupResults.All(b => b == true);
			}
		}

		/// <summary>Tests the expression.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private Boolean TestExpression(object value) {
			bool operandResult = false, modifierResult = false;
			Switch.On(Operand)
				.Case(ExpressionOperands.Equals, () => operandResult = Target.ExpressionEquals(value))
				.Case(ExpressionOperands.GreaterThan, () => operandResult = Target.ExpressionGreaterThan(value))
				.Case(ExpressionOperands.GreaterThanOrEqualTo, () => operandResult = (Target.ExpressionGreaterThan(value) || Target.ExpressionEquals(value)))
				.Case(ExpressionOperands.LessThan, () => operandResult = Target.ExpressionLessThan(value))
				.Case(ExpressionOperands.LessThanOrEqualTo, () => operandResult = (Target.ExpressionLessThan(value) || Target.ExpressionEquals(value)))
				.Case(ExpressionOperands.NotEqualTo, () => operandResult = !Target.ExpressionEquals(value))
				.Case(ExpressionOperands.None, () => operandResult = true);

			// Apply any modifiers.
			modifierResult = ApplyModifier(value);
			if (!(operandResult && modifierResult)) { this.Exceptions.Add(new ExpressionException() { Expression = this.ExpressionRef, ValueInError = value }); }
			return operandResult && modifierResult;
		}

		/// <summary>Applies the modifier.</summary>
		/// <param name="value">The value.</param>
		/// <returns>Boolean</returns>
		/// <exception cref="System.ArgumentNullException">The Max, Min, Between modifiers require numeric values to validate against.</exception>
		/// <exception cref="System.ArgumentException">
		/// The Between modifier must be used in conjunction with numeric type values only. or The Email modifier must be applied to text based values only.
		/// </exception>
		private bool ApplyModifier(object value) {
			TargetTypeEnum valuetype = Common.GetValueType(value);
			bool hasForce = _flags.HasFlag(ExpressionFlags.Force);
			bool caseInsensitive = _modifier.HasFlag(ExpressionModifiers.CaseInsensitive);
			bool required = _modifier.HasFlag(ExpressionModifiers.Required);
			bool isValid = true;
			// If no modifiers then return true.
			if (_modifier == ExpressionModifiers.None) { return true; }

			foreach (ExpressionModifiers mod in Extensions.EnumToList<ExpressionModifiers>()) {
				// Assume it will pass.
				ValidationResult result = new() { Result = ValidationResultTypes.PASS };
				if (_modifier.HasFlag(mod)) {
					switch (mod) {
						case ExpressionModifiers.Uppercase:
							if (value.GetType() != typeof(String)) { isValid = false; throw new ArgumentException("The Upper Case modifier must be applied to text based values only."); }
							if (hasForce) { value = value.ToString().ToUpper(); }
							isValid = value.ToString().ExpressionUpper();
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.NotUpperCaseMessage);
							}
							break;

						case ExpressionModifiers.Lowercase:
							if (value.GetType() != typeof(String)) { isValid = false; throw new ArgumentException("The Lower Case modifier must be applied to text based values only."); }
							if (hasForce) { value = value.ToString().ToLower(); }
							isValid = value.ToString().ExpressionLower();
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.NotLowerCaseMessage);
							}
							break;

						case ExpressionModifiers.Max:
							if (_modifierParameters.Count == 0) { isValid = false; throw new ArgumentNullException("The Max modifier requires a numeric value to validate against."); }
							if (value is Int32) { isValid = Convert.ToInt32(value).ExpressionMax(Convert.ToInt32(_modifierParameters[0])); }
							if (value is Decimal || value is float) { isValid = Convert.ToDecimal(value).ExpressionMax(Convert.ToInt32(_modifierParameters[0])); }
							if (value is String) { isValid = Convert.ToString(value).ExpressionMax(Convert.ToInt32(_modifierParameters[0])); }
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.ExceedsMaxMessage);
							}
							break;

						case ExpressionModifiers.Min:
							if (_modifierParameters.Count == 0) { isValid = false; throw new ArgumentNullException("The Min modifier requires a numeric value to validate against."); }
							if (value is Int32) { isValid = Convert.ToInt32(value).ExpressionMin(Convert.ToInt32(_modifierParameters[0])); }
							if (value is Decimal || value is float) { isValid = Convert.ToDecimal(value).ExpressionMin(Convert.ToInt32(_modifierParameters[0])); }
							if (value is String) { isValid = Convert.ToString(value).ExpressionMin(Convert.ToInt32(_modifierParameters[0])); }
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.MinimumNotReachedMessage);
							}
							break;

						case ExpressionModifiers.Email:
							if (value.GetType() != typeof(String)) { isValid = false; throw new ArgumentException("The Email modifier must be applied to text based values only."); }
							isValid = Convert.ToString(value).ExpressionIsEmail();
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.InvalidEmailMessage);
							}
							break;

						case ExpressionModifiers.In:
							if (_modifierParameters.Count == 0) { isValid = false; throw new ArgumentNullException("The In modifier requires list of values to validate against."); }
							isValid = value.ToString().ExpressionIn(_modifierParameters);
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.NotInListMessage);
							}
							break;

						case ExpressionModifiers.InDB:
							if (_modifierParameters.Count == 0) { isValid = false; throw new ArgumentNullException("The InDB modifier requires a list of DB tables to validate against."); }
							isValid = value.ToString().ExpressionInDB(_modifierParameters);
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.NotInListMessage);
							}
							break;

						case ExpressionModifiers.Required:
							if (value == null || value.ToString() == String.Empty) { isValid = false; }
							if (!isValid) {
								result.Result = ValidationResultTypes.FAIL_WITH_MESSAGE;
								result.Exception = new ValidationException(ExceptionMessages.RequiredMessage);
							}
							break;

						default:
							continue;
					}
					ValidationResult.Add(result);
				}
			}
			return !ValidationResult.Any(b => b.Result == ValidationResultTypes.FAIL || b.Result == ValidationResultTypes.FAIL_WITH_MESSAGE);
		}

		/// <summary>Gets the validation messages.</summary>
		/// <param name="htmlFormatted">if set to <c>true</c> the return string will have HTML formatting for use on a web page. Otherwise the messages will be delimited by a semi-colon</param>
		/// <returns>Combined string of all expression exception messages.</returns>
		public string GetValidationMessages(bool htmlFormatted = true) {
			StringBuilder sb = new();

			if (htmlFormatted) {
				sb.Append(String.Join("<br />", ValidationResult
					.Where(m => m.Result == ValidationResultTypes.FAIL_WITH_MESSAGE)
					.Select(m => $"<small class=\"help-block\">{m.Exception.Message}</small>")));
				if (Groups.Count > 0) {
					Groups.ForEach(g => g.Expressions.ForEach(e => sb.Append(e.GetValidationMessages(htmlFormatted))));
				}
			} else {
				sb.Append(String.Join(";", ValidationResult
					.Where(m => m.Result == ValidationResultTypes.FAIL_WITH_MESSAGE)
					.Select(m => m.Exception.Message)));
				if (Groups.Count > 0) {
					Groups.ForEach(g => g.Expressions.ForEach(e => sb.Append(e.GetValidationMessages(htmlFormatted))));
				}
			}
			return sb.ToString();
		}

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Expression" /> class.</summary>
		/// <remarks>Not setting the target will instruct the expression to use the value being validated.</remarks>
		public Expression() {
			Exceptions = new List<ExpressionException>();
			Groups = new List<ExpressionGroup>();
			_modifier = ExpressionModifiers.None;
			_operand = ExpressionOperands.None;
			Target = "this";
			ValidationResult = new List<Expressions.ValidationResult>();
		}

		/// <summary>Initializes a new instance of the <see cref="Expression" /> class.</summary>
		/// <param name="expressionRef">The expression reference.</param>
		public Expression(String expressionRef) : this() => ExpressionRef = expressionRef;

		public Expression(ExpressionModifiers modifier, params object[] parameters) : this() {
			_modifier = modifier;
			_modifierParameters = parameters.ToList();
		}

		/// <summary>Initializes a new instance of the <see cref="Expression" /> class.</summary>
		/// <param name="target">  The target.</param>
		/// <param name="modifier">The modifier.</param>
		public Expression(string target, ExpressionModifiers modifier, params object[] parameters) : this(modifier, parameters) {
			Target = target;
		}

		/// <summary>Initializes a new instance of the <see cref="Expression" /> class.</summary>
		/// <param name="modifier">       The modifier.</param>
		/// <param name="expressionGroup">The expression group.</param>
		public Expression(ExpressionGroup expressionGroup, ExpressionModifiers modifier, params object[] parameters) : this(modifier, parameters) => Groups.Add(expressionGroup);

		/// <summary>Initializes a new instance of the <see cref="Expression" /> class.</summary>
		/// <param name="target">  The target.</param>
		/// <param name="modifier">The modifier.</param>
		public Expression(string target, ExpressionGroup expressionGroup, ExpressionModifiers modifier, params object[] parameters) : this(target, modifier, parameters) => Groups.Add(expressionGroup);

		#endregion Constructors
	}
}