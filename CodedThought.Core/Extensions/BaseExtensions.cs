namespace CodedThought.Core.Extensions {

	public static class BaseExtensions {

		public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector) {
			return items == null ? Enumerable.Empty<T>() : items.Concat(items.SelectMany(i => childSelector(i).Flatten(childSelector)));
		}

		public static bool IsNumericType(this object o) {
			try {
				o = Convert<Decimal>(o.ToString());
				o = Convert<Double>(o.ToString());
				o = Convert<int>(o.ToString());
				o = Convert<Int32>(o.ToString());
				o = Convert<Int64>(o.ToString());
			} catch { }

			switch (Type.GetTypeCode(o.GetType())) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
				case TypeCode.Int32:
					return true;

				default:
					return false;
			}
		}

		/// <summary>Determines whether this instance is a double.</summary>
		/// <param name="o">The o.</param>
		/// <returns><c>true</c> if the specified value is double; otherwise, <c>false</c>.</returns>
		public static bool IsDouble(this object o) {
			const double decimalMin = (double)decimal.MinValue;
			const double decimalMax = (double)decimal.MaxValue;
			try {
				double test = Convert<Double>(o.ToString());
				if (test < decimalMin) return true;
				return test > decimalMax && false;
			} catch { return false; }
		}

		public static object ChangeToNumericType(this object o) {
			try {
				o = Convert<Double>(o.ToString());
				o = Convert<Decimal>(o.ToString());
				o = Convert<Int64>(o.ToString());
				o = Convert<int>(o.ToString());
				o = Convert<Int32>(o.ToString());
			} catch { }

			switch (Type.GetTypeCode(o.GetType())) {
				case TypeCode.Byte:
				case TypeCode.SByte:
					return System.Convert.ChangeType(o, TypeCode.Byte);

				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int64:
				case TypeCode.Int32:
					return System.Convert.ChangeType(o, TypeCode.Int32);

				case TypeCode.Decimal:
					return System.Convert.ChangeType(o, TypeCode.Decimal);

				case TypeCode.Double:
					return System.Convert.ChangeType(o, TypeCode.Double);

				case TypeCode.Single:
					return System.Convert.ChangeType(o, TypeCode.Single);

				default:
					// Just return the object if nothing works.
					return o;
			}
		}

		public static TypeCode GetNumericType(this object o) {
			return o.IsNumericType() ? Type.GetTypeCode(o.GetType()) : TypeCode.Object;
		}

		public static T Convert<T>(this string input) {
			try {
				var converter = TypeDescriptor.GetConverter(typeof(T));
				if (converter != null) {
					// Cast ConvertFromString(string text) : object to (T)
					return (T)converter.ConvertFromString(input);
				}
				return default(T);
			} catch (NotSupportedException) {
				return default(T);
			}
		}

		/// <summary>Sets the base properties from the passed base object.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="baseObj">The base object.</param>
		public static void SetBaseProperties<TEntity, T>(this TEntity obj, T baseObj) where T : class {
			try {
				PropertyInfo[] sourceProperties = typeof(T).GetProperties();
				PropertyInfo[] targetProperties = typeof(TEntity).GetProperties();

				foreach (PropertyInfo sourcePI in sourceProperties) {
					PropertyInfo targetPI = targetProperties.FirstOrDefault(p => p.Name == sourcePI.Name);
					if (targetPI != null) {
						if (targetPI.PropertyType == sourcePI.PropertyType) {
							object val = sourcePI.GetValue(baseObj, null);
							try {
								if (val != null) targetPI.SetValue(obj, val);
								continue;
							} catch { }
						}
					}
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Splits the on capitals.</summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static string SplitOnCapitals(this string input) {
			return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])(?![A-Z])", " $1").Trim();
		}

		/// <summary>Gets the exception message recursively including any inner exceptions.</summary>
		/// <param name="ex">The ex.</param>
		/// <returns></returns>
		public static string GetAllMessages(this Exception ex, string separator = "\r\nInnerException: ") {
			return ex.InnerException == null ? ex.Message : ex.Message + separator + GetAllMessages(ex.InnerException, separator);
		}
	}
}