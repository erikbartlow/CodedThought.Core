namespace CodedThought.Core.Data.ExcelSystem.Core.OpenXmlFormat
{
    internal class XlsxWorksheet
    {
        public const string N_dimension = "dimension";
        public const string N_worksheet = "worksheet";
        public const string N_row = "row";
        public const string N_col = "col";
        public const string N_c = "c"; //cell
        public const string N_v = "v";
        public const string N_t = "t";
        public const string A_ref = "ref";
        public const string A_r = "r";
        public const string A_t = "t";
        public const string A_s = "s";
        public const string N_sheetData = "sheetData";
        public const string N_inlineStr = "inlineStr";

        private XlsxDimension _dimension;

        public bool IsEmpty { get; set; }

        public XlsxDimension Dimension
        {
            get => _dimension; set => _dimension = value;
        }

        public int ColumnsCount => IsEmpty ? 0 : (_dimension == null ? -1 : _dimension.LastCol);

        public int RowsCount => _dimension == null ? -1 : _dimension.LastRow - _dimension.FirstRow + 1;

        private string _Name;

        public string Name => _Name;
        private int _id;

        public int Id => _id;
        private string _rid;

        public string RID
        {
            get => _rid;
            set => _rid = value;
        }

        private string _path;

        public string Path
        {
            get => _path;
            set => _path = value;
        }

        public XlsxWorksheet(string name, int id, string rid)
        {
            _Name = name;
            _id = id;
            _rid = rid;
        }
    }
}