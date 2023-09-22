using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;

namespace CodedThought.Core.Security {

	[Obsolete("Please use these components from the CodedThought.Core.Security.EnterpriseDirectory library.")]
	public enum LDAPAuthenticationType {
		Anonymous,
		Authenticated
	}

	[Obsolete("Please use these components from the CodedThought.Core.Security.EnterpriseDirectory library.")]
	public class LDAPMembershipServices {
		private string userName { get; set; }

		/// <summary>Gets or sets the LDAP application account.</summary>
		/// <value>The LDAP application account.</value>
		public string LDAPAppAccount { get; set; }

		/// <summary>Gets or sets the LDAP application password.</summary>
		/// <value>The LDAP application password.</value>
		public string LDAPAppPassword { get; set; }

		/// <summary>Gets or sets the authentication mode.</summary>
		/// <value>The authentication mode.</value>
		public LDAPAuthenticationType AuthenticationMode { get; set; }

		/// <summary>Gets or sets the active directory LDAP host.</summary>
		/// <value>The active directory dc host.</value>
		public string ActiveDirectoryDCHost { get; set; }

		/// <summary>Gets the currently logged in user.</summary>
		/// <param name="account">        The account.</param>
		/// <param name="extraProperties">The extra properties.</param>
		/// <returns></returns>
		/// <exception cref="MissingMemberException">The system was unable to determine the current user.</exception>
		/// <exception cref="COMException">Unable to reach LDAP server.</exception>
		public LDAPMember GetCurrentUser(string account, List<string> extraProperties = null) {
			try {
				List<LDAPExtraProperty> returnProperties = new();

				userName = account;
				DirectorySearcher search = new(GetCurrentDirectoryEntry());
				// Check if the email address was passed instead of the NT Account.
				search.Filter = userName.Contains("@") ? "(uid=" + userName + ")" : "(ntUserDomainId=" + FixUsername(userName) + ")";
				search.PropertiesToLoad.Add("uid");
				search.PropertiesToLoad.Add("cn");
				search.PropertiesToLoad.Add("hpLegalName");
				search.PropertiesToLoad.Add("hpPictureOneHpURI");
				search.PropertiesToLoad.Add("hpPictureThumbnailURI");
				if (extraProperties != null) {
					foreach (string prop in extraProperties) {
						search.PropertiesToLoad.Add(prop);
					}
				}
				SearchResult result = search.FindOne();
				if (result == null) throw new MissingMemberException("The system was unable to determine the current user.");

				if (result.Path != null || result.Path != string.Empty) {
					string email = (string)result.Properties["uid"][0];

					string fullname = ((result.Properties.Contains("hpLegalName"))) ? (string)result.Properties["hpLegalName"][0] : String.Empty;
					string cn = (result.Properties.Contains("cn")) ? (string)result.Properties["cn"][0] : String.Empty;

					string portraitLarge = (result.Properties.Contains("hpPictureOneHpURI")) ? (string)result.Properties["hpPictureOneHpURI"][0] : String.Empty;
					string portraitThumbnail = (result.Properties.Contains("hpPictureThumbnailURI")) ? (string)result.Properties["hpPictureThumbnailURI"][0] : String.Empty;
					LDAPMember user = new();
					user.FullName = fullname;
					user.Email = email;
					user.CommonName = cn;
					user.PortraitUrl = portraitLarge;
					user.PortraitUrlThumbnail = portraitThumbnail;
					// Set any extra properties in the user object.
					if (extraProperties != null) {
						foreach (string prop in extraProperties) {
							user.ExtraProperties.Add(new LDAPExtraProperty(prop, (string)result.Properties[prop][0]));
						}
					}

					return user;
				} else {
					throw new MissingMemberException("The system was unable to determine the current user.");
				}
			} catch (COMException comEx) {
				throw new COMException("Unable to reach LDAP server.", comEx);
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Gets the user.</summary>
		/// <param name="email">The email.</param>
		/// <returns></returns>
		/// <exception cref="MissingMemberException">The system was unable to determine the current user.</exception>
		public UserPrincipal GetUserByEmail(string email) {
			try {
				PrincipalContext ctx = new(ContextType.Domain);
				UserPrincipal user = UserPrincipal.FindByIdentity(ctx, IdentityType.Guid, String.Format("uid={0}", email));
				return user == null ? throw new MissingMemberException("The system was unable to determine the current user.") : user;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Determines whether the user is a group member of the specified group.</summary>
		/// <param name="user">     The user.</param>
		/// <param name="groupName">Name of the group.</param>
		/// <returns><c>true</c> if [is group member] [the specified user]; otherwise, <c>false</c>.</returns>
		protected bool IsGroupMember(UserPrincipal user, String groupName) {
			List<UserPrincipal> groupMembers = GetGroupMembers(groupName);
			return groupMembers.FindAll(u => u.Name == user.Name).Count > 0;
		}

		/// <summary>Determines whether [is group member] [the specified group name].</summary>
		/// <param name="groupName">Name of the group.</param>
		/// <returns><c>true</c> if [is group member] [the specified group name]; otherwise, <c>false</c>.</returns>
		protected bool IsGroupMember(string groupName, string email) {
			return GetADGroupUsers(groupName).Contains(email);
		}

		/// <summary>Gets the AD user groups.</summary>
		/// <param name="groupName">Name of the group.</param>
		/// <returns></returns>
		protected List<String> GetADGroupUsers(string groupName) {
			DirectorySearcher search = new(GetCurrentGroupDirectoryEntry());
			search.Filter = String.Format("(cn={0})", groupName);
			//search.PropertiesToLoad.Add( "uid" );
			search.PropertiesToLoad.Add("member");
			List<string> groupList = new();

			SearchResultCollection results = search.FindAll();
			foreach (SearchResult result in results) {
				ResultPropertyCollection resultPropsColl = result.Properties;
				foreach (Object prop in resultPropsColl["member"]) {
					groupList.Add(prop.ToString().Split(",".ToCharArray())[0].Split("=".ToCharArray())[1]);
				}
			}

			return groupList;
		}

		/// <summary>Gets the valid and active group members.</summary>
		/// <param name="groupName">Name of the group.</param>
		/// <returns></returns>
		/// <exception cref="NoMatchingPrincipalException"></exception>
		public List<UserPrincipal> GetGroupMembers(string groupName) {
			List<UserPrincipal> groupMembers = new();
			try {
				PrincipalContext ctx = new(ContextType.Domain);

				// Get the group.
				GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, groupName);

				if (group == null) {
					throw new NoMatchingPrincipalException(String.Format("The group, {0}, was not found.", groupName));
				} else {
					foreach (Principal p in group.GetMembers()) {
						UserPrincipal member = p as UserPrincipal;
						if (member != null) {
							if (!member.IsAccountLockedOut()) {
								groupMembers.Add(member);
							}
						}
					}
				}
				return groupMembers;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Fixes the username.</summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns></returns>
		private string FixUsername(string userName) {
			return userName.Replace("\\", ":");
		}

		/// <summary>Gets the current directory entry.</summary>
		/// <returns></returns>
		private DirectoryEntry GetCurrentDirectoryEntry() {
			DirectoryEntry dirEntry = null;
			CodedThought.Core.Switch.On(AuthenticationMode)
				.Case(LDAPAuthenticationType.Authenticated, () => { dirEntry = new DirectoryEntry($"LDAP://{ActiveDirectoryDCHost}/ou=People,o=CodedThought.com", LDAPAppAccount, LDAPAppPassword, AuthenticationTypes.Encryption); })
				.Default(() => { dirEntry = new DirectoryEntry($"LDAP://{ActiveDirectoryDCHost}/ou=People,o=CodedThought.com", null, null, AuthenticationTypes.Anonymous); });

			return dirEntry;
		}

		private DirectoryEntry GetCurrentGroupDirectoryEntry() {
			DirectoryEntry dirEntry = null;
			CodedThought.Core.Switch.On(AuthenticationMode)
				.Case(LDAPAuthenticationType.Authenticated, () => { dirEntry = new DirectoryEntry($"LDAP://{ActiveDirectoryDCHost}/ou=Groups,o=CodedThought.com", LDAPAppAccount, LDAPAppPassword, AuthenticationTypes.Encryption); })
				.Default(() => { dirEntry = new DirectoryEntry($"LDAP://{ActiveDirectoryDCHost}/ou=Groups,o=CodedThought.com", null, null, AuthenticationTypes.Anonymous); });

			return dirEntry;
		}

		/// <summary>Initializes a new instance of the <see cref="LDAPMembershipServices" /> class.</summary>
		public LDAPMembershipServices() {
			AuthenticationMode = LDAPAuthenticationType.Anonymous;
		}

		/// <summary>Initializes a new instance of the <see cref="LDAPMembershipServices" /> class.</summary>
		/// <param name="user">The user.</param>
		public LDAPMembershipServices(string ldapHost, string user) : this() {
			ActiveDirectoryDCHost = ldapHost;
			userName = FixUsername(user);
		}
	}
}