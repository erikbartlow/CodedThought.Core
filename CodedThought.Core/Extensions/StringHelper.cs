﻿using System.Collections;
using System.Globalization;
using System.Web;

namespace CodedThought.Core.Extensions {

	public static partial class StringHelper {

		/// <summary>
		/// <p>Returns true if <see cref="String" /> is <c>null</c> or empty (zero length)</p>
		/// </summary>
		/// <remarks>
		/// <p>This function might be useful if an empty string is assumed to be <c>null</c>.</p>
		/// <p>This is an extension method, so it can be called directly as <c>str.IsNullOrEmpty()</c>.</p>
		/// </remarks>
		/// <param name="str">String.</param>
		/// <returns>If <paramref name="str" /> is <c>null</c> or empty, <c>true</c></returns>
		public static bool IsEmptyOrNull(this string str) => string.IsNullOrEmpty(str);

		/// <summary>
		/// <p>Returns true if <see cref="String" /> is <c>null</c> or empty (zero length)</p>
		/// </summary>
		/// <remarks>
		/// <p>This function might be useful if an empty string is assumed to be <c>null</c>.</p>
		/// <p>This is an extension method, so it can be called directly as <c>str.IsNullOrEmpty()</c>.</p>
		/// </remarks>
		/// <param name="str">String.</param>
		/// <returns>If <paramref name="str" /> is <c>null</c> or empty, <c>true</c></returns>
		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

		/// <summary>Checks if a string <see cref="String" /> is <c>null</c>, empty or just contains whitespace characters.</summary>
		/// <remarks>
		/// <p><b>Warning:</b> "\n" (line end), "\t" (tab) and some other are also considered as whitespace). To see a list see <see cref="String.Trim()" /> function.</p>
		/// <p>This is an extension method, so it can be called directly as <c>str.IsTrimmedEmpty()</c>.</p>
		/// </remarks>
		/// <param name="str">String.</param>
		/// <returns>If string is null, empty or contains only white space, <c>true</c></returns>
		public static bool IsTrimmedEmpty(this string str) => TrimToNull(str) == null;

		/// <summary>
		/// <p>Removes whitespace characters in the left or right of the <see cref="String" /> string, and if resulting string is empty or null, returns null.</p>
		/// </summary>
		/// <remarks>
		/// <p>
		/// Generally, when a user entered string is going to be saved to database, if user entered an empty string, <c>null</c> or a string of whitespaces, it is stored as <c>null</c>, otherwise it
		/// is expected to remove whitespace at start and end only.
		/// </p>
		/// <p><b>Warning:</b> "\n" (line end), "\t" (tab) and some other are also considered as whitespace). To see a list see <see cref="String.Trim()" /> function.</p>
		/// <p>This is an extension method, so it can be called directly as <c>str.TrimToNull()</c>.</p>
		/// </remarks>
		/// <param name="str">String to be trimmed.</param>
		/// <returns>Trimmed string, result is null if empty.</returns>
		public static string TrimToNull(this string str) {
			if (str.IsNullOrEmpty())
				return null;

			str = str.Trim();

			return str.Length == 0 ? null : str;
		}

		/// <summary>
		/// <p>Removes whitespace characters in the left or right of the <see cref="String" /> string, and if resulting string is empty or null, returns empty.</p>
		/// </summary>
		/// <remarks>
		/// <p>
		/// Generally, when a user entered string is going to be saved to database, if user entered an empty string, <c>null</c> or a string of whitespaces, it is stored as empty string, otherwise it
		/// is expected to remove whitespace at start and end only.
		/// </p>
		/// <p><b>Warning:</b> "\n" (line end), "\t" (tab) and some other are also considered as whitespace). To see a list see <see cref="String.Trim()" /> function.</p>
		/// <p>This is an extension method, so it can be called directly as <c>str.TrimToEmpty()</c>.</p>
		/// </remarks>
		/// <param name="str">String to be trimmed.</param>
		/// <returns>Trimmed string (result won't be null).</returns>
		public static string TrimToEmpty(this string str) => str.IsNullOrEmpty() ? String.Empty : str.Trim();

		/// <summary>Compares two strings ignoring whitespace at the left or right.</summary>
		/// <remarks>
		/// <p><c>null</c> is considered to be empty.</p>
		/// <p><b>Warning:</b> "\n" (line end), "\t" (tab) and some other are also considered as whitespace). To see a list see <see cref="String.Trim()" /> function.</p>
		/// <p>This function can be used to compare a string entered by user to the value in the database for equality.</p>
		/// </remarks>
		/// <param name="string1">String 1.</param>
		/// <param name="string2">String 2.</param>
		/// <returns>If two strings are same trimmed, true</returns>
		public static bool IsTrimmedSame(this string string1, string string2) => (string1 == null || string1.Length == 0) &&
			   (string2 == null || string2.Length == 0)
|| TrimToNull(string1) == TrimToNull(string2);

		/// <summary>If the string's length is over a specified limit, trims its right and adds three points ("...").</summary>
		/// <remarks>This is an extension method, so it can be called directly as <c>str.ThreeDots()</c>.</remarks>
		/// <param name="str">      String.</param>
		/// <param name="maxLength">
		/// Maksimum length for the resulting string. If given as 0, or <paramref name="str" /> is shorter than this value, string returns as is. Otherwise <paramref name="str" /> it is trimmed to be
		/// under this limit in length including "the three dots".
		/// </param>
		/// <returns><paramref name="str" /> itself, or trimmed and three dotted string</returns>
		public static string ThreeDots(this string str, int maxLength) {
			if (str == null)
				return String.Empty;

			if (maxLength == 0 ||
			   str.Length <= maxLength)
				return str;

			if (maxLength > 3)
				maxLength -= 3;
			else
				return "...";

			return str.Substring(0, maxLength) + "...";
		}

