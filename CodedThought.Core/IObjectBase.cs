using System.Xml;
using System.Xml.Serialization;

namespace CodedThought.Core {

	public interface IObjectBase {

		/// <summary>Gets or sets the ID.</summary>
		[XmlAttribute()]
		Int32 ID { get; set; }
	}
}