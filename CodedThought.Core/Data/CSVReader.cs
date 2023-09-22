namespace CodedThought.Core.Data {

	/// <summary>Simple CSV Reader</summary>
	public class CSVReader {

		/// <summary>Returns the contents of the stream as a DataSet.</summary>
		/// <param name="stream">   The stream.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns></returns>
		public static DataSet AsDataSet(FileStream stream, bool hasColumnHeaders, String delimiter) {
			DataSet ds = new();
			ds.Tables.Add();
			String strLine;
			StreamReader reader = new(stream);

			strLine = reader.ReadLine();
			while (strLine != null) {
				//DataRow row = new DataRow();
				//ds.Tables[0].Rows.Add(
			}
			return ds;
		}

		/// <summary>Returns the contents of the file passed as a DataSet.</summary>
		/// <param name="file">     The file.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns></returns>
		public static DataSet AsDataSet(String file, bool hasColumnHeaders, String delimiter) {
			DataSet ds = new();
			int colCount = 0;
			int rowStart = (hasColumnHeaders == true ? 1 : 0);
			ds.Tables.Add();
			String[] allLines = File.ReadAllLines(file);
			for (int c = 0; c <= ParseCsvRow(allLines[0], delimiter).Length - 1; c++) {
				ds.Tables[0].Columns.Add();
			}
			for (int i = rowStart; i <= allLines.Length - 1; i++) {
				DataRow drow = ds.Tables[0].NewRow();
				foreach (string col in ParseCsvRow(allLines[i], delimiter)) {
					drow[colCount] = col;
					colCount++;
				}
				// Reset the column count.
				colCount = 0;
				ds.Tables[0].Rows.Add(drow);
			}
			return ds;
		}

		/// <summary>Returns the contents of the file passed as a DataSet.</summary>
		/// <param name="file">            The file.</param>
		/// <param name="hasColumnHeaders">if set to <c>true</c> [has column headers].</param>
		/// <param name="delimiter">       The delimiter.</param>
		/// <returns></returns>
		public static DataSet AsDataSet(String file, bool hasColumnHeaders, char[] delimiter) {
			DataSet ds = new();
			int colCount = 0;
			int rowStart = (hasColumnHeaders == true ? 1 : 0);
			ds.Tables.Add();
			String[] allLines = File.ReadAllLines(file);
			for (int c = 0; c <= ParseCsvRow(allLines[0], delimiter).Length - 1; c++) {
				ds.Tables[0].Columns.Add();
			}
			for (int i = rowStart; i <= allLines.Length - 1; i++) {
				DataRow drow = ds.Tables[0].NewRow();
				foreach (string col in ParseCsvRow(allLines[i], delimiter)) {
					drow[colCount] = col;
					colCount++;
				}
				// Reset the column count.
				colCount = 0;
				ds.Tables[0].Rows.Add(drow);
			}
			return ds;
		}

		/// <summary>Returns the contents of the file passed as a DataSet.</summary>
		/// <param name="file">            The file.</param>
		/// <param name="hasColumnHeaders">if set to <c>true</c> [has column headers].</param>
		/// <param name="delimiter">       The delimiter.</param>
		/// <returns></returns>
		public static DataSet AsDataSet(String file, bool hasColumnHeaders, string[] delimiter) {
			DataSet ds = new();
			int colCount = 0;
			int rowStart = (hasColumnHeaders == true ? 1 : 0);
			ds.Tables.Add();
			String[] allLines = File.ReadAllLines(file);
			for (int c = 0; c <= ParseCsvRow(allLines[0], delimiter).Length - 1; c++) {
				ds.Tables[0].Columns.Add();
			}
			for (int i = rowStart; i <= allLines.Length - 1; i++) {
				DataRow drow = ds.Tables[0].NewRow();
				foreach (string col in ParseCsvRow(allLines[i], delimiter)) {
					drow[colCount] = col;
					colCount++;
				}
				// Reset the column count.
				colCount = 0;
				ds.Tables[0].Rows.Add(drow);
			}
			return ds;
		}

		/// <summary>Parses the CSV row.</summary>
		/// <param name="row">      The row.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns></returns>
		private static string[] ParseCsvRow(String row, string delimiter) {
			string[] c;
			List<string> resp = new();
			bool cont = false;
			string cs = "";

			c = row.Split(delimiter.ToCharArray(), StringSplitOptions.None);

			foreach (string y in c) {
				string x = y;

				if (cont) {
					// End of field
					if (x.EndsWith("\"")) {
						cs += delimiter + x.Substring(0, x.Length - 1);
						resp.Add(cs);
						cs = "";
						cont = false;
						continue;
					} else {
						// Field still not ended
						cs += delimiter + x;
						continue;
					}
				}

				// Fully encapsulated with no comma within
				if (x.StartsWith("\"") && x.EndsWith("\"")) {
					if ((x.EndsWith("\"\"") && !x.EndsWith("\"\"\"")) && x != "\"\"") {
						cont = true;
						cs = x;
						continue;
					}

					resp.Add(x.Substring(1, x.Length - 2));
					continue;
				}

				// Start of encapsulation but comma has split it into at least next field
				if (x.StartsWith("\"") && !x.EndsWith("\"")) {
					cont = true;
					cs += x.Substring(1);
					continue;
				}

				// Non encapsulated complete field
				resp.Add(x);
			}

			return resp.ToArray();
		}

		/// <summary>Parses the CSV row.</summary>
		/// <param name="row">      The row.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns></returns>
		private static string[] ParseCsvRow(String row, char[] delimiter) {
			string[] c;
			List<string> resp = new();
			bool cont = false;
			string cs = "";

			c = row.Split(delimiter, StringSplitOptions.None);

			foreach (string y in c) {
				string x = y;

				if (cont) {
					// End of field
					if (x.EndsWith("\"")) {
						cs += delimiter + x.Substring(0, x.Length - 1);
						resp.Add(cs);
						cs = "";
						cont = false;
						continue;
					} else {
						// Field still not ended
						cs += delimiter + x;
						continue;
					}
				}

				// Fully encapsulated with no comma within
				if (x.StartsWith("\"") && x.EndsWith("\"")) {
					if ((x.EndsWith("\"\"") && !x.EndsWith("\"\"\"")) && x != "\"\"") {
						cont = true;
						cs = x;
						continue;
					}

					resp.Add(x.Substring(1, x.Length - 2));
					continue;
				}

				// Start of encapsulation but comma has split it into at least next field
				if (x.StartsWith("\"") && !x.EndsWith("\"")) {
					cont = true;
					cs += x.Substring(1);
					continue;
				}

				// Non encapsulated complete field
				resp.Add(x);
			}

			return resp.ToArray();
		}

		/// <summary>Parses the CSV row.</summary>
		/// <param name="row">      The row.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns></returns>
		private static string[] ParseCsvRow(String row, string[] delimiter) {
			string[] c;
			List<string> resp = new();
			bool cont = false;
			string cs = "";

			c = row.Split(delimiter, StringSplitOptions.None);

			foreach (string y in c) {
				string x = y;

				if (cont) {
					// End of field
					if (x.EndsWith("\"")) {
						cs += delimiter + x.Substring(0, x.Length - 1);
						resp.Add(cs);
						cs = "";
						cont = false;
						continue;
					} else {
						// Field still not ended
						cs += delimiter + x;
						continue;
					}
				}

				// Fully encapsulated with no comma within
				if (x.StartsWith("\"") && x.EndsWith("\"")) {
					if ((x.EndsWith("\"\"") && !x.EndsWith("\"\"\"")) && x != "\"\"") {
						cont = true;
						cs = x;
						continue;
					}

					resp.Add(x.Substring(1, x.Length - 2));
					continue;
				}

				// Start of encapsulation but comma has split it into at least next field
				if (x.StartsWith("\"") && !x.EndsWith("\"")) {
					cont = true;
					cs += x.Substring(1);
					continue;
				}

				// Non encapsulated complete field
				resp.Add(x);
			}

			return resp.ToArray();
		}
	}
}