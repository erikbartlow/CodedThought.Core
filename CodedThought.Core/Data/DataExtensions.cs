namespace CodedThought.Core.Data {

	public static class DataExtensions {

		#region Extensions

		/// <summary>To the list.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">    The table.</param>
		/// <param name="refColumn">The reference column.</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this DataTable table, DataColumn refColumn) => table.ToList<T>(refColumn.ColumnName);

		/// <summary>To the list.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">     The table.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this DataTable table, string columnName) {
			IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
			List<T> result = new();

			result = table.Rows.OfType<DataRow>()
				.Select(dr => dr.Field<T>(columnName)).ToList();

			return result;
		}

		/// <summary>Converts the DataTable to a generic list.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		public static IList<T> ToList<T>(this DataTable table) where T : new() {
			IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
			IList<T> result = new List<T>();

			foreach (object? row in table.Rows) {
				T? item = CreateItemFromRow<T>((DataRow)row, properties);
				result.Add(item);
			}

			return result;
		}

		/// <summary>Creates the item from row.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="row">       The row.</param>
		/// <param name="properties">The properties.</param>
		/// <returns></returns>
		private static T CreateItemFromRow<T>(this DataRow row, IList<PropertyInfo> properties) where T : new() {
			T item = new();
			foreach (PropertyInfo property in properties) {
				property.SetValue(item, row[property.Name], null);
			}
			return item;
		}

		/// <summary>Sets the property value.</summary>
		/// <param name="prop">         The property.</param>
		/// <param name="obj">          The object.</param>
		/// <param name="propertyValue">The property value.</param>
		public static void SetPropertyValue(this PropertyInfo prop, object obj, object propertyValue) {
			if (prop.CanWrite) {
				prop.SetValue(obj, Convert.ChangeType(propertyValue, prop.PropertyType), null);
			}
		}

		#endregion Extensions
	}
}