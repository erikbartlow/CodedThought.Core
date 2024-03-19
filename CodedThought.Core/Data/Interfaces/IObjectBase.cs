using System.Xml;
using System.Xml.Serialization;

namespace CodedThought.Core.Data.Interfaces
{

    public interface IObjectBase
    {

        /// <summary>Gets or sets the ID.</summary>
        [XmlAttribute()]
        int ID { get; set; }
    }
}