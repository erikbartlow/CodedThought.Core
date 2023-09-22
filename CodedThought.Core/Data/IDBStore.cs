namespace CodedThought.Core.Data {

	/// <summary></summary>
	public interface IDBStore {

		/// <summary>Extracts the specified business entity.</summary>
		/// <param name="businessEntity">The business entity.</param>
		/// <param name="columnName">    Name of the column.</param>
		/// <returns></returns>
		object Extract(object businessEntity, string columnName);

		/// <summary>Gets the primary key.</summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		int GetPrimaryKey(object obj);

		/// <summary>Gets the name of the primary key.</summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		string GetPrimaryKeyName(object obj);

		/// <summary>Sets the primary key.</summary>
		/// <param name="obj">  The obj.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		bool SetPrimaryKey(object obj, int value);

		/// <summary>Determines whether [has key column] [the specified obj].</summary>
		/// <param name="obj">The obj.</param>
		/// <returns><c>true</c> if [has key column] [the specified obj]; otherwise, <c>false</c>.</returns>
		bool HasKeyColumn(object obj);
	}
}