		public static string ToSingleLine(this string str) => str.TrimToEmpty().Replace("\r\n", " ").Replace("\n", " ").Trim();

		public static string ToSingleQuoted(this string str) {
			if (String.IsNullOrEmpty(str))
				return emptySingleQuote;

			StringBuilder sb = new();
			QuoteString(str, sb, false);
			return sb.ToString();
		}

		private const string emptySingleQuote = "''";
		private const string emptyDoubleQuote = "\"\"";

		/// <summary>Quotes a string</summary>
		/// <param name="s">          String</param>
		/// <param name="sb">         StringBuilder</param>
		/// <param name="doubleQuote">True to use double quotes</param>
		public static void QuoteString(string s, StringBuilder sb, bool doubleQuote) {
			if (String.IsNullOrEmpty(s)) {
				if (doubleQuote)
					sb.Append(emptyDoubleQuote);
				else
					sb.Append(emptySingleQuote);
				return;
			}

			char c;
			int len = s.Length;

			sb.EnsureCapacity(sb.Length + (s.Length * 2));

			char quoteChar = doubleQuote ? '"' : '\'';
			sb.Append(quoteChar);

			for (int i = 0; i < len; i++) {
				c = s[i];

				switch (c) {
					case '\r':
						sb.Append(@"\r");
						break;

					case '\n':
						sb.Append(@"\n");
						break;

					case '\t':
						sb.Append(@"\t");
						break;

					case '\'':
						if (!doubleQuote)
							sb.Append(@"\'"); // IE doesn't understand \' in double quoted strings!!!
						else
							sb.Append(c);
						break;

					case '\"':
						if (doubleQuote) // IE doesn't understand \" in single quoted strings!!!
							sb.Append(@"\""");
						else
							sb.Append(c);
						break;

					case '\\':
						sb.Append(@"\\");
						break;

					case '/':
						sb.Append(@"\/");
						break;

					default:
						if (c < ' ') {
							sb.Append(@"\u");
							sb.Append(((int)c).ToString("X4", Invariants.NumberFormat));
						} else
							sb.Append(c);
						break;
				}
			}

			sb.Append(quoteChar);
		}

		public static bool IsEmptyOrNull(this ICollection collection) => collection == null || collection.Count == 0;

		public static string SafeSubstring(this string value, int startIndex, int maxLength) {
			if (value.IsNullOrEmpty())
				return String.Empty;

			int len = value.Length;
			if (startIndex >= len || maxLength <= 0)
				return String.Empty;

			return startIndex + maxLength > len ? value.Substring(startIndex) : value.Substring(startIndex, maxLength);
		}

		public static String SanitizeFilename(string s) {
			if (s == null)
				throw new ArgumentNullException("s");

			s = RemoveDiacritics(s);
			s = s.Replace("/", "_");
			s = s.Replace(":", "_");
			s = s.Replace("&", "_");
			s = s.Replace("ı", "i");
			return s.TrimToEmpty();
		}

		public static String RemoveDiacritics(string s) {
			string normalizedString = s.Normalize(NormalizationForm.FormKD);
			StringBuilder stringBuilder = new();

			for (int i = 0; i < normalizedString.Length; i++) {
				Char c = normalizedString[i];
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
					if (c == 'ı')
						c = 'i';

					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString();
		}

		/// <summary>Formats a nullable struct</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">  The source.</param>
		/// <param name="format">  The format string If <c>null</c> use the default format defined for the type of the IFormattable implementation.</param>
		/// <param name="provider">The format provider If <c>null</c> the default provider is used</param>
		/// <param name="empty">   The empty.</param>
		/// <returns>The formatted string or the default value if the source is <c>null</c></returns>
		public static string ToStringDefault<T>(this T? source, string format = null, IFormatProvider provider = null, string empty = null)
		   where T : struct, IFormattable => source.HasValue ? source.Value.ToString(format, provider) : empty ?? "";

		/// <summary>Formats a nullable object</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">  The source.</param>
		/// <param name="format">  The format string If <c>null</c> use the default format defined for the type of the IFormattable implementation.</param>
		/// <param name="provider">The format provider If <c>null</c> the default provider is used</param>
		/// <param name="empty">   The empty.</param>
		/// <returns>The formatted string or the default value if the source is <c>null</c></returns>
		public static string ToStringDefault<T>(this T source, string format = null, IFormatProvider provider = null, string empty = null)
		   where T : class, IFormattable => source != null ? source.ToString(format, provider) : empty ?? "";

		/// <summary>Joins two strings conditionally, by putting separator between if both are non empty</summary>
		public static string Join(string a, string separator, string b) {
			if (a.IsNullOrEmpty() && b.IsNullOrEmpty())
				return "";

			if (a.IsNullOrEmpty())
				return b ?? "";

			return b.IsNullOrEmpty() ? a ?? "" : a + separator + b;
		}

		/// <summary>Splits a string based on the passed delimiter character and an occompanying <see cref="Regex" /> pattern.</summary>
		/// <param name="input">    </param>
		/// <param name="delimiter"></param>
		/// <param name="regex">    </param>
		/// <returns></returns>
		public static string[] Split(this string input, string delimiter, Regex regex) {
			List<string> list = new();
			string? curr;
			foreach (Match match in regex.Matches(input).Cast<Match>()) {
				curr = match.Value;
				if (0 == curr.Length) {
					list.Add("");
				}

				list.Add(curr.TrimStart(delimiter.ToCharArray()));
			}

			return list.ToArray();
		}

		public static string ToQueryString(this Dictionary<string, string> values) {
			string qString = string.Join("&", values.Select(kvp => string.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value))));
			return values.Count > 0 ? "?" + qString : string.Empty;
		}
	}
}