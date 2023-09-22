using System.Text.RegularExpressions;

namespace CodedThought.Core.Business {

	/// <summary>Address class.</summary>
	[Serializable]
	public class Address : ObjectEntityBase {

		#region Data

		private AddressType type;
		private string street;
		private string street2;
		private string street3;
		private string street4;
		private string street5;
		private string street6;
		private string city;
		private string state;
		private string postalcode;
		private string country;
		private Boolean primary;
		private Boolean visible;

		#endregion Data

		#region Properties

		/// <summary>Gets or sets the address's type.</summary>
		public AddressType AddressType {
			get { return this.type; }
			set { this.type = value; }
		}

		/// <summary>Gets or sets the Address's street (line 1).</summary>
		public string Street {
			get { return this.street; }
			set { this.street = value; }
		}

		/// <summary>Gets or sets the Address's street (line 2).</summary>
		public string Street2 {
			get { return this.street2; }
			set { this.street2 = value; }
		}

		/// <summary>Gets or sets the Address's street (line 3).</summary>
		public string Street3 {
			get { return this.street3; }
			set { this.street3 = value; }
		}

		/// <summary>Gets or sets the Address's street (line 4).</summary>
		public string Street4 {
			get { return this.street4; }
			set { this.street4 = value; }
		}

		/// <summary>Gets or sets the Address's street (line 5).</summary>
		public string Street5 {
			get { return this.street5; }
			set { this.street5 = value; }
		}

		/// <summary>Gets or sets the Address's street (line 6).</summary>
		public string Street6 {
			get { return this.street6; }
			set { this.street6 = value; }
		}

		/// <summary>Gets or sets the Address's City.</summary>
		public string City {
			get { return this.city; }
			set { this.city = value; }
		}

		/// <summary>Gets or sets the Addresse's State.</summary>
		public string State {
			get { return this.state; }
			set { this.state = value; }
		}

		/// <summary>Gets or sets the Address's Postal Code.</summary>
		public string PostalCode {
			get { return this.postalcode; }
			set { this.postalcode = value; }
		}

		/// <summary>Gets or sets the address's country.</summary>
		public string Country {
			get {
				return this.country;
			}
			set {
				this.country = value;
			}
		}

		/// <summary>Gets or sets if this is a primary address.</summary>
		public Boolean PrimaryAddress {
			get {
				return this.primary;
			}
			set {
				this.primary = value;
			}
		}

		/// <summary>Gets or sets if this address is visible to the public or not.</summary>
		public Boolean VisibleAddress {
			get {
				return this.visible;
			}
			set {
				this.visible = value;
			}
		}

		/// <summary>Gets the full address concatentated together with carriage returns and line feeds.</summary>
		public string FullAddress {
			get {
				System.Text.StringBuilder strAddress = new();
				if (this.street.Length > 0) {
					// Ouput the attendee's address
					strAddress.Append(this.street + "\r\n");
					if (this.street2.Length > 0) { strAddress.Append(this.street2 + "\r\n"); }
					if (this.street3.Length > 0) { strAddress.Append(this.street3 + "\r\n"); }
					if (this.street4.Length > 0) { strAddress.Append(this.street4 + "\r\n"); }
					if (this.street5.Length > 0) { strAddress.Append(this.street5 + "\r\n"); }
					if (this.street6.Length > 0) { strAddress.Append(this.street6 + "\r\n"); }
					strAddress.Append(this.city + ", ");
					strAddress.Append(this.state + "  ");
					strAddress.Append(this.postalcode);
				}
				return strAddress.ToString();
			}
		}

		/// <summary>Gets the full address concatentated together with carriage returns and line feeds.</summary>
		public string FullAddressHTML {
			get {
				System.Text.StringBuilder strAddress = new();
				if (this.street.Length > 0) {
					// Ouput the attendee's address
					strAddress.Append(this.street + "<br>");
					if (this.street2.Length > 0) { strAddress.Append(this.street2 + "<br>"); }
					if (this.street3.Length > 0) { strAddress.Append(this.street3 + "<br>"); }
					if (this.street4.Length > 0) { strAddress.Append(this.street4 + "<br>"); }
					if (this.street5.Length > 0) { strAddress.Append(this.street5 + "<br>"); }
					if (this.street6.Length > 0) { strAddress.Append(this.street6 + "<br>"); }
					strAddress.Append(this.city + ", ");
					strAddress.Append(this.state + "  ");
					strAddress.Append(this.postalcode);
				}
				return strAddress.ToString();
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>Parses an address string into the proper parts</summary>
		/// <param name="addressText"></param>
		public void ParseAddress(string addressText) {
			if (addressText.Length == 0) { return; }

			string[] aryAddress;
			string[] ary3rdLine;
			string[] aryStatePostalCode;

			addressText = addressText.Replace("\r\n", "|");
			//Split the string by the Carriage Return
			char[] sep = { '|' };
			aryAddress = addressText.Split(sep);
			for (int x = 0; x <= aryAddress.Length - 1; x++) {
				if (x != aryAddress.Length - 1) {
					//Get the street lines
					switch (x) {
						case 0:
							this.street = aryAddress[x].ToString().Trim();
							break;

						case 1:
							this.street2 = aryAddress[x].ToString().Trim();
							break;

						case 2:
							this.street3 = aryAddress[x].ToString().Trim();
							break;

						case 3:
							this.street4 = aryAddress[x].ToString().Trim();
							break;

						case 4:
							this.street5 = aryAddress[x].ToString().Trim();
							break;

						case 5:
							this.street6 = aryAddress[x].ToString().Trim();
							break;

						default:
							this.street = aryAddress[x].ToString().Trim();
							break;
					}
				} else {
					//Get the City
					char[] sep1 = { ',' };
					ary3rdLine = aryAddress[x].Split(sep1);
					this.city = ary3rdLine[0].ToString().Trim();
					//Get the State
					char[] sep2 = { ' ', ' ' };
					Regex r = new("  ");
					aryStatePostalCode = r.Split(ary3rdLine[1]);
					this.state = aryStatePostalCode[0].ToString().Trim();
					//Get the postal code
					this.postalcode = aryStatePostalCode[1].ToString().Trim();
				}
			}
		}

		#endregion Methods

		#region Constructor

		public Address()
			: base() {
			this.street = String.Empty;
			this.street2 = String.Empty;
			this.street3 = String.Empty;
			this.street4 = String.Empty;
			this.street5 = String.Empty;
			this.street6 = String.Empty;
			this.city = String.Empty;
			this.state = String.Empty;
			this.postalcode = String.Empty;
			this.primary = false;
		}

		#endregion Constructor
	}
}