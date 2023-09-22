using System.DirectoryServices.AccountManagement;

namespace CodedThought.Core.Security {

	[Obsolete("Please use these components from the CodedThought.Core.Security.EnterpriseDirectory library.")]
	public class LDAPMember : ILDAPMember {

		public LDAPMember() {
			ExtraProperties = new List<LDAPExtraProperty>();
		}

		public UserPrincipal DirectoryUser { get; set; }

		public string FullName { get; set; }

		public string CommonName { get; set; }

		public string Email { get; set; }

		public string PortraitUrl { get; set; }

		public string PortraitUrlThumbnail { get; set; }

		public List<LDAPExtraProperty> ExtraProperties { get; set; }
	}
}