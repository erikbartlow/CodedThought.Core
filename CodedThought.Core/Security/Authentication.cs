using System.Net;

namespace CodedThought.Core.Security {

	/// <summary>
	/// Summary description for Authentication
	/// Created:  8/5/2010 1:54:00 PM Created By: bartlowe
	/// </summary>
	public class Authentication {

		#region Declarations


		#endregion Declarations

		#region Properties

		public String Url { get; set; }

		/// <summary>Gets or sets the name of the user.</summary>
		/// <value>The name of the user.</value>
		public String UserName { get; set; }

		/// <summary>Gets or sets the password.</summary>
		/// <value>The password.</value>
		public String Password { get; set; }

		/// <summary>Gets or sets the domain.</summary>
		/// <value>The domain.</value>
		public String Domain { get; set; }

		#endregion Properties

		#region Methods

		/// <summary>Gets the current user default credentials.</summary>
		/// <returns></returns>
		public NetworkCredential GetCurrentUserCredentials() {
			if (Url != String.Empty) {
				return System.Net.CredentialCache.DefaultCredentials.GetCredential(new Uri(Url), "");
			} else {
				throw new Exceptions.CodedThoughtApplicationException("The Url parameter is missing.");
			}
		}

		/// <summary>Gets the current user credentials.</summary>
		/// <param name="url">The URL.</param>
		/// <returns></returns>
		public NetworkCredential GetCurrentUserCredentials(String url) {
			Url = url;
			return GetCurrentUserCredentials();
		}

		/// <summary>Gets the current user credentials.</summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public NetworkCredential GetCurrentUserCredentials(String userName, String password) {
			if (userName.IndexOf("\\") > 0) {
				return new NetworkCredential(userName, password);
			} else {
				throw new Exceptions.CodedThoughtApplicationException("Your username does not contain the domain in the [domain\\username] format.");
			}
		}

		/// <summary>Gets the current user credentials.</summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		/// <param name="domain">  The domain.</param>
		/// <returns></returns>
		public NetworkCredential GetCurrentUserCredentials(String userName, String password, String domain) => new NetworkCredential(userName, password, domain);

		#endregion Methods

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Authentication" /> class.</summary>
		public Authentication() {
		}

		#endregion Constructors
	}
}