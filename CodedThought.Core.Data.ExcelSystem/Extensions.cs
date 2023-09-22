using CodedThought.Core.Data.ExcelSystem;
using System.Data;
using System.Reflection;

namespace CodedThought.Core.Extensions {

	public static class Extensions {

		public static ExcelColumnAttribute GetExcelColumnAttributes(this System.Reflection.PropertyInfo prop, bool inherit = false) {
			return prop.GetCustomAttributes(inherit).OfType<ExcelColumnAttribute>().Distinct().FirstOrDefault();
		}

		/// <summary>Creates a list of items based on the passed generic type.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">     </param>
		/// <param name="hasHeaders"></param>
		/// <remarks>If hasHeaders is set to false this will allow the method to use ExcelColumn attribute column orders instead of column names.</remarks>
		/// <returns></returns>
		public static IList<T> ToListFromExcel<T>(this DataTable table, bool hasHeaders = true) where T : new() {
			IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
			IList<T> result = new List<T>();

			foreach (object row in table.Rows) {
				var item = CreateItemFromExcelRow<T>((DataRow)row, properties, hasHeaders);
				result.Add(item);
			}

			return result;
		}

		/// <summary>Creates the item from excel row.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="row">       The row.</param>
		/// <param name="properties">The properties.</param>
		/// <returns></returns>
		private static T CreateItemFromExcelRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new() {
			try {
				T item = new();
				foreach (var property in properties) {
					ExcelColumnAttribute? excelColumn = (ExcelColumnAttribute)Attribute.GetCustomAttribute(property, typeof(ExcelColumnAttribute));
					CodedThought.Core.Data.DataColumnAttribute? dataColumn = (CodedThought.Core.Data.DataColumnAttribute)Attribute.GetCustomAttribute(property, typeof(CodedThought.Core.Data.DataColumnAttribute));
					if (excelColumn != null && dataColumn != null) {
						// If both column attributes are available we have a match.
						switch (dataColumn.ColumnType) {
							case DbType.Int32:
							case DbType.Int64:
								property.SetValue(item, int.Parse(row[excelColumn.ColumnOrder].ToString()), null);
								break;
							case DbType.Boolean:
								property.SetValue(item, bool.Parse(row[excelColumn.ColumnOrder].ToString()), null); 
								break;
							case DbType.Decimal:
								property.SetValue(item, Decimal.Parse(row[excelColumn.ColumnOrder].ToString()), null);
								break;
							case DbType.Double:
								property.SetValue(item, Double.Parse(row[excelColumn.ColumnOrder].ToString()), null);
								break;

							case DbType.Date:
							case DbType.DateTime:
								property.SetValue(item, DateTime.Parse(row[excelColumn.ColumnOrder].ToString()), null);
								break;

							default:
								property.SetValue(item, row[excelColumn.ColumnOrder].ToString(), null);
								break;
						}
					}
				}
				return item;
			} catch (Exception ex) {
				throw new Exception($"Error in CreateItemFromExcelRow.  The row in error is {string.Join("|", row.ItemArray)}.", ex);
			}
		}

		/// <summary>Creates the item from excel row using column order.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="row">       The row.</param>
		/// <param name="properties">The properties.</param>
		/// <returns></returns>
		private static T CreateItemFromExcelRow<T>(DataRow row, IList<PropertyInfo> properties, bool hasHeaders = false) where T : new() {
			return CreateItemFromExcelRow<T>(row, properties);
		}
	}
}