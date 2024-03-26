using System.Text.RegularExpressions;
using System.Text;

namespace CodedThought.Core.Business {

	/// <summary>Address class.</summary>
	[Serializable]
	public class Address : ObjectEntityBase {

		#region Properties

		/// <summary>Gets or sets the address's type.</summary>
		public AddressType AddressType { get; set; }

		/// <summary>Gets or sets the Address's street (line 1).</summary>
		public string Street { get; set; }

		/// <summary>Gets or sets the Address's street (line 2).</summary>
		public string Street2 { get; set; }

		/// <summary>Gets or sets the Address's street (line 3).</summary>
		public string Street3 { get; set; }

		/// <summary>Gets or sets the Address's street (line 4).</summary>
		public string Street4 { get; set; }

		/// <summary>Gets or sets the Address's street (line 5).</summary>
		public string Street5 { get; set; }

		/// <summary>Gets or sets the Address's street (line 6).</summary>
		public string Street6 { get; set; }

		/// <summary>Gets or sets the Address's City.</summary>
		public string City { get; set; }

		/// <summary>Gets or sets the Addresse's State.</summary>
		public string State { get; set; }

		/// <summary>Gets or sets the Address's Postal Code.</summary>
		public string PostalCode { get; set; }

		/// <summary>Gets or sets the address's country.</summary>
		public string Country { get; set; }

		/// <summary>Gets or sets if this is a primary address.</summary>
		public Boolean PrimaryAddress { get; set; }

		/// <summary>Gets or sets if this address is visible to the public or not.</summary>
		public Boolean VisibleAddress { get; set; }

		/// <summary>Gets the full address concatentated together with carriage returns and line feeds.</summary>
		public string FullAddress {
			get {
				StringBuilder strAddress = new();
				if (Street.Length > 0) {
					// Ouput the attendee's address
					strAddress.Append(Street + "\r\n");
					if (Street2.Length > 0) { strAddress.Append(Street2 + "\r\n"); }
					if (Street3.Length > 0) { strAddress.Append(Street3 + "\r\n"); }
					if (Street4.Length > 0) { strAddress.Append(Street4 + "\r\n"); }
					if (Street5.Length > 0) { strAddress.Append(Street5 + "\r\n"); }
					if (Street6.Length > 0) { strAddress.Append(Street6 + "\r\n"); }
					strAddress.Append(City + ", ");
					strAddress.Append(State + "  ");
					strAddress.Append(PostalCode);
				}
				return strAddress.ToString();
			}
		}

		/// <summary>Gets the full address concatentated together with carriage returns and line feeds.</summary>
		public string FullAddressHTML {
			get {
				StringBuilder strAddress = new();
				if (Street.Length > 0) {
					// Ouput the attendee's address
					strAddress.Append(Street + "<br>");
					if (Street2.Length > 0) { strAddress.Append(Street2 + "<br>"); }
					if (Street3.Length > 0) { strAddress.Append(Street3 + "<br>"); }
					if (Street4.Length > 0) { strAddress.Append(Street4 + "<br>"); }
					if (Street5.Length > 0) { strAddress.Append(Street5 + "<br>"); }
					if (Street6.Length > 0) { strAddress.Append(Street6 + "<br>"); }
					strAddress.Append(City + ", ");
					strAddress.Append(State + "  ");
					strAddress.Append(PostalCode);
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
							Street = aryAddress[x].ToString().Trim();
							break;

						case 1:
							Street2 = aryAddress[x].ToString().Trim();
							break;

						case 2:
							Street3 = aryAddress[x].ToString().Trim();
							break;

						case 3:
							Street4 = aryAddress[x].ToString().Trim();
							break;

						case 4:
							Street5 = aryAddress[x].ToString().Trim();
							break;

						case 5:
							Street6 = aryAddress[x].ToString().Trim();
							break;

						default:
							Street = aryAddress[x].ToString().Trim();
							break;
					}
				} else {
					//Get the City
					char[] sep1 = { ',' };
					ary3rdLine = aryAddress[x].Split(sep1);
					City = ary3rdLine[0].ToString().Trim();
					//Get the State
					char[] sep2 = { ' ', ' ' };
					Regex r = new("  ");
					aryStatePostalCode = r.Split(ary3rdLine[1]);
					State = aryStatePostalCode[0].ToString().Trim();
					//Get the postal code
					PostalCode = aryStatePostalCode[1].ToString().Trim();
				}
			}
		}

		#endregion Methods

		#region Constructor

		public Address()
			: base() {
			Street = String.Empty;
			Street2 = String.Empty;
			Street3 = String.Empty;
			Street4 = String.Empty;
			Street5 = String.Empty;
			Street6 = String.Empty;
			City = String.Empty;
			State = String.Empty;
			PostalCode = String.Empty;
			Country = String.Empty;
			PrimaryAddress = false;
			AddressType = new();
		}

		#endregion Constructor
	}
}