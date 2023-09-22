namespace CodedThought.Core.Data {

	/// <summary>Stores information onthe structure of the stored procedure referenced by the store.</summary>
	public class ProcedureParameter {
		private string _Name;
		private DbTypeSupported _type;
		private string _SourceColumn;
		private string _Value;
		private int _Size;

		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}

		public DbTypeSupported Type {
			get { return _type; }
			set { _type = value; }
		}

		public string SourceColumn {
			get { return _SourceColumn; }
			set { _SourceColumn = value; }
		}

		public string Value {
			get { return _Value; }
			set { _Value = value; }
		}

		public int Size {
			get { return _Size; }
			set { _Size = value; }
		}

		/// <summary>Initializes a new instance of the <see cref="ProcedureParameter" /> class.</summary>
		/// <param name="name"> The name.</param>
		/// <param name="type"> The type.</param>
		/// <param name="size"> The size.</param>
		/// <param name="value">The value.</param>
		public ProcedureParameter(string name, DbTypeSupported type, int size, string value) {
			this._Name = name;
			this._type = type;
			this._Size = size;
			this._Value = value;
		}

		/// <summary>Initializes a new instance of the <see cref="ProcedureParameter" /> class.</summary>
		/// <param name="name">        The name.</param>
		/// <param name="type">        The type.</param>
		/// <param name="size">        The size.</param>
		/// <param name="sourceColumn">The source column.</param>
		/// <param name="value">       The value.</param>
		public ProcedureParameter(string name, DbTypeSupported type, int size, string sourceColumn, string value) {
			this._Name = name;
			this._type = type;
			this._Size = size;
			this._Value = value;
			this._SourceColumn = sourceColumn;
		}
	}
}