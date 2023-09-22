namespace CodedThought.Core {

	/// <summary>Indicates if the target property should be indexed in a SortingList.</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class IndexAttribute : Attribute {
		private bool _index;

		/// <summary>Initializes a new instance of the <see cref="IndexAttribute" /> class.</summary>
		public IndexAttribute() {
			_index = true;
		}

		/// <summary>Initializes a new instance of the <see cref="IndexAttribute" /> class.</summary>
		/// <param name="value">if set to <c>true</c> this property will be indexed.</param>
		public IndexAttribute(bool value) {
			_index = value;
		}

		/// <summary>Gets or sets a value indicating whether this <see cref="IndexAttribute" /> is index.</summary>
		/// <value><c>true</c> if index; otherwise, <c>false</c>.</value>
		public bool Index {
			get { return _index; }
			set { _index = value; }
		}
	}
}