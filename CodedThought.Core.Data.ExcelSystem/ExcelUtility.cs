using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using CodedThought.Core.Exceptions;
using CodedThought.Core.Extensions;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CodedThought.Core.Data.ExcelSystem {

	public class ExcelUtility {

		public enum EncodingToUse {
			UTF8 = 0,
			UTF7 = 1,
			ISO = 2
		}

		public static EncodingToUse CurrentFileEncoding { get; set; }

		public static Encoding FileEncodingToUse {
			get {
				return CurrentFileEncoding switch {
					EncodingToUse.UTF7 => Encoding.UTF7,
					EncodingToUse.ISO => Encoding.GetEncoding("ISO-8859-1"),
					_ => Encoding.UTF8,
				};
			}
		}

		/// <summary>Lists to data table.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static DataTable ListToDataTable<T>(List<T> list) {
			DataTable dt = new();
			try {
				List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

				foreach (PropertyInfo info in properties) {
					ExcelColumnAttribute excelColumn = GetExcelColumnAttribute(info);
					DataColumn col = new(info.Name, GetNullableType(info.PropertyType));
					// Capture the excel column properties for later use.
					foreach (PropertyInfo prop in typeof(ExcelColumnAttribute).GetProperties()) {
						col.ExtendedProperties.Add(key: prop.Name, value: prop.GetValue(excelColumn));
					}
					dt.Columns.Add(col);
				}
				foreach (T t in list) {
					DataRow row = dt.NewRow();
					foreach (PropertyInfo info in properties) {
						row[info.Name] = info.GetValue(t, null) ?? DBNull.Value;
					}
					dt.Rows.Add(row);
				}
				return dt;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Handles the boolean conversion.</summary>
		/// <param name="val">The value.</param>
		/// <returns></returns>
		private static bool HandleBoolean(object val) {
			if (ReferenceEquals(val, null)) {
				return false;
			} else {
				try {
					return bool.Parse(val.ToString());
				} catch { return false; }
			}
		}

		/// <summary>Gets the excel column attribute.</summary>
		/// <param name="prop">The property.</param>
		/// <returns></returns>
		private static ExcelColumnAttribute GetExcelColumnAttribute(PropertyInfo prop) {
			ExcelColumnAttribute attrib = (ExcelColumnAttribute)Attribute.GetCustomAttribute(prop, typeof(ExcelColumnAttribute));
			attrib ??= new ExcelColumnAttribute() {
				Ignore = true
			};
			return attrib;
		}

		/// <summary>Gets the excel column attribute by the excel column name.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="excelColumnName">Name of the excel column.</param>
		/// <returns></returns>
		private static ExcelColumnAttribute GetExcelColumnAttribute<T>(string excelColumnName) {
			ExcelColumnAttribute excelColumnAttribute = null;
			PropertyInfo[] properties = typeof(T).GetProperties();
			foreach (PropertyInfo prop in properties) {
				excelColumnAttribute = prop.GetCustomAttribute<ExcelColumnAttribute>();
				if (excelColumnAttribute != null) {
					if (excelColumnAttribute.ColumnName.ToUpper() == excelColumnName.ToUpper()) {
						break;
					} else { continue; }
				}
			}
			return excelColumnAttribute;
		}

		/// <summary>Gets the property from excel column.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="excelColumnName">Name of the excel column.</param>
		/// <returns></returns>
		private static PropertyInfo GetPropertyFromExcelColumn<T>(string excelColumnName) {
			ExcelColumnAttribute excelColumnAttribute = null;
			PropertyInfo property = null;
			PropertyInfo[] properties = typeof(T).GetProperties();
			foreach (PropertyInfo prop in properties) {
				excelColumnAttribute = prop.GetCustomAttribute<ExcelColumnAttribute>();
				if (excelColumnAttribute != null) {
					if (excelColumnAttribute.ColumnName.ToUpper() == excelColumnName.ToUpper()) {
						property = prop;
						break;
					} else { continue; }
				}
			}
			return property;
		}

		/// <summary>Gets the type of the nullable.</summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		private static Type GetNullableType(Type t) {
			Type returnType = t;
			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
				returnType = Nullable.GetUnderlyingType(t);
			}
			return returnType;
		}

		//private static object FormatValueForType(Type t, string value) {
		//    if (String.IsNullOrEmpty(value)) return string.Empty;
		//    switch (t.Name.ToUpper()) {
		//        case "DATETIME":
		//            DateTime dt = DateTime.TryParse(value, out dt) ? dt : DateTime.MinValue;
		//        case "INT":
		//        case "INT32":
		//        case "INT64":
		//        case "DOUBLE":
		//        case "DECIMAL":
		//        case "STRING":
		//        default:
		//            return value;
		//    }
		//}
		/// <summary>Determines whether the specified object is format table.</summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		private static bool IsFormattable(object obj) {
			return IsNumeric(obj) || IsDate(obj);
		}

		/// <summary>Determines whether the specified object is numeric.</summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		private static bool IsNumeric(object obj) {
			if (Equals(obj, null)) {
				return false;
			}
			int testInt = int.MinValue;
			int.TryParse(obj.ToString(), out testInt);
			if (!Equals(testInt, int.MinValue)) {
				obj = testInt;
			}
			Type objType = obj.GetType();
			objType = Nullable.GetUnderlyingType(objType) ?? objType;

			return objType.IsPrimitive
				? objType != typeof(bool) &&
				   objType != typeof(char) &&
				   objType != typeof(IntPtr) &&
				   objType != typeof(UIntPtr)
				: objType == typeof(decimal);
		}

		/// <summary>Determines whether the specified object is date.</summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		private static bool IsDate(object obj) {
			DateTime dateValue = DateTime.MinValue;
			DateTime.TryParse(obj.ToString(), out dateValue);
			// If the parsed value later than the min value then it is a valid date.
			return dateValue > DateTime.MinValue;
		}

		/// <summary>Determines whether [is nullable type] [the specified type].</summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		private static bool IsNullableType(Type type) => type == typeof(string) ||
				 type.IsArray ||
				 (type.IsGenericType &&
				  type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));

		/// <summary>Opens the excel file as DataSet.</summary>
		/// <param name="fullFileName">Full name of the file.</param>
		/// <returns></returns>
		/// <exception cref="CodedThoughtException"></exception>
		public static DataSet OpenExcelFileAsDataSet(string fullFileName, bool hasHeaders = false) {
			try {
				Stream excelStream;
				DataSet dsExcel = new();
				// Open a file stream to the file.
				excelStream = File.Open(fullFileName, FileMode.Open, FileAccess.Read);
				using (StreamReader sr = new(fullFileName, FileEncodingToUse)) {
					return OpenExcelFileAsDataSet(sr, System.IO.Path.GetExtension(fullFileName), hasHeaders);
				}
			} catch (IOException ioEx) {
				throw new CodedThoughtException("There was an error accessing the file.", ioEx);
			} catch (CodedThoughtException hpEx) {
				throw new CodedThoughtException(hpEx.Message, hpEx);
			} catch (Exception ex) {
				throw new CodedThoughtException(ex.Message, ex);
			}
		}

		public static DataSet OpenExcelFileAsDataSet(StreamReader uploadStream, string fileExtension, bool hasHeaders = false) {
			try {
				return OpenExcelFileAsDataSet(uploadStream, fileExtension, hasHeaders);
			} catch (IOException ioEx) {
				throw new CodedThoughtException("There was an error accessing the file.", ioEx);
			} catch (CodedThoughtException hpEx) {
				throw new CodedThoughtException(hpEx.Message, hpEx);
			} catch (Exception ex) {
				throw new CodedThoughtException(ex.Message, ex);
			}
		}

		/// <summary>Opens the excel file as data set.</summary>
		/// <param name="uploadStream"> The upload stream.</param>
		/// <param name="fileExtension">The file extension.</param>
		/// <returns></returns>
		/// <exception cref="CodedThoughtException"></exception>
		public static DataSet OpenExcelFileAsDataSet(FileStream uploadStream, string fileExtension, bool hasHeaders = false) {
			try {
				return OpenExcelFileAsDataSet((Stream)uploadStream, fileExtension, hasHeaders);
			} catch (IOException ioEx) {
				throw new CodedThoughtException("There was an error accessing the file.", ioEx);
			} catch (CodedThoughtException hpEx) {
				throw new CodedThoughtException(hpEx.Message, hpEx);
			} catch (Exception ex) {
				throw new CodedThoughtException(ex.Message, ex);
			}
		}

		/// <summary>Opens the excel file as data set.</summary>
		/// <param name="uploadStream"> The upload stream.</param>
		/// <param name="fileExtension">The file extension including the period.</param>
		/// <returns></returns>
		/// <remarks>The file extension should contain the leading period.</remarks>
		/// <exception cref="CodedThoughtException"></exception>
		public static DataSet OpenExcelFileAsDataSet(Stream uploadStream, string fileExtension, bool hasHeaders = false) {
			try {
				DataSet dsExcel = new();
				IExcelDataReader? reader = null;
				Switch.On(fileExtension)
					.Case(".xls", () => reader = ExcelReaderFactory.CreateBinaryReader(uploadStream)) // MS Excel 2003-
					.Case(".xlsx", () => reader = ExcelReaderFactory.CreateOpenXmlReader(uploadStream)) // MS Excel +2010
					.Case(".xlsm", () => reader = ExcelReaderFactory.CreateOpenXmlReader(uploadStream)); // MS Excel +2010 Macro Enabled

				dsExcel = reader.AsDataSet(true, hasHeaders);
				reader.Close();
				return dsExcel;
			} catch (IOException ioEx) {
				throw new CodedThoughtException("There was an error accessing the file.", ioEx);
			} catch (CodedThoughtException hpEx) {
				throw new CodedThoughtException(hpEx.Message, hpEx);
			} catch (Exception ex) {
				throw new CodedThoughtException(ex.Message, ex);
			}
		}

		/// <summary>Opens the CSV file as <see cref="DataSet" />.</summary>
		/// <param name="uploadStream">The upload stream.</param>
		/// <param name="delimiter">   The delimiter.</param>
		/// <param name="hasHeaders">  if set to <c>true</c> [has headers].</param>
		/// <returns></returns>
		public static DataSet OpenCSVFileAsDataSet(Stream uploadStream, string delimiter, bool hasHeaders = true) {
			// Variable to track the current row in case a row is not importable with the expected conditions.
			int currentImportRow = 1;
			string errorRow = string.Empty;

			try {
				string csvSplitRegexPattern = $"(?:^|{delimiter})(\"(?:[^\"])*\"|[^{delimiter}]*)";
				Regex csvSplit = new(csvSplitRegexPattern, RegexOptions.Compiled);
				using (StreamReader csv = new(uploadStream, FileEncodingToUse)) {
					DataTable dt = new();
					string thisRow;
					string[] currentRow;
					string[] headerRow;
					string oldDelimiter = delimiter;

					// Check for double delimiters
					if (delimiter.Length > 1) {
						delimiter = delimiter.Substring(0, 1);
					}

					// Add the data table headers if necessary.
					if (hasHeaders) {
						if ((thisRow = csv.ReadLine()) != null) {
							if (oldDelimiter.Length > 1)
								thisRow = thisRow.Replace(oldDelimiter, delimiter);
							headerRow = thisRow.Split(delimiter, csvSplit);
							foreach (string col in headerRow) {
								dt.Columns.Add(new DataColumn(col, typeof(string)));
							}
						}
					}
					// Load up the rest of the rows.
					while ((thisRow = csv.ReadLine()) != null) {
						errorRow = thisRow;
						if (oldDelimiter.Length > 1)
							thisRow = thisRow.Replace(oldDelimiter, delimiter);
						currentRow = thisRow.Split(delimiter, csvSplit);
						// If there isn't a header row then add them based on the first row of data.
						if (dt.Columns.Count == 0)
							for (int x = 0; x <= currentRow.Length - 1; x++)
								dt.Columns.Add($"Column{x}");
						DataRow newRow = dt.NewRow();
						for (int i = 0; i <= currentRow.Length - 1; i++) {
							newRow[i] = currentRow[i];
						}
						dt.Rows.Add(newRow);
						currentImportRow++;
					}
					DataSet ds = new();
					ds.Tables.Add(dt);
					return ds;
				}
			} catch (Exception ex) {
				throw new Exception($"Error occurred while opening CSV to DataSet.  The row in error is #{currentImportRow + 1} {errorRow}.", ex);
			}
		}

		public static DataSet OpenCSVFileAsDataSet(Stream uploadStream, string delimiter, bool hasHeaders = false, int columnCount = 0) {
			// Variable to track the current row in case a row is not importable with the expected conditions.
			int currentImportRow = 1;
			string errorRow = string.Empty;

			try {
				using (StreamReader csv = new(uploadStream, FileEncodingToUse)) {
					DataTable dt = new();
					string thisRow;
					string[] currentRow;
					string oldDelimiter = delimiter;

					if (hasHeaders)
						return OpenCSVFileAsDataSet(uploadStream, delimiter, hasHeaders);
					if (hasHeaders == false && columnCount == 0) { throw new Exception("When hasHeaders is set to false a column count must be set."); }

					// Check for double delimiters
					if (delimiter.Length > 1) {
						delimiter = delimiter.Substring(0, 1);
					}

					// Add the data table headers if necessary.
					for (int x = 0; x <= columnCount - 1; x++)
						dt.Columns.Add($"Column{x}");

					// Load up the rest of the rows.
					while ((thisRow = csv.ReadLine()) != null) {
						errorRow = thisRow;
						if (oldDelimiter.Length > 1)
							thisRow = thisRow.Replace(oldDelimiter, delimiter);
						currentRow = thisRow.Split(delimiter.ToCharArray(), StringSplitOptions.None);
						DataRow newRow = dt.NewRow();
						for (int i = 0; i <= currentRow.Length - 1; i++) {
							newRow[i] = currentRow[i];
						}
						dt.Rows.Add(newRow);
						currentImportRow++;
					}
					DataSet ds = new();
					ds.Tables.Add(dt);
					return ds;
				}
			} catch (Exception ex) {
				throw new Exception($"Error occurred while opening CSV to DataSet.  The row in error is #{currentImportRow + 1} {errorRow}.", ex);
			}
		}

		public static DataSet OpenCSVFileAsDataSet<T>(Stream uploadStream, string delimiter, bool hasHeaders = true) {
			// Variable to track the current row in case a row is not importable with the expected conditions.
			int currentImportRow = 1;
			string errorRow = string.Empty;
			try {
				using (StreamReader csv = new(uploadStream, FileEncodingToUse)) {
					DataTable dt = new();
					string thisRow;
					string[] currentRow;
					string[] headerRow;
					string oldDelimiter = delimiter;
					// Check for double delimiters
					if (delimiter.Length > 1) {
						delimiter = delimiter.Substring(0, 1);
					}

					// Add the data table headers if necessary.
					if (hasHeaders) {
						if ((thisRow = csv.ReadLine()) != null) {
							if (oldDelimiter.Length > 1)
								thisRow = thisRow.Replace(oldDelimiter, delimiter);
							headerRow = thisRow.Split(delimiter.ToCharArray());
							PropertyInfo[] excelProperties = typeof(T).GetProperties();
							foreach (string col in headerRow) {
								// Get the column data type from the excel column attribute.
								PropertyInfo prop = GetPropertyFromExcelColumn<T>(col);
								dt.Columns.Add(new DataColumn(col, GetNullableType(prop.PropertyType)));
							}
						}
					} else {
						PropertyInfo[] properties = typeof(T).GetProperties();
						for (int i = 0; i <= properties.Length - 1; i++) {
							dt.Columns.Add(new DataColumn(properties[i].Name, GetNullableType(properties[i].PropertyType)));
						}
					}
					// Load up the rest of the rows.
					while ((thisRow = csv.ReadLine()) != null) {
						errorRow = thisRow;
						if (oldDelimiter.Length > 1)
							thisRow = thisRow.Replace(oldDelimiter, delimiter);
						currentRow = thisRow.Split(delimiter.ToCharArray(), StringSplitOptions.None);
						// If there isn't a header row then add them based on the first row of data.
						if (dt.Columns.Count == 0)
							for (int x = 0; x <= currentRow.Length - 1; x++)
								dt.Columns.Add($"Column{x}");
						DataRow newRow = dt.NewRow();
						for (int i = 0; i <= currentRow.Length - 1; i++) {
							try {
								newRow[i] = (string.IsNullOrEmpty(currentRow[i]) ? string.Empty : currentRow[i]);
							} catch (Exception parseEx) {
								throw parseEx;
							}
						}
						dt.Rows.Add(newRow);
						currentImportRow++;
					}
					DataSet ds = new();
					ds.Tables.Add(dt);
					return ds;
				}
			} catch (Exception ex) {
				throw new Exception($"Error occurred while opening CSV to DataSet.  The row in error is #{currentImportRow + 1} {errorRow}.", ex);
			}
		}

		public static DataSet OpenCSVFileAsDataSet<T>(Stream uploadStream, string delimiter, bool hasHeaders = false, int columnCount = 0) {
			// Variable to track the current row in case a row is not importable with the expected conditions.
			int currentImportRow = 1;
			string errorRow = string.Empty;
			try {
				using (StreamReader csv = new(uploadStream, FileEncodingToUse)) {
					DataTable dt = new();
					string thisRow;
					string[] currentRow;
					string oldDelimiter = delimiter;
					PropertyInfo[] properties = typeof(T).GetProperties();
					columnCount = properties.Length;
					if (hasHeaders)
						return OpenCSVFileAsDataSet(uploadStream, delimiter, hasHeaders);
					if (hasHeaders == false && columnCount == 0) { throw new Exception("When hasHeaders is set to false a column count must be set."); }

					// Check for double delimiters
					if (delimiter.Length > 1) {
						delimiter = delimiter.Substring(0, 1);
					}

					// Add the data table headers if necessary.
					for (int x = 0; x <= columnCount - 1; x++)
						dt.Columns.Add(new DataColumn($"Column{x}", GetNullableType(properties[x].PropertyType)));

					// Load up the rest of the rows.
					while ((thisRow = csv.ReadLine()) != null) {
						errorRow = thisRow;
						if (oldDelimiter.Length > 1)
							thisRow = thisRow.Replace(oldDelimiter, delimiter);
						currentRow = thisRow.Split(delimiter.ToCharArray(), StringSplitOptions.None);
						DataRow newRow = dt.NewRow();
						for (int i = 0; i <= currentRow.Length - 1; i++) {
							if (!string.IsNullOrEmpty(currentRow[i]))
								newRow[i] = currentRow[i];
						}
						dt.Rows.Add(newRow);
						currentImportRow++;
					}
					DataSet ds = new();
					ds.Tables.Add(dt);
					return ds;
				}
			} catch (Exception ex) {
				throw new Exception($"Error occurred while opening CSV to DataSet.  The row in error is #{currentImportRow + 1} {errorRow}.", ex);
			}
		}
	}

	/// <summary>Maps a class to an Excel Column or XML Element</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ExcelColumnAttribute : Attribute {

		public enum ExcelStyleFormats {
			NoStyle = 0,
			Numeric = 1,
			NumericTwoDecimals = 2,
			NumericTenths = 3,
			NumericTenthsTwoDecimals = 4,
			Percentage = 9,
			PercentageTwoDecimals = 10,
			DateMMDDYYYY = 14,
			DateDMMMYYYY = 15,
			DateMMMYYYY = 16,
			DateMDYYYHMM = 22
		}

		#region Properties

		/// <summary>Gets or sets the name of the column.</summary>
		/// <value>The name of the column.</value>
		public string ColumnName { get; set; }

		/// <summary>Gets or sets the column order.</summary>
		/// <value>The column order.</value>
		public int ColumnOrder { get; set; }

		/// <summary>Gets or sets the null default value.</summary>
		/// <value>The null default value.</value>
		/// <remarks>This value will be used in lieu of NULL.</remarks>
		public string NullDefaultValue { get; set; }

		/// <summary>Gets or sets the type of the column.</summary>
		/// <value>The type of the column.</value>
		public Type ColumnType { get; set; }

		/// <summary>Gets or sets the format string.</summary>
		/// <value>The format string.</value>
		public String Format { get; set; }

		/// <summary>Gets or sets a value indicating whether this <see cref="ExcelColumnAttribute" /> is bold.</summary>
		/// <value><c>true</c> if bold; otherwise, <c>false</c>.</value>
		public bool Bold { get; set; }

		/// <summary>Gets or sets the column alignment.</summary>
		/// <value>The column alignment.</value>
		public HorizontalAlignmentValues ColumnAlignment { get; set; }

		/// <summary>Gets or sets a value indicating whether this <see cref="ExcelColumnAttribute" /> is ignore.</summary>
		/// <value><c>true</c> if ignore; otherwise, <c>false</c>.</value>
		public bool Ignore { get; set; }

		#endregion Properties

		#region Constructors

		/// <summary>Prevents a default instance of the <see cref="ExcelColumnAttribute" /> class from being created.</summary>
		public ExcelColumnAttribute() {
			ColumnName = string.Empty;
			NullDefaultValue = string.Empty;
			Ignore = false;
			Format = String.Empty;
			ColumnAlignment = HorizontalAlignmentValues.Left;
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="ignore">if set to <c>true</c> ignore for output to excel.</param>
		public ExcelColumnAttribute(bool ignore) : this() => Ignore = true;

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">Name of the column.</param>
		public ExcelColumnAttribute(string columnName, int order) :
		   this(columnName, order, typeof(string), String.Empty, String.Empty) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		public ExcelColumnAttribute(string columnName, int order, string defaultNullValue) :
		   this(columnName, order, typeof(string), defaultNullValue, String.Empty) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		public ExcelColumnAttribute(string columnName, int order, string defaultNullValue, string format) :
		   this(columnName, order, typeof(string), defaultNullValue, format) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		public ExcelColumnAttribute(string columnName, int order, string defaultNullValue, ExcelStyleFormats format) :
		   this(columnName, order, typeof(string), defaultNullValue, format) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="order">           The order.</param>
		/// <param name="columnType">      Type of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		/// <param name="format">          The format.</param>
		public ExcelColumnAttribute(string columnName, int order, Type columnType, string defaultNullValue, string format) :
		   this(columnName, order, columnType, defaultNullValue, format, false, HorizontalAlignmentValues.Left) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="order">           The order.</param>
		/// <param name="columnType">      Type of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		/// <param name="format">          The format.</param>
		public ExcelColumnAttribute(string columnName, int order, Type columnType, string defaultNullValue, ExcelStyleFormats format) :
		   this(columnName, order, columnType, defaultNullValue, format, false, HorizontalAlignmentValues.Left) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="order">           The order.</param>
		/// <param name="columnType">      Type of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		/// <param name="format">          The format.</param>
		public ExcelColumnAttribute(string columnName, int order, Type columnType, string defaultNullValue, string format, HorizontalAlignmentValues aligned) :
		   this(columnName, order, columnType, defaultNullValue, format, false, aligned) {
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="order">           The order.</param>
		/// <param name="columnType">      Type of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		/// <param name="format">          The format.</param>
		public ExcelColumnAttribute(string columnName, int order, Type columnType, string defaultNullValue, ExcelStyleFormats format, HorizontalAlignmentValues aligned) :
		   this(columnName, order, columnType, defaultNullValue, format, false, aligned) {
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableAttribute" /> class.</summary>
		/// <param name="containerName">Name of the container.</param>
		/// <param name="viewName">     Name of the view container.</param>
		/// <remarks>If a view is set then it will be used during all Get methods. All columns in the view must match those in the DataColumn attributes of the data entity.</remarks>
		public ExcelColumnAttribute(string columnName, int order, Type columnType, string defaultNullValue, string format, bool bold, HorizontalAlignmentValues aligned) : this() {
			ColumnName = columnName;
			ColumnOrder = order;
			NullDefaultValue = defaultNullValue;
			ColumnType = columnType;
			Ignore = false;
			Format = format;
			Bold = bold;
			ColumnAlignment = aligned;
		}

		/// <summary>Initializes a new instance of the <see cref="ExcelColumnAttribute" /> class.</summary>
		/// <param name="columnName">      Name of the column.</param>
		/// <param name="order">           The order.</param>
		/// <param name="columnType">      Type of the column.</param>
		/// <param name="defaultNullValue">The default null value.</param>
		/// <param name="format">          Built in format.</param>
		/// <param name="bold">            if set to <c>true</c> [bold].</param>
		public ExcelColumnAttribute(string columnName, int order, Type columnType, string defaultNullValue, ExcelStyleFormats format, bool bold, HorizontalAlignmentValues aligned) : this() {
			ColumnName = columnName;
			ColumnOrder = order;
			NullDefaultValue = defaultNullValue;
			ColumnType = columnType;
			Ignore = false;
			Format = ((int)format).ToString();
			Bold = bold;
			ColumnAlignment = aligned;
		}

		#endregion Constructors
	}
}