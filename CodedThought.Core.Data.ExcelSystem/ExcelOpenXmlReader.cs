#define DEBUGREADERS

using CodedThought.Core.Data.ExcelSystem.Core;
using CodedThought.Core.Data.ExcelSystem.Core.OpenXmlFormat;
using System.Data;
using System.Globalization;
using System.Xml;

namespace CodedThought.Core.Data.ExcelSystem {

	public class ExcelOpenXmlReader : IExcelDataReader {

		#region Members

		private XlsxWorkbook _workbook;
		private bool _isValid;
		private bool _isClosed;
		private bool _isFirstRead;
		private string _exceptionMessage;
		private int _depth;
		private int _resultIndex;
		private int _emptyRowCount;
		private ZipWorker _zipWorker;
		private XmlReader _xmlReader;
		private Stream _sheetStream;
		private object[] _cellsValues;
		private object[] _savedCellsValues;

		private bool disposed;
		private bool _isFirstRowAsColumnNames;
		private const string COLUMN = "Column";
		private string instanceId = Guid.NewGuid().ToString();

		private List<int> _defaultDateTimeStyles;
		private string _namespaceUri;

		#endregion Members

		internal ExcelOpenXmlReader() {
			_isValid = true;
			_isFirstRead = true;

			_defaultDateTimeStyles = new List<int>(new int[]
			{
				14, 15, 16, 17, 18, 19, 20, 21, 22, 45, 46, 47
			});
		}

		private void ReadGlobals() {
			_workbook = new XlsxWorkbook(
				_zipWorker.GetWorkbookStream(),
				_zipWorker.GetWorkbookRelsStream(),
				_zipWorker.GetSharedStringsStream(),
				_zipWorker.GetStylesStream());

			CheckDateTimeNumFmts(_workbook.Styles.NumFmts);
		}

		private void CheckDateTimeNumFmts(List<XlsxNumFmt> list) {
			if (list.Count == 0)
				return;

			foreach (XlsxNumFmt numFmt in list) {
				if (string.IsNullOrEmpty(numFmt.FormatCode))
					continue;
				string fc = numFmt.FormatCode.ToLower();

				int pos;
				while ((pos = fc.IndexOf('"')) > 0) {
					int endPos = fc.IndexOf('"', pos + 1);

					if (endPos > 0)
						fc = fc.Remove(pos, endPos - pos + 1);
				}

				//it should only detect it as a date if it contains
				//dd mm mmm yy yyyy
				//h hh ss
				//AM PM
				//and only if these appear as "words" so either contained in [ ]
				//or delimted in someway
				//updated to not detect as date if format contains a #
				FormatReader formatReader = new() { FormatString = fc };
				if (formatReader.IsDateFormatString()) {
					_defaultDateTimeStyles.Add(numFmt.Id);
				}
			}
		}

		private void ReadSheetGlobals(XlsxWorksheet sheet) {
			if (_xmlReader != null)
				_xmlReader.Close();
			if (_sheetStream != null)
				_sheetStream.Close();

			_sheetStream = _zipWorker.GetWorksheetStream(sheet.Path);

			if (null == _sheetStream)
				return;

			_xmlReader = XmlReader.Create(_sheetStream);

			//count rows and cols in case there is no dimension elements
			int rows = 0;
			int cols = 0;

			_namespaceUri = null;
			int biggestColumn = 0; //used when no col elements and no dimension
			while (_xmlReader.Read()) {
				if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == XlsxWorksheet.N_worksheet) {
					//grab the namespaceuri from the worksheet element
					_namespaceUri = _xmlReader.NamespaceURI;
				}

				if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == XlsxWorksheet.N_dimension) {
					string dimValue = _xmlReader.GetAttribute(XlsxWorksheet.A_ref);

					sheet.Dimension = new XlsxDimension(dimValue);
					break;
				}

				//removed: Do not use col to work out number of columns as this is really for defining formatting, so may not contain all columns
				//if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == XlsxWorksheet.N_col)
				//    cols++;

				if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == XlsxWorksheet.N_row)
					rows++;

