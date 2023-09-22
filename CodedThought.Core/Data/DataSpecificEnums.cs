namespace CodedThought.Core.Data {

	#region enums

	/// <summary>List of Databases supported</summary>
	public enum DBSupported {

		/// <summary>Oracle</summary>
		Oracle = 0,

		/// <summary>SQL Server</summary>
		SqlServer,

		/// <summary>MySql database</summary>
		MySql,

		/// <summary>OleDb</summary>
		OleDb,

		/// <summary>MongoDb</summary>
		MongoDb,

		/// <summary>REST Api</summary>
		/// <remarks>
		/// It is worth noting that the REST Api database object is significantly less functional than a direct database connection. Most interface methods simply do not work in a REST environment.
		/// </remarks>
		ApiServer
	};

	/// <summary>Database types that are supported</summary>
	public enum DbTypeSupported {

		/// <summary>Use for non Unicode strings.</summary>
		dbVarChar = 0,

		/// <summary>SmallInt</summary>
		dbInt16,

		/// <summary>int</summary>
		dbInt32,

		/// <summary>BigInt</summary>
		dbInt64,

		/// <summary>double</summary>
		dbDouble,

		/// <summary>DateTime</summary>
		dbDateTime,

		/// <summary>DateTime2</summary>
		dbDateTime2,

		/// <summary>Char</summary>
		dbChar,

		/// <summary>Binary Large Object</summary>
		dbBlob,

		/// <summary>Bit</summary>
		dbBit,

		/// <summary>decimal (scaled int)</summary>
		dbDecimal,

		/// <summary>Use for binary data for files.</summary>
		dbImage,

		/// <summary>Use for binary data for files.</summary>
		dbVarBinary,

		/// <summary>GUID</summary>
		dbGUID,

		/// <summary>Use for standard string and Unicode character strings.</summary>
		dbNVarChar
	};

	/// <summary>Database vendor name enum.</summary>
	public enum DBVendor {
		SqlServer,
		Oracle,
		MySql,
		OleDb
	}

	/// <summary>List of Date format string</summary>
	public enum DateFormat {

		/// <summary>format MM/DD/YYYY</summary>
		MMDDYYYY = 0,

		/// <summary>format Mon DD, YYYY</summary>
		MonDDYYYY,

		/// <summary>format MM-DD-YYYY</summary>
		MMDDYYYY_Hyphen
	};

	/// <summary>List of Date format string</summary>
	public enum FunctionName {

		/// <summary>function name of substring</summary>
		SUBSTRING = 0,

		/// <summary>ISNULL/NVL</summary>
		ISNULL,

		/// <summary>Now</summary>
		CURRENTDATE,

		/// <summary>CONCATENATing String</summary>
		CONCATENATE
	};

	/// <summary>List of Date format string</summary>
	public enum DatePart {

		/// <summary>Day</summary>
		DAY = 0,

		/// <summary>Month</summary>
		MONTH,

		/// <summary>Year</summary>
		YEAR
	};

	public enum WhereType {
		AND,
		OR,
		NOT
	};

	#endregion enums
}