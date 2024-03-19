using System.Xml.Serialization;
using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core
{

    /// <summary>ObjectEntityBase provides the base for all object entities used in this project.</summary>
    [Serializable]
	public class ObjectEntityBase : IObjectBase {

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="ObjectEntityBase" /> class.</summary>
		public ObjectEntityBase() {
			ID = Int32.MinValue;
			Name = string.Empty;
			Description = string.Empty;
			CreatedDate = DateTime.MinValue;
		}

		/// <summary>Initializes a new instance of the <see cref="ObjectEntityBase" /> class.</summary>
		/// <param name="id">  The id.</param>
		/// <param name="name">The name.</param>
		public ObjectEntityBase(int id, string name) : this() {
			ID = id;
			Name = name;
		}

		/// <summary>Initializes a new instance of the <see cref="ObjectEntityBase" /> class.</summary>
		/// <param name="id">         The id.</param>
		/// <param name="name">       The name.</param>
		/// <param name="description">The description.</param>
		public ObjectEntityBase(int id, string name, string description) : this(id, name) => Description = description;

		/// <summary>Initializes a new instance of the <see cref="ObjectEntityBase" /> class.</summary>
		/// <param name="id">         </param>
		/// <param name="name">       </param>
		/// <param name="description"></param>
		/// <param name="createdDate"></param>
		public ObjectEntityBase(int id, string name, string description, DateTime createdDate) : this(id, name, description) => CreatedDate = createdDate;

		#endregion Constructors

		#region Properties

		/// <summary>Gets and sets the ID.</summary>
		[XmlAttribute()]
		public virtual int ID { get; set; }

		/// <summary>Gets or sets the name.</summary>
		public string Name { get; set; }

		/// <summary>Gets or sets the Description.</summary>
		public string Description { get; set; }

		/// <summary>Gets or sets the created date.</summary>
		/// <value>The created date.</value>
		public DateTime CreatedDate { get; set; }

		#endregion Properties
	}
}