using System.Collections;

namespace CodedThought.Core.Data {

	public sealed class DbTypeConvertor {

		private struct DbTypeMapEntry {
			public Type Type;
			public DbType DbType;
			public SqlDbType SqlDbType;

			public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType) {
				this.Type = type;
				this.DbType = dbType;
				this.SqlDbType = sqlDbType;
			}
		};

		private static ArrayList _DbTypeList = new();

		#region Constructors

		static DbTypeConvertor() {
			DbTypeMapEntry dbTypeMapEntry
			= new(typeof(bool), DbType.Boolean, SqlDbType.Bit);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(DateTime), DbType.DateTime2, SqlDbType.DateTime2);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(Decimal), DbType.Decimal, SqlDbType.Decimal);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(Int16), DbType.Int16, SqlDbType.SmallInt);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(Int32), DbType.Int32, SqlDbType.Int);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(Int64), DbType.Int64, SqlDbType.BigInt);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar);
			_DbTypeList.Add(dbTypeMapEntry);

			dbTypeMapEntry
			= new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.NVarChar);
			_DbTypeList.Add(dbTypeMapEntry);
		}

		private DbTypeConvertor() {
		}

		#endregion Constructors

		#region Methods

		/// <summary>Convert db type to .Net data type</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static Type ToNetType(DbType dbType) {
			DbTypeMapEntry entry = Find(dbType);
			return entry.Type;
		}

		/// <summary>Convert TSQL type to .Net data type</summary>
		/// <param name="sqlDbType"></param>
		/// <returns></returns>
		public static Type ToNetType(SqlDbType sqlDbType) {
			DbTypeMapEntry entry = Find(sqlDbType);
			return entry.Type;
		}

		/// <summary>Convert .Net type to Db type</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static DbType ToDbType(Type type) {
			DbTypeMapEntry entry = Find(type);
			return entry.DbType;
		}

		/// <summary>Convert TSQL data type to DbType</summary>
		/// <param name="sqlDbType"></param>
		/// <returns></returns>
		public static DbType ToDbType(SqlDbType sqlDbType) {
			DbTypeMapEntry entry = Find(sqlDbType);
			return entry.DbType;
		}

		/// <summary>Convert .Net type to TSQL data type</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static SqlDbType ToSqlDbType(Type type) {
			DbTypeMapEntry entry = Find(type);
			return entry.SqlDbType;
		}

		/// <summary>Convert DbType type to TSQL data type</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static SqlDbType ToSqlDbType(DbType dbType) {
			DbTypeMapEntry entry = Find(dbType);
			return entry.SqlDbType;
		}

		private static DbTypeMapEntry Find(Type type) {
			object retObj = null;
			for (int i = 0; i < _DbTypeList.Count; i++) {
				DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
				if (entry.Type == (Nullable.GetUnderlyingType(type) ?? type)) {
					retObj = entry;
					break;
				}
			}
			return retObj == null
				? throw
				new ApplicationException("Referenced an unsupported Type")
				: (DbTypeMapEntry)retObj;
		}

		private static DbTypeMapEntry Find(DbType dbType) {
			object retObj = null;
			for (int i = 0; i < _DbTypeList.Count; i++) {
				DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
				if (entry.DbType == dbType) {
					retObj = entry;
					break;
				}
			}
			return retObj == null
				? throw
				new ApplicationException("Referenced an unsupported DbType")
				: (DbTypeMapEntry)retObj;
		}

		private static DbTypeMapEntry Find(SqlDbType sqlDbType) {
			object retObj = null;
			for (int i = 0; i < _DbTypeList.Count; i++) {
				DbTypeMapEntry entry = (DbTypeMapEntry)_DbTypeList[i];
				if (entry.SqlDbType == sqlDbType) {
					retObj = entry;
					break;
				}
			}
			return retObj == null
				? throw
				new ApplicationException("Referenced an unsupported SqlDbType")
				: (DbTypeMapEntry)retObj;
		}

		#endregion Methods
	}
}