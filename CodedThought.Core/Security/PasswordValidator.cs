namespace CodedThought.Core.Security {

	/// <summary>Summary description for PasswordValidator.</summary>
	public class PasswordValidator {

		#region Data

		/// <summary>Local string variable to hold the returning validation message</summary>
		protected string _invalidMessage = string.Empty;

		/// <summary>Local integer variable to hold the minimum password length</summary>
		protected int _minLength = 8;

		/// <summary>Local integer variable to hold the maximum password length</summary>
		protected int _maxLength = 64;

		/// <summary>Local string variable to hold the regex pattern to match</summary>
		protected string _pattern = @"(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[\-\+\?\&\*\$\^\@])";

		#endregion Data

		#region Properties

		/// <summary>Gets or sets the password validators invalid message text.</summary>
		public string InvalidReasonMessage {
			get { return this._invalidMessage; }
			set { this._invalidMessage = value; }
		}

		/// <summary>Gets or sets the validator pattern used by regular expressions</summary>
		public string PasswordPattern {
			get { return this._pattern; }
			set { this._pattern = value; }
		}

		/// <summary>Gets or sets the passwords minimum length</summary>
		public int MinimumPasswordLength {
			get { return this._minLength; }
			set { this._minLength = value; }
		}

		/// <summary>Gets or sets the passwords maximum length</summary>
		public int MaximumPasswordLength {
			get { return this._maxLength; }
			set { this._maxLength = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Check whether the provided string is a strong password. The string must contain at least one uppercase, one lowercase, one numeric, and one special character. The method allows uppercase,
		/// lowercase, digits, and keyboard characters"
		/// </summary>
		/// <param name="password">The password to validate.</param>
		/// <returns>True if the password is a strong password, false otherwise.</returns>
		public Boolean IsStrongPassword(String password) {
			// Default to a true password.
			Boolean returnValue = true;

			// Defines minimum appearance of characters
			// At least one digit between 1 and 9
			// At least one lowercase character
			// At least one uppercase character
			// At least one special character in the defined list
			//		- or + or ? or * or $ or ^ or @
			//String ex1 = @"(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[\-\+\?\&\*\$\^\@])";

			if (!IsMatch(password, this.PasswordPattern, RegexOptions.IgnorePatternWhitespace)) {
				this.InvalidReasonMessage = "Your password must contain at least one of the following characters, lowercase, uppercase, and special character.  Please retry your password.";
				returnValue = false;
			}
			if (password.Length < this.MinimumPasswordLength || password.Length > this.MaximumPasswordLength) {
				this.InvalidReasonMessage = "Your password must be between " + this.MinimumPasswordLength + " and " + this.MaximumPasswordLength + " characters long.  Please retry your password.";
				returnValue = false;
			}

			return returnValue;
		}

		/// <summary>Match a regular expression against a provided string.</summary>
		/// <param name="input">  The input string to validate.</param>
		/// <param name="pattern">The regular expression pattern used to validate the input.</param>
		/// <param name="options">A bitwise OR combination of the RegExOption enumeration values</param>
		/// <returns>True if the parameters produce a match, false otherwise.</returns>
		public Boolean IsMatch(String input, String pattern, RegexOptions options) {
			Regex regex = new(pattern, options);
			Match m = regex.Match(input);
			return m.Success;
		}

		#endregion Methods

		#region Constructors

		/// <summary>PasswordValidator constructor</summary>
		public PasswordValidator() {
			// TODO: Add constructor logic here
		}

		#endregion Constructors
	}
}