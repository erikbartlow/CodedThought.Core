namespace CodedThought.Core.Validation.Expressions {

	public static class Extensions {

		#region Operand Extensions.

		#region Common

		public static Boolean ExpressionEquals<T>(this T value, T compare) {
			Boolean compareResult = true;
			CodedThought.Core.Switch.On(value.GetType())
				.Case(typeof(string), () => compareResult = Convert.ToString(value).Equals(compare.ToString()))
				.Case(typeof(int), () => compareResult = Convert.ToInt32(value).Equals(compare))
				.Case(typeof(Decimal), () => compareResult = Convert.ToDecimal(value).Equals(compare))
				.Case(typeof(float), () => compareResult = Convert.ToDecimal(value).Equals(compare))
				.Case(typeof(DateTime), () => compareResult = Convert.ToDateTime(value).Equals(compare));
			return compareResult;
		}

		public static int ExpressionComparesTo<T>(this T value, T compare) {
			int compareResult = 0;
			CodedThought.Core.Switch.On(value.GetType())
				.Case(typeof(string), () => compareResult = Convert.ToString(value).CompareTo(compare.ToString()))
				.Case(typeof(int), () => compareResult = Convert.ToInt32(value).CompareTo(compare))
				.Case(typeof(Decimal), () => compareResult = Convert.ToDecimal(value).CompareTo(compare))
				.Case(typeof(float), () => compareResult = Convert.ToDecimal(value).CompareTo(compare))
				.Case(typeof(long), () => compareResult = Convert.ToInt32(value).CompareTo(compare))
				.Case(typeof(DateTime), () => compareResult = Convert.ToDateTime(value).CompareTo(compare));

			return compareResult;
		}

		public static Boolean ExpressionContains<T>(this T value, T compare) {
			bool compareResult = false;
			CodedThought.Core.Switch.On(value.GetType())
				.Case(typeof(String), () => compareResult = Convert.ToString(value).Contains(compare.ToString()))
				.Case(typeof(Char), () => compareResult = Convert.ToString(value).Contains(compare.ToString()));

			return compareResult;
		}

		public static Boolean ExpressionIn<T>(this T value, List<object> compare) {
			bool compareResult = false;
			CodedThought.Core.Switch.On(value.GetType())
				.Case(typeof(String), () => compareResult = compare.Select(v => v.ToString().ToUpper()).ToList<string>().Where(c => c.Contains(value.ToString().ToUpper())).Count() > 0)
				.Case(typeof(Char), () => compareResult = compare.Select(v => v.ToString().ToUpper()).ToList<string>().Where(c => c.Contains(value.ToString().ToUpper())).Count() > 0)
				.Case(typeof(int), () => compareResult = compare.Select(v => Convert.ToInt32(v)).ToList<int>().Where(c => c.Equals(Convert.ToInt32(value))).Count() > 0)
				.Case(typeof(Decimal), () => compareResult = compare.Select(v => Convert.ToDecimal(v)).ToList<Decimal>().Where(c => c.Equals(Convert.ToDecimal(value))).Count() > 0)
				.Case(typeof(float), () => compareResult = compare.Select(v => Convert.ToDecimal(v)).ToList<Decimal>().Where(c => c.Equals(Convert.ToDecimal(value))).Count() > 0);

			return compareResult;
		}

		public static Boolean ExpressionBetween<T>(this T value, List<object> compare) {
			bool compareResult = false;
			if (value.ExpressionEquals(compare[0]) || value.ExpressionEquals(compare[1])) {
				compareResult = true;
			} else {
				// Second check if the target is greater than the first value and less than the second value.
				if (value.ExpressionGreaterThan(compare[0]) && value.ExpressionLessThan(compare[1])) {
					compareResult = true;
				}
			}
			return compareResult;
		}

		public static Boolean ExpressionInDB<T>(this T value, List<object> compare) {
			bool compareResult = false;
			string[] table_column;
			List<KeyValuePair<string, string>> whereParams = new();

			if (compare.Count > 0) {
				table_column = compare[0].ToString().Split('.');

				if (compare.Count > 1) {
					string[] factors;
					for (int i = 1; i < compare.Count; i++) {
						factors = compare[i].ToString().Split('=');
						if (factors.Length == 2)
							whereParams.Add(new KeyValuePair<string, string>(factors[0], factors[1]));
					}
				}

				//if (table_column.Length == 2) {
				//    DataTable dt = SmartUploadController.Instance.GetListOfDistinctValues(table_column[0], table_column[1], whereParams);

				//    List<string> lstResult = (from table in dt.AsEnumerable()
				//                              where table.Field<string>(table_column[1])?.ToUpper() == value.ToString()?.ToUpper()
				//                              select table.Field<string>(table_column[1])).ToList();
				//    if (lstResult.Count > 0)
				//        compareResult = true;
				//}
			}
			return compareResult;
		}

		#endregion Common

		#region Equals

		public static Boolean ExpressionEquals(this object value, object compare) => ExpressionEquals<object>(value, compare);

		public static Boolean ExpressionEquals(this DateTime value, DateTime compare) => ExpressionEquals<DateTime>(value, compare);

		public static Boolean ExpressionEquals(this String value, string compare) => ExpressionEquals<String>(value, compare);

		public static Boolean ExpressionEquals(this int value, int compare) => ExpressionEquals<int>(value, compare);

		public static Boolean ExpressionEquals(this Decimal value, Decimal compare) => ExpressionEquals<Decimal>(value, compare);

		public static Boolean ExpressionEquals(this Double value, Double compare) => ExpressionEquals<Double>(value, compare);

		public static Boolean ExpressionEquals(this float value, float compare) => ExpressionEquals<float>(value, compare);

		#endregion Equals

		#region Greater Than

		public static Boolean ExpressionGreaterThan(this object value, object compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<object>(compare))
				.Case(-1, () => result = false) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = true); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionGreaterThan(this DateTime value, DateTime compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<DateTime>(compare))
				.Case(-1, () => result = false) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = true); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionGreaterThan(this int value, int compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<int>(compare))
				.Case(-1, () => result = false) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = true); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionGreaterThan(this Decimal value, Decimal compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<Decimal>(compare))
				.Case(-1, () => result = false) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = true); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionGreaterThan(this float value, float compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<float>(compare))
				.Case(-1, () => result = false) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = true); // Value is after the compare value.
			return result;
		}

		#endregion Greater Than

		#region Less Than

		public static Boolean ExpressionLessThan(this object value, object compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<object>(compare))
				.Case(-1, () => result = true) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = false); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionLessThan(this DateTime value, DateTime compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<DateTime>(compare))
				.Case(-1, () => result = true) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = false); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionLessThan(this int value, int compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<int>(compare))
				.Case(-1, () => result = true) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = false); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionLessThan(this Decimal value, Decimal compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<Decimal>(compare))
				.Case(-1, () => result = true) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = false); // Value is after the compare value.
			return result;
		}

		public static Boolean ExpressionLessThan(this float value, float compare) {
			bool result = false;
			// 0 = Same, -1 = Value is Before compare, 1 = Value is After compare
			CodedThought.Core.Switch.On(value.ExpressionComparesTo<float>(compare))
				.Case(-1, () => result = true) // Value is before the compare value.
				.Case(0, () => result = false) // Value is the same as the compare value.
				.Case(1, () => result = false); // Value is after the compare value.
			return result;
		}

		#endregion Less Than

		#region Greater Than or Equal To

		public static Boolean ExpressionGreaterThanEqualTo(this object value, object compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionGreaterThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionGreaterThanEqualTo(this DateTime value, DateTime compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionGreaterThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionGreaterThanEqualTo(this int value, int compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionGreaterThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionGreaterThanEqualTo(this Decimal value, Decimal compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionGreaterThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionGreaterThanEqualTo(this float value, float compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionGreaterThan(compare)) result = true;
			return result;
		}

		#endregion Greater Than or Equal To

		#region Less Than or Equal To

		public static Boolean ExpressionLessThanEqualTo(this object value, object compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionLessThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionLessThanEqualTo(this DateTime value, DateTime compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionLessThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionLessThanEqualTo(this int value, int compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionLessThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionLessThanEqualTo(this Decimal value, Decimal compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionLessThan(compare)) result = true;
			return result;
		}

		public static Boolean ExpressionLessThanEqualTo(this float value, float compare) {
			bool result = false;
			if (value.ExpressionEquals(compare)) result = true;
			if (value.ExpressionLessThan(compare)) result = true;
			return result;
		}

		#endregion Less Than or Equal To

		#region Contains

		public static Boolean ExpressionContains(this object value, object compare) => value.ExpressionContains<object>(compare);

		public static Boolean ExpressionContains(this String value, String compare) => value.ExpressionContains<String>(compare);

		#endregion Contains

		public static IEnumerable<ExpressionModifiers> GetValues(this ExpressionModifiers value) => Enum.GetValues(typeof(ExpressionModifiers)).Cast<ExpressionModifiers>();

		public static List<T> EnumToList<T>() {
			Type enumType = typeof(T);

			// Can't use type constraints on value types, so have to do check like this
			if (enumType.BaseType != typeof(Enum))
				throw new ArgumentException("T must be of type System.Enum");

			Array enumValArray = Enum.GetValues(enumType);

			List<T> enumValList = new(enumValArray.Length);

			foreach (int val in enumValArray) {
				enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
			}

			return enumValList;
		}

		#endregion Operand Extensions.

		#region Modifier Extensions.

		// Is Upper case
		public static Boolean ExpressionUpper(this String value) => value.ToUpper() == value;

		// Is Lower case
		public static Boolean ExpressionLower(this String value) => value.ToLower() == value;

		// Case insensitivity test.
		public static Boolean ExpressionCaseInsensitive(this String value) => value.ToLower() == value.ToLower();

		// Round Decimal
		public static Decimal ExpressionRound(this Decimal value) => Math.Round(value);

		// Maximum String
		public static Boolean ExpressionMax(this String value, int max) => (value.Length <= max);

		// Minimum String
		public static Boolean ExpressionMin(this String value, int min) => (value.Length >= min);

		// Maximum Decimal
		public static Boolean ExpressionMax(this Decimal value, int max) => (value <= max);

		// Minimum Decimal
		public static Boolean ExpressionMin(this Decimal value, int min) => (value >= min);

		// Maximum Float
		public static Boolean ExpressionMax(this float value, int max) => (value <= max);

		// Minimum Float
		public static Boolean ExpressionMin(this float value, int min) => (value >= min);

		// Maximum Int
		public static Boolean ExpressionMax(this int value, int max) => (value <= max);

		// Minimum Int
		public static Boolean ExpressionMin(this int value, int min) => (value >= min);

		// Email
		public static Boolean ExpressionIsEmail(this String value) => System.Text.RegularExpressions.Regex.IsMatch(value, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

		#endregion Modifier Extensions.
	}
}