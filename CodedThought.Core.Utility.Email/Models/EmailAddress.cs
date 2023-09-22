namespace CodedThought.Core.Utility.Email {

	/// <summary>Email address Class in the CodedThought.Core.Business Library</summary>
	public class EmailAddress {

		#region Declarations

		/// <summary>Email address domain enumerator.</summary>
		public enum EmailAddressDomains {

			///<summary>
			///	Must verify eligibility for registration; only those in various categories of air-travel-related entities may register.
			///</summary>
			AERO = 0,

			///<summary>
			///	This is a TLD for companies, organizations, and individuals based in the region of Asia, Australia, and the Pacific.
			///</summary>
			ASIA = 1,

			///<summary>
			///	This is an open TLD; any person or entity is permitted to register; however, registrations may be challenged later if they are not by commercial entities in accordance with the domain's charter.
			///</summary>
			BIZ = 2,

			///<summary>
			///	This is a TLD for websites in the Catalan language or related to Catalan culture.
			///</summary>
			CAT = 3,

			///<summary>
			///	This is an open TLD; any person or entity is permitted to register.
			///</summary>
			COM = 4,

			///<summary>
			///	The .coop TLD is limited to cooperatives as defined by the Rochdale Principles.
			///</summary>
			COOP = 5,

			///<summary>
			///	The .edu TLD is limited to institutions of learning (nearly all universally in the U.S. and increasingly overseas, e.g., Australia and China), such as 2 and 4-year colleges and universities.
			///</summary>
			EDU = 6,

			///<summary>
			///	The .gov TLD is limited to U.S. governmental entities and agencies (mostly but not exclusively federal).
			///</summary>
			GOV = 7,

			///<summary>
			///	This is an open TLD; any person or entity is permitted to register.
			///</summary>
			INFO = 8,

			///<summary>
			///	The .int TLD is strictly limited to organizations, offices, and programs which are endorsed by a treaty between two or more nations.
			///</summary>
			INT = 9,

			///<summary>
			///	The .jobs TLD is designed to be added after the names of established companies with jobs to advertise. At this time, owners of a "company.jobs" domain are not permitted to post jobs of third party employers.
			///</summary>
			JOBS = 10,

			///<summary>
			///	The .mil TLD is limited to use by the U.S. military.
			///</summary>
			MIL = 11,

			///<summary>
			///	Must be used for mobile-compatible sites in accordance with standards.
			///</summary>
			MOBI = 12,

			///<summary>
			///	Must be verified as a legitimate museum.
			///</summary>
			MUSEUM = 13,

			///<summary>
			///	This is an open TLD; any person or entity is permitted to register; however, registrations may be challenged later if they are not by individuals (or the owners of fictional characters) in accordance with the domain's charter.
			///</summary>
			NAME = 14,

			///<summary>
			///	This is an open TLD; any person or entity is permitted to register.
			///</summary>
			NET = 15,

			///<summary>
			///	This is an open TLD; any person or entity is permitted to register.
			///</summary>
			ORG = 16,

			///<summary>
			///	Currently, .pro is reserved for licensed or certified lawyers, accountants, physicians and engineers in France, Canada, UK and the U.S. A professional seeking to register a .pro domain must provide their registrar with the appropriate credentials.
			///</summary>
			PRO = 17,

			/// <summary></summary>
			TEL = 18,

			///<summary>
			///	Must be verified as a legitimate travel-related entity.
			///</summary>
			TRAVEL = 19,

			/// <summary></summary>
			NONE = 20
		}

		private String _name;
		private String _computerName;
		private EmailAddressDomains _domain;
		private String _address;

		#endregion Declarations

		#region Properties

		/// <summary>Gets or sets the name of the account.</summary>
		/// <value>The name of the account.</value>
		public String AccountName {
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>Gets or sets the name of the computer.</summary>
		/// <value>The name of the computer.</value>
		public String ComputerName {
			get { return _computerName; }
			set { _computerName = value; }
		}

		/// <summary>Gets or sets the domain.</summary>
		/// <value>The domain.</value>
		public EmailAddressDomains Domain {
			get { return _domain; }
			set { _domain = value; }
		}

		/// <summary>Gets or sets the address.</summary>
		/// <value>The address.</value>
		public String Address {
			get { return _address; }
			set { _address = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>Validates the specified address.</summary>
		/// <param name="address">The address.</param>
		/// <returns></returns>
		public Boolean Validate(String address) {
			return Validate(address, @"\w+([-+.])*@\w+([-+.])*\.\w+([-.]\w+)*");
		}

		/// <summary>Validates the specified address.</summary>
		/// <param name="address">The address.</param>
		/// <param name="pattern">The pattern.</param>
		/// <returns></returns>
		public Boolean Validate(String address, String pattern) {
			System.Text.RegularExpressions.Regex regex = new(pattern, System.Text.RegularExpressions.RegexOptions.None);
			return regex.IsMatch(address);
		}

		/// <summary>Validates this instance.</summary>
		/// <returns></returns>
		public Boolean Validate() {
			return Validate(_address, @"\w+([-+.])*@\w+([-+.])*\.\w+([-.]\w+)*");
		}

		/// <summary>Gets the mailto link.</summary>
		/// <returns></returns>
		public String GetMailToLink() {
			return _address != String.Empty ? String.Format("<a href=\"mailto:{0}\">{0}</a>", _address) : String.Empty;
		}

		/// <summary>Parses the specified address.</summary>
		/// <param name="address">The address.</param>
		/// <returns></returns>
		public System.Collections.Generic.List<String> ParseAddress(String address) {
			if (Validate(address)) {
				System.Collections.Generic.List<String> parts = new();
				String[] simpleParts = address.Split("@".ToCharArray());
				parts.Add(simpleParts[0]);
				parts.Add(simpleParts[1].Split(".".ToCharArray())[0]);
				parts.Add(simpleParts[1].Split(".".ToCharArray())[1]);
				return parts;
			} else {
				return new List<string>();
			}
		}

		/// <summary>Parses this instance.</summary>
		/// <returns></returns>
		public System.Collections.Generic.List<String> ParseAddress() {
			List<String> emailParts = ParseAddress(_address);
			if (emailParts.Count == 0)
				return new List<String>();
			_name = emailParts[0];
			_computerName = emailParts[1];
			try {
				_domain = (EmailAddressDomains)Enum.Parse(typeof(EmailAddressDomains), emailParts[2].ToUpper());
			} catch {
				// Unable to parse the domain so set it to NONE.
				_domain = EmailAddressDomains.NONE;
			}
			return emailParts;
		}

		#endregion Methods

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="EmailAddress" /> class.</summary>
		public EmailAddress() {
			_address = String.Empty;
			_name = String.Empty;
			_computerName = String.Empty;
			_domain = EmailAddressDomains.NONE;
		}

		/// <summary>Initializes a new instance of the <see cref="EmailAddress" /> class.</summary>
		/// <param name="address">The address.</param>
		public EmailAddress(string address) {
			_address = address;
			ParseAddress();
		}

		#endregion Constructors
	}
}