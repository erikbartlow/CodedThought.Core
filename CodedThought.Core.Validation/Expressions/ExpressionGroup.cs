namespace CodedThought.Core.Validation.Expressions {

	public class ExpressionGroup {

		/// <summary>Gets or sets the expressions.</summary>
		/// <value>The expressions.</value>
		public List<Expression> Expressions { get; set; }

		/// <summary>Gets or sets the ExpressionJoin.</summary>
		/// <value>The ExpressionJoin.</value>
		public ExpressionJoin JoinType { get; set; }

		/// <summary>Gets a value indicating whether this instance is valid based on the expression.</summary>
		/// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>. If <c>false</c> then the Exceptions list property will have 1 or more exceptions.</value>
		public Boolean Test(object value) {
			TargetTypeEnum targetType = Common.GetValueType(value);
			bool finalResult = false;
			if (Expressions.Count == 0) {
				// If there are no expressions to test then return true.
				return true;
			} else {
				// Process the groups first if any.
				List<bool> expResults = new();
				foreach (Expression exp in Expressions) {
					expResults.Add(exp.Test(value));
				}
				CodedThought.Core.Switch.On(JoinType)
					.Case(ExpressionJoin.OR, () => finalResult = expResults.Any(b => b == false))
					.Case(ExpressionJoin.AND, () => finalResult = expResults.All(b => b == true));
				return finalResult;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="ExpressionGroup" /> class.</summary>
		public ExpressionGroup() { JoinType = ExpressionJoin.AND; Expressions = new List<Expression>(); }

		/// <summary>Initializes a new instance of the <see cref="ExpressionGroup" /> class.</summary>
		/// <param name="typeOfExpressionJoin">The type of ExpressionJoin.</param>
		public ExpressionGroup(ExpressionJoin typeOfJoin) : this() { JoinType = typeOfJoin; }

		/// <summary>Initializes a new instance of the <see cref="ExpressionGroup" /> class.</summary>
		/// <param name="typeOfExpressionJoin">The type of ExpressionJoin.</param>
		/// <param name="expression">          The expression.</param>
		public ExpressionGroup(ExpressionJoin typeOfJoin, Expression expression) : this() { JoinType = typeOfJoin; Expressions.Add(expression); }
	}
}