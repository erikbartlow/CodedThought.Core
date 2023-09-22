using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace CodedThought.Core.Data.ExcelSystem {

	public class ExcelOpenXmlWriter {

		public static bool CreateExcelDocument<T>(List<T> list, string xlsxFilePath) {
			DataSet ds = new();
			ds.Tables.Add(ListToDataTable(list));

			return CreateExcelDocument(ds, xlsxFilePath);
		}

		#region HELPER_FUNCTIONS

		// This function is adapated from: http://www.codeguru.com/forum/showthread.php?t=450171 My thanks to Carl Quirion, for making it "nullable-friendly".
		public static DataTable ListToDataTable<T>(List<T> list) {
			DataTable dt = new();

			foreach (PropertyInfo info in typeof(T).GetProperties()) {
				dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
			}
			foreach (T t in list) {
				DataRow row = dt.NewRow();
				foreach (PropertyInfo info in typeof(T).GetProperties()) {
					row[info.Name] = !IsNullableType(info.PropertyType) ? info.GetValue(t, null) : info.GetValue(t, null) ?? DBNull.Value;
				}
				dt.Rows.Add(row);
			}
			return dt;
		}

		private static Type GetNullableType(Type t) {
			Type returnType = t;
			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
				returnType = Nullable.GetUnderlyingType(t);
			}
			return returnType;
		}

		private static bool IsNullableType(Type type) => (type == typeof(string) ||
					type.IsArray ||
					(type.IsGenericType &&
					 type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));

		public static bool CreateExcelDocument(DataTable dt, string xlsxFilePath) {
			DataSet ds = new();
			ds.Tables.Add(dt);
			bool result = CreateExcelDocument(ds, xlsxFilePath);
			ds.Tables.Remove(dt);
			return result;
		}

		#endregion HELPER_FUNCTIONS

		/// <summary>Creates the excel document as stream from the passed DataSet.</summary>
		/// <param name="ds">      The ds.</param>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		public static System.IO.Stream CreateExcelDocumentAsStream(DataSet ds, string filename) {
			try {
				System.IO.MemoryStream stream = new();
				using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true)) {
					WriteExcelFile(ds, document);
				}
				stream.Flush();
				stream.Position = 0;
				byte[] data1 = new byte[stream.Length];
				stream.Read(data1, 0, data1.Length);

				return stream;
			} catch (Exception ex) {
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return null;
			}
		}

		/// <summary>Creates the excel document as stream from the passed generic list of objects.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">    The list.</param>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		public static System.IO.Stream CreateExcelDocumentAsStream<T>(List<T> list, string filename) {
			try {
				DataSet ds = new();
				ds.Tables.Add(ListToDataTable(list));

				System.IO.MemoryStream stream = new();
				using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true)) {
					WriteExcelFile(ds, document);
				}
				stream.Flush();
				stream.Position = 0;
				byte[] data1 = new byte[stream.Length];
				stream.Read(data1, 0, data1.Length);

				return stream;
			} catch (Exception ex) {
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return null;
			}
		}

		/// <summary>Create an Excel file, and writes it out to the response buffer as a MemoryStream (rather than directly to a file)</summary>
		/// <param name="ds">      DataSet containing the data to be written to the Excel.</param>
		/// <param name="filename">The filename (without a path) to call the new Excel file.</param>
		/// <param name="context"> HttpResponse of the current page.</param>
		/// <returns>Either a MemoryStream, or NULL if something goes wrong.</returns>
		//public static bool CreateExcelDocumentAsStream(DataSet ds, string filename, HttpContext context) {
		//    try {
		//        System.IO.MemoryStream stream = new System.IO.MemoryStream();
		//        using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true)) {
		//            WriteExcelFile(ds, document);
		//        }
		//        stream.Flush();
		//        stream.Position = 0;

		// context.Response..ClearContent(); context.Clear(); context.Buffer = true; context.Charset = "";

		// // NOTE: If you get an "HttpCacheability does not exist" error on the following line, make sure you have // manually added System.Web to this project's References.

		// context.Cache.SetCacheability(System.Web.HttpCacheability.NoCache); context.AddHeader("content-disposition", "attachment; filename=" + filename); context.ContentType =
		// "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; byte[] data1 = new byte[stream.Length]; stream.Read(data1, 0, data1.Length); stream.Close(); context.BinaryWrite(data1);
		// context.Flush(); context.End();

		//        return true;
		//    } catch (Exception ex) {
		//        Trace.WriteLine("Failed, exception thrown: " + ex.Message);
		//        return false;
		//    }
		//}

		/// <summary>Create an Excel file, and write it to a file.</summary>
		/// <param name="ds">           DataSet containing the data to be written to the Excel.</param>
		/// <param name="excelFilename">Name of file to be written.</param>
		/// <returns>True if successful, false if something went wrong.</returns>
		public static bool CreateExcelDocument(DataSet ds, string excelFilename) {
			try {
				using (SpreadsheetDocument document = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook)) {
					WriteExcelFile(ds, document);
				}
				Trace.WriteLine("Successfully created: " + excelFilename);
				return true;
			} catch (Exception ex) {
				Trace.WriteLine("Failed, exception thrown: " + ex.Message);
				return false;
			}
		}

		private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet) {
			// Create the Excel file contents. This function is used when creating an Excel file either writing to a file, or writing to a MemoryStream.
			spreadsheet.AddWorkbookPart();
			spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

			// My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
			spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

			// If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
			WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
			Stylesheet stylesheet = new();
			workbookStylesPart.Stylesheet = stylesheet;

			// Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
			uint worksheetNumber = 1;
			foreach (DataTable dt in ds.Tables) {
				// For each worksheet you want to create
				string workSheetID = "rId" + worksheetNumber.ToString();
				string worksheetName = dt.TableName;

				WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
				newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();

				// create sheet data
				newWorksheetPart.Worksheet.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.SheetData());

				// save worksheet
				WriteDataTableToExcelWorksheet(dt, newWorksheetPart);
				newWorksheetPart.Worksheet.Save();

				// create the worksheet to workbook relation
				if (worksheetNumber == 1)
					spreadsheet.WorkbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

				spreadsheet.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>().AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheet() {
					Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
					SheetId = (uint)worksheetNumber,
					Name = dt.TableName
				});

				worksheetNumber++;
			}

			spreadsheet.WorkbookPart.Workbook.Save();
		}

		private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart) {
			var worksheet = worksheetPart.Worksheet;
			var sheetData = worksheet.GetFirstChild<SheetData>();

			string cellValue = "";

			// Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
			//
			// We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual cells of data, we'll know if to write Text values or Numeric
			// cell values.
			int numberOfColumns = dt.Columns.Count;
			bool[] IsNumericColumn = new bool[numberOfColumns];

			string[] excelColumnNames = new string[numberOfColumns];
			for (int n = 0; n < numberOfColumns; n++)
				excelColumnNames[n] = GetExcelColumnName(n);

			// Create the Header row in our Excel Worksheet
			uint rowIndex = 1;

			Row headerRow = new() { RowIndex = rowIndex };  // add a row at the top of spreadsheet
			sheetData.Append(headerRow);

			for (int colInx = 0; colInx < numberOfColumns; colInx++) {
				DataColumn col = dt.Columns[colInx];
				AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, headerRow);
				IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32");
			}

			// Now, step through each row of data in our DataTable...
			double cellNumericValue = 0;
			foreach (DataRow dr in dt.Rows) {
				// ...create a new row, and append a set of this row's data to it.
				++rowIndex;
				Row newExcelRow = new() { RowIndex = rowIndex };  // add a row at the top of spreadsheet
				sheetData.Append(newExcelRow);

				for (int colInx = 0; colInx < numberOfColumns; colInx++) {
					cellValue = dr.ItemArray[colInx].ToString();

					// Create cell with data
					if (IsNumericColumn[colInx]) {
						// For numeric cells, make sure our input data IS a number, then write it out to the Excel file. If this numeric value is NULL, then don't write anything to the Excel file.
						cellNumericValue = 0;
						if (double.TryParse(cellValue, out cellNumericValue)) {
							cellValue = cellNumericValue.ToString();
							AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
						}
					} else {
						// For text cells, just write the input data straight out to the Excel file.
						AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
					}
				}
			}
		}

		private static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow) {
			// Add a new Excel Cell to our Row
			Cell cell = new() { CellReference = cellReference, DataType = CellValues.String };
			CellValue cellValue = new() {
				Text = cellStringValue
			};
			cell.Append(cellValue);
			excelRow.Append(cell);
		}

		private static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow) {
			// Add a new Excel Cell to our Row
			Cell cell = new() { CellReference = cellReference };
			CellValue cellValue = new() {
				Text = cellStringValue
			};
			cell.Append(cellValue);
			excelRow.Append(cell);
		}

		private static string GetExcelColumnName(int columnIndex) {
			// Convert a zero-based column index into an Excel column reference (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
			//
			// eg GetExcelColumnName(0) should return "A" GetExcelColumnName(1) should return "B" GetExcelColumnName(25) should return "Z" GetExcelColumnName(26) should return "AA"
			// GetExcelColumnName(27) should return "AB" ..etc..
			if (columnIndex < 26)
				return ((char)('A' + columnIndex)).ToString();

			char firstChar = (char)('A' + (columnIndex / 26) - 1);
			char secondChar = (char)('A' + (columnIndex % 26));

			return string.Format("{0}{1}", firstChar, secondChar);
		}
	}
}