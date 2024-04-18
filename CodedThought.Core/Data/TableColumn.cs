using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data
{

    /// <summary>Stores information on the structire of the data storage represented by the store.</summary>
    public class TableColumn : ITableColumn
    {

        /// <summary>name of table</summary>
        public string Name { get; set; }

        /// <summary>database data type</summary>
        public DbTypeSupported Type { get; set; }

        /// <summary>system data type</summary>
        public Type SystemType { get; set; }

        /// <summary>column size</summary>
        public int Size { get; set; }

        /// <summary>IsUpdatable flag</summary>
        public Boolean IsUpdateable { get; set; }

        /// <summary>use this column fo rsorting data</summary>
        public Boolean IsSortColumn { get; set; }

        /// <summary>This column is nullable or not.</summary>
        public Boolean IsNullable { get; set; }

        /// <summary>This column is an identity column.</summary>
        public Boolean IsIdentity { get; set; }

        /// <summary>The maximum length of the values in this column.</summary>
        public Int32 MaxLength { get; set; }

        /// <summary>Is the sorting order descending? Default is ascending.</summary>
        public Boolean IsDescending { get; set; } = false;

        /// <summary>To be used when you do not insert data for this field, for example if this was a Sequence or an Identity field</summary>
        public Boolean IsInsertable { get; set; }

        /// <summary>The ordinal position</summary>
        public Int32 OrdinalPosition { get; set; }

        /// <summary>The corresponding property name</summary>
        public String CorrespondingPropertyName { get; set; }

        /// <summary>Constructor</summary>
        /// <param name="name">        </param>
        /// <param name="type">        </param>
        /// <param name="size">        </param>
        /// <param name="isUpdateable"></param>
        public TableColumn(string name, DbTypeSupported type, int size, Boolean isUpdateable)
        {
            Name = name;
            Type = type;
            Size = size;
            IsUpdateable = isUpdateable;
            IsSortColumn = false;
            IsInsertable = true;
            IsNullable = true;
        }

        /// <summary>Constructor</summary>
        /// <param name="name">        </param>
        /// <param name="type">        </param>
        /// <param name="size">        </param>
        /// <param name="isUpdateable"></param>
        /// <param name="isSortColumn"></param>
        public TableColumn(string name, DbTypeSupported type, int size, Boolean isUpdateable, Boolean isSortColumn)
        {
            Name = name;
            Type = type;
            Size = size;
            IsUpdateable = isUpdateable;
            IsSortColumn = isSortColumn;
            IsInsertable = true;
            IsNullable = true;
            SystemType = null;
        }

        /// <summary>Constructor</summary>
        /// <param name="name">        </param>
        /// <param name="type">        </param>
        /// <param name="size">        </param>
        /// <param name="isUpdateable"></param>
        /// <param name="isSortColumn"></param>
        /// <param name="isDescending"></param>
        public TableColumn(string name, DbTypeSupported type, int size, Boolean isUpdateable, Boolean isSortColumn, Boolean isDescending)
        {
            Name = name;
            Type = type;
            Size = size;
            IsUpdateable = isUpdateable;
            IsSortColumn = isSortColumn;
            IsInsertable = true;
            IsDescending = isDescending;
        }
    }
}