using CodedThought.Core.Data;

namespace CodedThought.Core.Extensions {

	public static class Data {

		/// <summary>Returns whether a row exists in the table by the passed index.</summary>
		/// <param name="row">The row.</param>
		/// <returns></returns>
		public static bool EndOfTable(this DataRow row) {
			if (row == null) {
				return true;
			} else {
				foreach (object value in row.ItemArray) {
					if (value.ToString().Length > 0) {
						return false;
					}
				}
				return true;
			}
		}

		/// <summary>Returns whether a row exists in the table by the passed index.</summary>
		/// <param name="table">   The table.</param>
		/// <param name="rowIndex">Index of the row.</param>
		/// <returns></returns>
		public static bool EndOfTable(this DataTable table, int rowIndex, bool hasHeaderRow = true) {
			Int32 totalRows = hasHeaderRow ? table.Rows.Count - 1 : table.Rows.Count;
			return totalRows <= 0 || !(rowIndex < totalRows);
		}

		public static DataTableAttribute GetDataTableAttribute(this Type obj, bool inherit = false) => obj.GetCustomAttributes(inherit).OfType<DataTableAttribute>().Distinct().FirstOrDefault();

		public static ApiDataControllerAttribute GetApiDataControllerAttribute(this Type obj, bool inherit = false) => obj.GetCustomAttributes(inherit).OfType<ApiDataControllerAttribute>().Distinct().FirstOrDefault();

		public static DataTableUsageAttribute GetDataTableUsageAttribute(this Type obj, bool inherit = false) => obj.GetCustomAttributes(inherit).OfType<DataTableUsageAttribute>().Distinct().FirstOrDefault();

		public static List<DataColumnAttribute> GetDataColumnAttributes(this Type obj, bool inherit = false) {
			List<DataColumnAttribute> list = obj.GetCustomAttributes(inherit).OfType<DataColumnAttribute>().Distinct().ToList();

			// Look at the type interface declarations and extract from that type.
			Type[] interfaces = obj.GetInterfaces();
			foreach (Type intr in interfaces) {
				list.AddRange(intr.GetCustomAttributes(inherit).OfType<DataColumnAttribute>().Distinct().ToList());
			}
			return list;
		}

		public static DataColumnAttribute GetDataColumnAttributes(this PropertyInfo prop, bool inherit = false) {
			DataColumnAttribute attr = prop.GetCustomAttributes(inherit).OfType<DataColumnAttribute>().Distinct().FirstOrDefault();
			if (attr == null)
				return null;
			attr.PropertyName = prop.Name;
			attr.PropertyType = prop.PropertyType;
			return attr;
		}

		public static ApiDataParameterAttribute GetApiDataParametersAttributes(this PropertyInfo prop, bool inherit = false) {
			ApiDataParameterAttribute attr = prop.GetCustomAttributes(inherit).OfType<ApiDataParameterAttribute>().Distinct().FirstOrDefault();
			if (attr == null)
				return null;
			attr.PropertyName = prop.Name;
			attr.PropertyType = prop.PropertyType;
			return attr;
		}

		public static IEnumerable<T> GetCustomAttributesIncludingBaseInterfaces<T>(this Type type, bool inherit = false) {
			Type attributeType = typeof(T);
			return type.GetCustomAttributes(attributeType, inherit).
			  Union(type.GetInterfaces().
			  SelectMany(interfaceType => interfaceType.GetCustomAttributes(attributeType, inherit))).
			  Union(Attribute.GetCustomAttributes(attributeType, typeof(T), inherit)).
			  Distinct().Cast<T>();
		}

		public static List<TAttributeType> GetCustomAttributesFromType<TAttributeType>(this Type typeToReflect, bool inherit = false)
		where TAttributeType : Attribute {
			List<TAttributeType> list = new();
			Type attributeType = typeof(TAttributeType);

			// Get any class level attributes of this attribute type.
			list.AddRange(typeToReflect.GetCustomAttributes(inherit).Distinct().Cast<TAttributeType>().ToList());

			// Loop over the direct property members
			PropertyInfo[] properties = typeToReflect.GetProperties();
			foreach (PropertyInfo propertyInfo in properties) {
				// Get the attributes as well as from the inherited classes (true)
				List<TAttributeType> attributes = propertyInfo.GetCustomAttributes(attributeType, inherit).Distinct().Cast<TAttributeType>().ToList();
				if (!attributes.Any())
					continue;
				list.AddRange(attributes);
			}

			// Look at the type interface declarations and extract from that type.
			Type[] interfaces = typeToReflect.GetInterfaces();
			foreach (Type intr in interfaces) {
				list.AddRange(intr.GetCustomAttributes(typeof(TAttributeType), inherit).Distinct().Cast<TAttributeType>().ToList());
			}

			return list;
		}

		/// <summary>Converts the current list of CodedThought.Core data aware objects to a <see cref="DataTable" />.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">          The list.</param>
		/// <param name="allowInherited">if set to <c>true</c> will allow DataColumns from inherited objects to be applied. The default is false.</param>
		/// <returns></returns>
		public static DataTable ToDataTable<T>(this List<T> list, bool allowInherited = false) {
			DataTable dt = new();
			try {
				List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

				foreach (PropertyInfo info in properties) {
					DataColumnAttribute dataColumn = info.GetDataColumnAttributes(allowInherited);
					DataColumn col = new(info.Name, info.PropertyType.NullableType());
					// Capture the excel column properties for later use.
					foreach (PropertyInfo prop in typeof(DataColumnAttribute).GetProperties()) {
						col.ExtendedProperties.Add(key: prop.Name, value: prop.GetValue(dataColumn));
					}
					dt.Columns.Add(col);
				}
				foreach (T t in list) {
					DataRow row = dt.NewRow();
					foreach (PropertyInfo info in properties) {
						row[info.Name] = info.GetValue(t, null) ?? DBNull.Value;
					}
					dt.Rows.Add(row);
				}
				return dt;
			} catch {
				throw;
			}
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
		public static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new() {
			T item = new();
			foreach (PropertyInfo property in properties) {
				property.SetValue(item, row[property.Name], null);
			}
			return item;
		}

		private static void ColumnHeaderName(DataColumn col, string value) => col.Caption = value;

		private static string ColumnHeaderName(DataColumn col) => col.Caption;

		/// <summary>Sets the property value.</summary>
		/// <param name="prop">         The property.</param>
		/// <param name="obj">          The object.</param>
		/// <param name="propertyValue">The property value.</param>
		public static void SetPropertyValue(this PropertyInfo prop, object obj, object propertyValue) {
			if (prop.CanWrite) {
				prop.SetValue(obj, Convert.ChangeType(propertyValue, prop.PropertyType), null);
			}
		}

		public static Type NullableType(this Type t) {
			Type returnType = t;
			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
				returnType = Nullable.GetUnderlyingType(t);
			}
			return returnType;
		}
	}
}