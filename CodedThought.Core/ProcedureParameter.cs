namespace CodedThought.Core.Data {

	/// <summary>Stores information onthe structure of the stored procedure referenced by the store.</summary>
	public class ProcedureParameter {
		public string Name { get; set; }

		public DbTypeSupported Type { get; set; }

		public string SourceColumn { get; set; }

		public string Value { get; set; }

		public int Size { get; set; }

		/// <summary>Initializes a new instance of the <see cref="ProcedureParameter" /> class.</summary>
		/// <param name="name"> The name.</param>
		/// <param name="type"> The type.</param>
		/// <param name="size"> The size.</param>
		/// <param name="value">The value.</param>
		public ProcedureParameter(string name, DbTypeSupported type, int size, string value) {
			SourceColumn = string.Empty;
			this.Name = name;
			this.Type = type;
			this.Size = size;
			this.Value = value;
		}

		/// <summary>Initializes a new instance of the <see cref="ProcedureParameter" /> class.</summary>
		/// <param name="name">        The name.</param>
		/// <param name="type">        The type.</param>
		/// <param name="size">        The size.</param>
		/// <param name="sourceColumn">The source column.</param>
		/// <param name="value">       The value.</param>
		public ProcedureParameter(string name, DbTypeSupported type, int size, string sourceColumn, string value) {
			this.Name = name;
			this.Type = type;
			this.Size = size;
			this.Value = value;
			this.SourceColumn = sourceColumn;
		}
	}
}