				//check cells so we can find size of sheet if can't work it out from dimension or col elements (dimension should have been set before the cells if it was available)
				//ditto for cols
				if (sheet.Dimension == null && cols == 0 && _xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == XlsxWorksheet.N_c) {
					string refAttribute = _xmlReader.GetAttribute(XlsxWorksheet.A_r);

					if (refAttribute != null) {
						var thisRef = ReferenceHelper.ReferenceToColumnAndRow(refAttribute);
						if (thisRef[1] > biggestColumn)
							biggestColumn = thisRef[1];
					}
				}
			}

			//if we didn't get a dimension element then use the calculated rows/cols to create it
			if (sheet.Dimension == null) {
				if (cols == 0)
					cols = biggestColumn;

				if (rows == 0 || cols == 0) {
					sheet.IsEmpty = true;
					return;
				}

				sheet.Dimension = new XlsxDimension(rows, cols);

				//we need to reset our position to sheet data
				_xmlReader.Close();
				_sheetStream.Close();
				_sheetStream = _zipWorker.GetWorksheetStream(sheet.Path);
				_xmlReader = XmlReader.Create(_sheetStream);
			}

			//read up to the sheetData element. if this element is empty then there aren't any rows and we need to null out dimension

			_xmlReader.ReadToFollowing(XlsxWorksheet.N_sheetData, _namespaceUri);
			if (_xmlReader.IsEmptyElement) {
				sheet.IsEmpty = true;
			}
		}

		private bool ReadSheetRow(XlsxWorksheet sheet) {
			if (null == _xmlReader)
				return false;

			if (_emptyRowCount != 0) {
				_cellsValues = new object[sheet.ColumnsCount];
				_emptyRowCount--;
				_depth++;

				return true;
			}

			if (_savedCellsValues != null) {
				_cellsValues = _savedCellsValues;
				_savedCellsValues = null;
				_depth++;

				return true;
			}

			if ((_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.LocalName == XlsxWorksheet.N_row) ||
				_xmlReader.ReadToFollowing(XlsxWorksheet.N_row, _namespaceUri)) {
				_cellsValues = new object[sheet.ColumnsCount];

				int rowIndex = int.Parse(_xmlReader.GetAttribute(XlsxWorksheet.A_r));
				if (rowIndex != (_depth + 1))
					if (rowIndex != (_depth + 1)) {
						_emptyRowCount = rowIndex - _depth - 1;
					}
				bool hasValue = false;
				string a_s = String.Empty;
				string a_t = String.Empty;
				string a_r = String.Empty;
				int col = 0;
				int row = 0;

				while (_xmlReader.Read()) {
					if (_xmlReader.Depth == 2)
						break;

					if (_xmlReader.NodeType == XmlNodeType.Element) {
						hasValue = false;

						if (_xmlReader.LocalName == XlsxWorksheet.N_c) {
							a_s = _xmlReader.GetAttribute(XlsxWorksheet.A_s);
							a_t = _xmlReader.GetAttribute(XlsxWorksheet.A_t);
							a_r = _xmlReader.GetAttribute(XlsxWorksheet.A_r);
							XlsxDimension.XlsxDim(a_r, out col, out row);
						} else if (_xmlReader.LocalName == XlsxWorksheet.N_v || _xmlReader.LocalName == XlsxWorksheet.N_t) {
							hasValue = true;
						}
					}

					if (_xmlReader.NodeType == XmlNodeType.Text && hasValue) {
						object o = _xmlReader.Value;

						var style = NumberStyles.Any;
						var culture = CultureInfo.InvariantCulture;

						if (double.TryParse(o.ToString(), style, culture, out double number))
							o = number;

						if (null != a_t && a_t == XlsxWorksheet.A_s) //if string
						{
							o = Helpers.ConvertEscapeChars(_workbook.SST[int.Parse(o.ToString())]);
						} // Requested change 4: missing (it appears that if should be else if)
						else if (null != a_t && a_t == XlsxWorksheet.N_inlineStr) //if string inline
						{
							o = Helpers.ConvertEscapeChars(o.ToString());
						} else if (a_t == "b") //boolean
						{
							o = _xmlReader.Value == "1";
						} else if (null != a_s) //if something else
						{
							XlsxXf xf = _workbook.Styles.CellXfs[int.Parse(a_s)];
							if (xf.ApplyNumberFormat && o != null && o.ToString() != string.Empty && IsDateTimeStyle(xf.NumFmtId))
								o = Helpers.ConvertFromOATime(number);
							else if (xf.NumFmtId == 49)
								o = o.ToString();
						}

						if (col - 1 < _cellsValues.Length)
							_cellsValues[col - 1] = o;
					}
				}

				if (_emptyRowCount > 0) {
					_savedCellsValues = _cellsValues;
					return ReadSheetRow(sheet);
				}
				_depth++;

				return true;
			}

			_xmlReader.Close();
			if (_sheetStream != null)
				_sheetStream.Close();

			return false;
		}

		private bool InitializeSheetRead() {
			if (ResultsCount <= 0)
				return false;

			ReadSheetGlobals(_workbook.Sheets[_resultIndex]);

			if (_workbook.Sheets[_resultIndex].Dimension == null)
				return false;

			_isFirstRead = false;

			_depth = 0;
			_emptyRowCount = 0;

			return true;
		}

		private bool IsDateTimeStyle(int styleId) => _defaultDateTimeStyles.Contains(styleId);

		#region IExcelDataReader Members

		public void Initialize(Stream fileStream) {
			_zipWorker = new ZipWorker();
			_zipWorker.Extract(fileStream);

			if (!_zipWorker.IsValid) {
				_isValid = false;
				_exceptionMessage = _zipWorker.ExceptionMessage;

				Close();

				return;
			}

			ReadGlobals();
		}

		public System.Data.DataSet AsDataSet() => AsDataSet(false, true);

		public System.Data.DataSet AsDataSet(bool convertOADatetime) => AsDataSet(convertOADatetime, true);

		public System.Data.DataSet AsDataSet(bool convertOADateTime, bool hasHeaders) {
			if (!_isValid)
				return null;

			DataSet dataset = new();
			IsFirstRowAsColumnNames = hasHeaders;
			for (int ind = 0; ind < _workbook.Sheets.Count; ind++) {
				DataTable table = new(_workbook.Sheets[ind].Name);

				ReadSheetGlobals(_workbook.Sheets[ind]);

				if (_workbook.Sheets[ind].Dimension == null)
					continue;

				_depth = 0;
				_emptyRowCount = 0;

				//DataTable columns
				if (!_isFirstRowAsColumnNames) {
					for (int i = 0; i < _workbook.Sheets[ind].ColumnsCount; i++) {
						table.Columns.Add(null, typeof(Object));
					}
				} else if (ReadSheetRow(_workbook.Sheets[ind])) {
					for (int index = 0; index < _cellsValues.Length; index++) {
						if (_cellsValues[index] != null && _cellsValues[index].ToString().Length > 0)
							Helpers.AddColumnHandleDuplicate(table, _cellsValues[index].ToString());
						else
							Helpers.AddColumnHandleDuplicate(table, string.Concat(COLUMN, index));
					}
				} else
					continue;

				table.BeginLoadData();

				while (ReadSheetRow(_workbook.Sheets[ind])) {
					// Check if this is an empty row.
					if (!IsEmptyRow())
						table.Rows.Add(_cellsValues);
				}

				if (table.Rows.Count > 0)
					dataset.Tables.Add(table);
				table.EndLoadData();
			}
			dataset.AcceptChanges();
			Helpers.FixDataTypes(dataset);
			return dataset;
		}

		private bool IsEmptyRow() {
			bool isEmpty = true;
			for (int i = 0; i <= _cellsValues.Length - 1; i++) {
				if (!IsDBNull(i)) isEmpty = false;
			}
			return isEmpty;
		}

		public bool IsFirstRowAsColumnNames {
			get => _isFirstRowAsColumnNames;
			set => _isFirstRowAsColumnNames = value;
		}

		public bool IsValid => _isValid;
		public string ExceptionMessage => _exceptionMessage;
		public string Name => (_resultIndex >= 0 && _resultIndex < ResultsCount) ? _workbook.Sheets[_resultIndex].Name : null;

		public void Close() {
			_isClosed = true;

			if (_xmlReader != null)
				_xmlReader.Close();

			if (_sheetStream != null)
				_sheetStream.Close();

			if (_zipWorker != null)
				_zipWorker.Dispose();
		}

		public int Depth => _depth;
		public int ResultsCount => _workbook == null ? -1 : _workbook.Sheets.Count;
		public bool IsClosed => _isClosed;

		public bool NextResult() {
			if (_resultIndex >= (this.ResultsCount - 1))
				return false;

			_resultIndex++;

			_isFirstRead = true;
			_savedCellsValues = null;

			return true;
		}

		public bool Read() {
			if (!_isValid)
				return false;

			return (!_isFirstRead || InitializeSheetRead()) && ReadSheetRow(_workbook.Sheets[_resultIndex]);
		}

		public int FieldCount => (_resultIndex >= 0 && _resultIndex < ResultsCount) ? _workbook.Sheets[_resultIndex].ColumnsCount : -1;

		public bool GetBoolean(int i) {
			return !IsDBNull(i) && Boolean.Parse(_cellsValues[i].ToString());
		}

		public DateTime GetDateTime(int i) {
			if (IsDBNull(i))
				return DateTime.MinValue;

			try {
				return (DateTime)_cellsValues[i];
			} catch (InvalidCastException) {
				return DateTime.MinValue;
			}
		}

		public decimal GetDecimal(int i) {
			return IsDBNull(i) ? decimal.MinValue : decimal.Parse(_cellsValues[i].ToString());
		}

		public double GetDouble(int i) {
			return IsDBNull(i) ? double.MinValue : double.Parse(_cellsValues[i].ToString());
		}

		public float GetFloat(int i) {
			return IsDBNull(i) ? float.MinValue : float.Parse(_cellsValues[i].ToString());
		}

		public short GetInt16(int i) {
			return IsDBNull(i) ? short.MinValue : short.Parse(_cellsValues[i].ToString());
		}

		public int GetInt32(int i) {
			return IsDBNull(i) ? int.MinValue : int.Parse(_cellsValues[i].ToString());
		}

		public long GetInt64(int i) {
			return IsDBNull(i) ? long.MinValue : long.Parse(_cellsValues[i].ToString());
		}

		public string GetString(int i) {
			return IsDBNull(i) ? null : _cellsValues[i].ToString();
		}

		public object GetValue(int i) => _cellsValues[i];

		public bool IsDBNull(int i) => (null == _cellsValues[i]) || (DBNull.Value == _cellsValues[i]);

		public object this[int i] => _cellsValues[i];

		#endregion IExcelDataReader Members

		#region IDisposable Members

		public void Dispose() {
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			// Check to see if Dispose has already been called.

			if (!this.disposed) {
				if (disposing) {
					if (_xmlReader != null)
						((IDisposable)_xmlReader).Dispose();
					if (_sheetStream != null)
						_sheetStream.Dispose();
					if (_zipWorker != null)
						_zipWorker.Dispose();
				}

				_zipWorker = null;
				_xmlReader = null;
				_sheetStream = null;

				_workbook = null;
				_cellsValues = null;
				_savedCellsValues = null;

				disposed = true;
			}
		}

		~ExcelOpenXmlReader() {
			Dispose(false);
		}

		#endregion IDisposable Members

		#region Not Supported IDataReader Members

		public DataTable GetSchemaTable() => throw new NotSupportedException();

		public int RecordsAffected => throw new NotSupportedException();

		#endregion Not Supported IDataReader Members

		#region Not Supported IDataRecord Members

		public byte GetByte(int i) => throw new NotSupportedException();

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotSupportedException();

		public char GetChar(int i) => throw new NotSupportedException();

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotSupportedException();

		public IDataReader GetData(int i) => throw new NotSupportedException();

		public string GetDataTypeName(int i) => throw new NotSupportedException();

		public Type GetFieldType(int i) => throw new NotSupportedException();

		public Guid GetGuid(int i) => throw new NotSupportedException();

		public string GetName(int i) => throw new NotSupportedException();

		public int GetOrdinal(string name) => throw new NotSupportedException();

		public int GetValues(object[] values) => throw new NotSupportedException();

		public object this[string name] => throw new NotSupportedException();

		#endregion Not Supported IDataRecord Members
	}
}