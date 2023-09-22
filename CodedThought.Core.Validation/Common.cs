using System.Data;

namespace CodedThought.Core.Validation {

	public enum UploadTypes {
		FlushNFill = 1,
		MultiTableUpdate = 2
	}

	public enum ValidationResultTypes {
		PASS = 1,
		FAIL = 2,
		FAIL_WITH_MESSAGE = 3
	}

	public enum ExpressionJoin {
		AND = 0,
		OR = 1
	}

	[Flags]
	public enum ExpressionFlags {

		/// <summary>When available this flag will force the evaluator to apply the modifier to the value.</summary>
		Force = 1,

		None = 2
	}

	[Flags]
	public enum ExpressionModifiers {

		/// <summary>Convert case of the target to upper case.</summary>
		Uppercase = 1,

		/// <summary>Convert the case of the target to lower case.</summary>
		Lowercase = 2,

		/// <summary>Do not consider case.</summary>
		CaseInsensitive = 4,

		/// <summary>Round the decimal up to the nearest whole number..</summary>
		RoundUp = 8,

		/// <summary>Round the decimal down to the nears whole number.</summary>
		RoundDown = 16,

		/// <summary>The maximum value the target can be, or the maximum length if the target is a string.</summary>
		Max = 32,

		/// <summary>The minimum value the target can be, or the minimum length if the target is a string.</summary>
		Min = 64,

		/// <summary>The target must be between the passed arguments.</summary>
		Between = 128,

		/// <summary>The target must comply with an email format.</summary>
		Email = 256,

		/// <summary>Must have a value.</summary>
		Required = 512,

		/// <summary>The target must contain the value in the string.</summary>
		Contains = 1024,

		/// <summary>The target must be one of the values in the parameter list.</summary>
		In = 2048,

		/// <summary>The target must be one of the values in the parameter list obtined from a DB table.</summary>
		InDB = 4096,

		/// <summary>No modifier selected.</summary>
		None = 8192
	}

	public enum ExpressionOperands {
		None = -1,
		Equals = 0,
		GreaterThan = 1,
		LessThan = 2,
		GreaterThanOrEqualTo = 3,
		LessThanOrEqualTo = 4,
		NotEqualTo = 5
	}

	public enum TargetTypeEnum {

		/// <summary>The test target value is numerically based.</summary>
		Numeric = 1,

		/// <summary>The test target value is text based.</summary>
		Text = 2,

		/// <summary>The test target value is date based.</summary>
		Date = 3
	}

	public static class Common {

		/// <summary>Gets the value system type for proper comparisons.</summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static TargetTypeEnum GetValueType(object value) {
			if (value is Int32 || value is int || value is long) { return TargetTypeEnum.Numeric; }
			if (value is String || value is Char) { return TargetTypeEnum.Text; }
			if (value is Decimal || value is float || value is double) { return TargetTypeEnum.Numeric; }
			return value is DateTime ? TargetTypeEnum.Date : TargetTypeEnum.Text;
		}

		/// <summary>Gets the value system as the expected type.</summary>
		/// <param name="value">       The value.</param>
		/// <param name="expectedType">The default value.</param>
		/// <returns></returns>
		public static object ConvertValueToItsExpectedType(object value, TargetTypeEnum expectedType) {
			if (expectedType == TargetTypeEnum.Numeric) {
				double doubleValue;
				if (double.TryParse(value.ToString(), out doubleValue))
					return doubleValue;
			}

			if (expectedType == TargetTypeEnum.Date) {
				DateTime dateValue;
				if (DateTime.TryParse(value.ToString(), out dateValue))
					return dateValue;
			}
			return value.ToString();
		}

		/// <summary>Gets or sets the path to business logic plugins.</summary>
		/// <value>The path to business logic plugins.</value>
		public static string PathToBusinessLogicPlugins { get; set; }

		/// <summary>Gets or sets a value indicating whether the validation will run in via an automated process. This will disable the SmartUpload Access checks.</summary>
		/// <value><c>true</c> if [run automated]; otherwise, <c>false</c>.</value>
		public static bool RunAutomated { get; set; }

		/// <summary>Determins if the passed row marks the end of the table.</summary>
		/// <param name="row">        The row.</param>
		/// <param name="columnCount">The column count.</param>
		/// <returns></returns>
		public static bool EndOfTable(DataRow row) {
			if (row == null) {
				return true;
			} else {
				foreach (object value in row.ItemArray) {
					if (value.ToString().Length > 0) {
						return false;
					}
				}
				return true;
			}
		}

		/// <summary>Determines whether this instance can access the specified path.</summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static bool CanAccessFileOrFolder(string path, string fileName = null) {
			try {
				if (fileName != null) {
					if (fileName.Contains("*")) {
						File.OpenRead(Directory.EnumerateFiles(path, fileName).First()).Close();
					} else {
						File.OpenRead(String.Format("{0}\\{1}", path, Path.GetFileName(fileName))).Close();
					}
				} else {
					System.Security.AccessControl.DirectorySecurity ds = System.IO.FileSystemAclExtensions.GetAccessControl(new DirectoryInfo(path));
				}
				return true;
			} catch {
				return false;
			}
		}
	}
}