namespace CodedThought.Core.Extensions {

	public static class BaseExtensions {

		public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector) => items == null ? Enumerable.Empty<T>() : items.Concat(items.SelectMany(i => childSelector(i).Flatten(childSelector)));

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
				if (test < decimalMin)
					return true;
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

		public static TypeCode GetNumericType(this object o) => o.IsNumericType() ? Type.GetTypeCode(o.GetType()) : TypeCode.Object;

		public static T Convert<T>(this string input) {
			try {
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
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
								if (val != null)
									targetPI.SetValue(obj, val);
								continue;
							} catch { }
						}
					}
				}
			} catch (Exception ex) {
				throw;
			}
		}

		/// <summary>Splits the on capitals.</summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static string SplitOnCapitals(this string input) => System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])(?![A-Z])", " $1").Trim();

		/// <summary>Gets the exception message recursively including any inner exceptions.</summary>
		/// <param name="ex">The ex.</param>
		/// <returns></returns>
		public static string GetAllMessages(this Exception ex, string separator = "\r\nInnerException: ") => ex.InnerException == null ? ex.Message : ex.Message + separator + GetAllMessages(ex.InnerException, separator);

		/// <summary>
		/// Converts the current date/time in UNIX format to a standard .NET DateTime.
		/// </summary>
		/// <param name="unixTimestamp"></param>
		/// <param name="kind"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
        public static DateTime ConvertFromUnixTimestamp(this long unixTimestamp, DateTimeKind kind = DateTimeKind.Local)
        {
            // Define the Unix epoch start time as UTC
            DateTime utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimestamp);

            // Adjust based on the specified DateTimeKind
            return kind switch
            {
                DateTimeKind.Utc => utcDateTime, // Already UTC
                DateTimeKind.Local => utcDateTime.ToLocalTime(), // Convert to local time
                DateTimeKind.Unspecified => DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified), // Change kind to Unspecified
                _ => throw new ArgumentOutOfRangeException(nameof(kind), "Invalid DateTimeKind value"),
            };
        }
        /// <summary>
        /// Converts a standard DateTime to a UNIX timestamp (seconds since Unix epoch).
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>The UNIX timestamp as a long.</returns>
        public static long ConvertToUnixTimestamp(this DateTime dateTime)
        {
            // Ensure the DateTime is in UTC
            DateTime utcDateTime;

            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    utcDateTime = dateTime;
                    break;
                case DateTimeKind.Local:
                    utcDateTime = dateTime.ToUniversalTime();
                    break;
                case DateTimeKind.Unspecified:
                default:
                    // Assuming Unspecified DateTime is local
                    utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
                    break;
            }

            // Define the Unix epoch start time
            DateTimeOffset unixEpoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // Calculate the total seconds elapsed since the Unix epoch
            long unixTimestamp = (long) (utcDateTime - unixEpoch).TotalSeconds;

            return unixTimestamp;
        }
    }
}