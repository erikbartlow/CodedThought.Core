using System.DirectoryServices.AccountManagement;

namespace CodedThought.Core.Security {

	[Obsolete("Please use these components from the CodedThought.Core.Security.EnterpriseDirectory library.")]
	public interface ILDAPMember {
		UserPrincipal DirectoryUser { get; set; }

		string FullName { get; set; }

		string Email { get; set; }

		string PortraitUrl { get; set; }

		string PortraitUrlThumbnail { get; set; }

		List<LDAPExtraProperty> ExtraProperties { get; set; }
	}
}