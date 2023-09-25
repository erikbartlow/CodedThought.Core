using System.Net;

namespace CodedThought.Core.Security {

	/// <summary>
	/// Summary description for Authentication
	/// Created:  8/5/2010 1:54:00 PM Created By: bartlowe
	/// </summary>
	public class Authentication {

		#region Declarations

		private String _url;
		private String _userName;
		private String _password;
		private String _domain;

		#endregion Declarations

		#region Properties

		public String Url {
			get { return _url; }
			set { _url = value; }
		}

		/// <summary>Gets or sets the name of the user.</summary>
		/// <value>The name of the user.</value>
		public String UserName {
			get { return _userName; }
			set { _userName = value; }
		}

		/// <summary>Gets or sets the password.</summary>
		/// <value>The password.</value>
		public String Password {
			get { return _password; }
			set { _password = value; }
		}

		/// <summary>Gets or sets the domain.</summary>
		/// <value>The domain.</value>
		public String Domain {
			get { return _domain; }
			set { _domain = value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>Gets the current user default credentials.</summary>
		/// <returns></returns>
		public NetworkCredential GetCurrentUserCredentials() {
			if (_url != String.Empty) {
				return System.Net.CredentialCache.DefaultCredentials.GetCredential(new Uri(_url), "");
			} else {
				throw new Exceptions.CodedThoughtApplicationException("The Url parameter is missing.");
			}
		}

		/// <summary>Gets the current user credentials.</summary>
		/// <param name="url">The URL.</param>
		/// <returns></returns>
		public NetworkCredential GetCurrentUserCredentials(String url) {
			_url = url;
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
		public NetworkCredential GetCurrentUserCredentials(String userName, String password, String domain) {
			return new NetworkCredential(userName, password, domain);
		}

		#endregion Methods

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="Authentication" /> class.</summary>
		public Authentication() {
		}

		#endregion Constructors
	}
}