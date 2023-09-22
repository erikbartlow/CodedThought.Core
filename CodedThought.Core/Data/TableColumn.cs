namespace CodedThought.Core.Data {

	/// <summary>Stores information on the structire of the data storage represented by the store.</summary>
	public class TableColumn {

		/// <summary>name of table</summary>
		public string name;

		/// <summary>database data type</summary>
		public DbTypeSupported type;

		/// <summary>system data type</summary>
		public Type systemType;

		/// <summary>column size</summary>
		public int size;

		/// <summary>IsUpdatable flag</summary>
		public Boolean isUpdateable;

		/// <summary>use this column fo rsorting data</summary>
		public Boolean isSortColumn;

		/// <summary>This column is nullable or not.</summary>
		public Boolean isNullable;

		/// <summary>This column is an identity column.</summary>
		public Boolean isIdentity;

		/// <summary>The maximum length of the values in this column.</summary>
		public Int32 maxLength;

		/// <summary>Is the sorting order descending? Default is ascending.</summary>
		public Boolean isDescending = false;

		/// <summary>To be used when you do not insert data for this field, for example if this was a Sequence or an Identity field</summary>
		public Boolean isInsertable;

		/// <summary>The ordinal position</summary>
		public Int32 ordinalPosition;

		/// <summary>The corresponding property name</summary>
		public String correspondingPropertyName;

		/// <summary>Constructor</summary>
		/// <param name="name">        </param>
		/// <param name="type">        </param>
		/// <param name="size">        </param>
		/// <param name="isUpdateable"></param>
		public TableColumn(string name, DbTypeSupported type, int size, Boolean isUpdateable) {
			this.name = name;
			this.type = type;
			this.size = size;
			this.isUpdateable = isUpdateable;
			this.isSortColumn = false;
			this.isInsertable = true;
			this.isNullable = true;
		}

		/// <summary>Constructor</summary>
		/// <param name="name">        </param>
		/// <param name="type">        </param>
		/// <param name="size">        </param>
		/// <param name="isUpdateable"></param>
		/// <param name="isSortColumn"></param>
		public TableColumn(string name, DbTypeSupported type, int size, Boolean isUpdateable, Boolean isSortColumn) {
			this.name = name;
			this.type = type;
			this.size = size;
			this.isUpdateable = isUpdateable;
			this.isSortColumn = isSortColumn;
			this.isInsertable = true;
			this.isNullable = true;
		}

		/// <summary>Constructor</summary>
		/// <param name="name">        </param>
		/// <param name="type">        </param>
		/// <param name="size">        </param>
		/// <param name="isUpdateable"></param>
		/// <param name="isSortColumn"></param>
		/// <param name="isDescending"></param>
		public TableColumn(string name, DbTypeSupported type, int size, Boolean isUpdateable, Boolean isSortColumn, Boolean isDescending) {
			this.name = name;
			this.type = type;
			this.size = size;
			this.isUpdateable = isUpdateable;
			this.isSortColumn = isSortColumn;
			this.isInsertable = true;
			this.isDescending = isDescending;
		}
	}
}