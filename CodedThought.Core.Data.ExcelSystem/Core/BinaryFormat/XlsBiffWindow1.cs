namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat {

	/// <summary>Represents Workbook's global window description</summary>
	internal class XlsBiffWindow1 : XlsBiffRecord {

		#region Window1Flags enum

		[Flags]
		public enum Window1Flags : ushort {
			Hidden = 0x1,
			Minimized = 0x2,
			//(Reserved) = 0x4,

			HScrollVisible = 0x8,
			VScrollVisible = 0x10,
			WorkbookTabs = 0x20
			//(Other bits are reserved)
		}

		#endregion Window1Flags enum

		internal XlsBiffWindow1(byte[] bytes, uint offset, ExcelBinaryReader reader)
			: base(bytes, offset, reader) {
		}

		/// <summary>Returns X position of a window</summary>
		public ushort Left => base.ReadUInt16(0x0);

		/// <summary>Returns Y position of a window</summary>
		public ushort Top => base.ReadUInt16(0x2);

		/// <summary>Returns width of a window</summary>
		public ushort Width => base.ReadUInt16(0x4);

		/// <summary>Returns height of a window</summary>
		public ushort Height => base.ReadUInt16(0x6);

		/// <summary>Returns window flags</summary>
		public Window1Flags Flags => (Window1Flags)base.ReadUInt16(0x8);

		/// <summary>Returns active workbook tab (zero-based)</summary>
		public ushort ActiveTab => base.ReadUInt16(0xA);

		/// <summary>Returns first visible workbook tab (zero-based)</summary>
		public ushort FirstVisibleTab => base.ReadUInt16(0xC);

		/// <summary>Returns number of selected workbook tabs</summary>
		public ushort SelectedTabCount => base.ReadUInt16(0xE);

		/// <summary>Returns workbook tab width to horizontal scrollbar width</summary>
		public ushort TabRatio => base.ReadUInt16(0x10);
	}
}