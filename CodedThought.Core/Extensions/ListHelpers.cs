using CodedThought.Core.Data;

namespace CodedThought.Core.Extensions {

	public static class ListHelpers {

		/// <summary>Converts the DataTable to a generic list.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		public static IList<T> ToList<T>(this DataTable table) where T : new() {
			IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
			IList<T> result = new List<T>();

			foreach (var row in table.Rows) {
				var item = Data.CreateItemFromRow<T>((DataRow)row, properties);
				result.Add(item);
			}

			return result;
		}
	}
}