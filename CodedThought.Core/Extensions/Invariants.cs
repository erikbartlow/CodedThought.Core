using System.Globalization;

namespace CodedThought.Core.Extensions {

	public static class Invariants {

		/// <summary>Number format information for invariant culture</summary>
		public static readonly NumberFormatInfo NumberFormat;

		/// <summary>Date time format information for invariant culture</summary>
		public static readonly DateTimeFormatInfo DateTimeFormat;

		/// <summary>Statik DataHelper contructor'ı. Varsayılan bağlantı string'i ve bağlantı kültürü parametlerini initialize eder.</summary>
		static Invariants() {
			NumberFormat = NumberFormatInfo.InvariantInfo;
			DateTimeFormat = DateTimeFormatInfo.InvariantInfo;
		}

		public static bool IsIntegerType(object value) {
			return value != null &&
			   (value is Int32 ||
				value is Int64 ||
				value is Int16);
		}

		public static string ToInvariant(this int value) {
			return value.ToString(Invariants.NumberFormat);
		}

		public static string ToInvariant(this Int64 value) {
			return value.ToString(Invariants.NumberFormat);
		}

		public static string ToInvariant(this Double value) {
			return value.ToString(Invariants.NumberFormat);
		}

		public static string ToInvariant(this Decimal value) {
			return value.ToString(Invariants.NumberFormat);
		}

		/// <summary>
		/// <p>Tries to converts an ID's string representation to its numerical ID value (Int64).</p>
		/// <p>Unlike <see cref="Int64" />, <c>null</c>, empty string and all other invalid strings results in <see cref="Int64" /> value (not an exception).</p>
		/// </summary>
		/// <param name="str">String representation of an ID.</param>
		/// <returns>Numerical ID value or Null.Int64 if null, empty, or invalid string.</returns>
		/// <seealso cref="TryParseID(string)" />
		/// <seealso cref="Int64.Parse(string)" />
		public static Int64? TryParseID(this string str) {
			Int64 id;
			return Int64.TryParse(str, out id) ? id : null;
		}

		/// <summary>
		/// <p>Tries to converts an ID's string representation to its numerical ID value (Int64).</p>
		/// <p>Unlike <see cref="Int64" />, <c>null</c>, empty string and all other invalid strings results in <see cref="Int64" /> value (not an exception).</p>
		/// </summary>
		/// <param name="str">String representation of an ID.</param>
		/// <returns>Numerical ID value or Null.Int64 if null, empty, or invalid string.</returns>
		/// <seealso cref="TryParseID(string)" />
		/// <seealso cref="Int64.Parse(string)" />
		public static Int32? TryParseID32(this string str) {
			Int32 id;
			return Int32.TryParse(str, out id) ? id : null;
		}

		/// <summary>Converts an ID value, to its string representation.</summary>
		/// <param name="id">ID value.</param>
		/// <returns>If <paramref name="id" /> has <see cref="Int64" /> value, <c>String.Empty</c>, otherwise its string representation</returns>
		public static string IDString(this Int64? id) {
			return id == null ? String.Empty : id.ToString();
		}
	}
}