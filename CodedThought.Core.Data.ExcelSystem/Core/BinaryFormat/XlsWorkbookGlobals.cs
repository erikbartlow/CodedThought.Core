namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat {

	/// <summary>Represents Globals section of workbook</summary>
	internal class XlsWorkbookGlobals {
		private readonly List<XlsBiffRecord> m_ExtendedFormats = new();
		private readonly List<XlsBiffRecord> m_Fonts = new();
		private readonly Dictionary<ushort, XlsBiffFormatString> m_Formats = new();
		private readonly List<XlsBiffBoundSheet> m_Sheets = new();
		private readonly List<XlsBiffRecord> m_Styles = new();
		private XlsBiffSimpleValueRecord m_Backup;
		private XlsBiffSimpleValueRecord m_CodePage;
		private XlsBiffRecord m_Country;
		private XlsBiffRecord m_DSF;
		private XlsBiffRecord m_ExtSST;
		private XlsBiffInterfaceHdr m_InterfaceHdr;

		private XlsBiffRecord m_MMS;
		private XlsBiffSST m_SST;

		private XlsBiffRecord m_WriteAccess;

		public XlsBiffInterfaceHdr InterfaceHdr {
			get => m_InterfaceHdr; set => m_InterfaceHdr = value;
		}

		public XlsBiffRecord MMS {
			get => m_MMS; set => m_MMS = value;
		}

		public XlsBiffRecord WriteAccess {
			get => m_WriteAccess; set => m_WriteAccess = value;
		}

		public XlsBiffSimpleValueRecord CodePage {
			get => m_CodePage; set => m_CodePage = value;
		}

		public XlsBiffRecord DSF {
			get => m_DSF; set => m_DSF = value;
		}

		public XlsBiffRecord Country {
			get => m_Country; set => m_Country = value;
		}

		public XlsBiffSimpleValueRecord Backup {
			get => m_Backup; set => m_Backup = value;
		}

		public List<XlsBiffRecord> Fonts => m_Fonts;
		public Dictionary<ushort, XlsBiffFormatString> Formats => m_Formats;

		public List<XlsBiffRecord> ExtendedFormats => m_ExtendedFormats;
		public List<XlsBiffRecord> Styles => m_Styles;
		public List<XlsBiffBoundSheet> Sheets => m_Sheets;

		/// <summary>Shared String Table of workbook</summary>
		public XlsBiffSST SST {
			get => m_SST; set => m_SST = value;
		}

		public XlsBiffRecord ExtSST {
			get => m_ExtSST; set => m_ExtSST = value;
		}
	}
}