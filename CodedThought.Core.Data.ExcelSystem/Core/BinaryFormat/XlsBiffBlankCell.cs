namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat {

	/// <summary>Represents blank cell Base class for all cell types</summary>
	internal class XlsBiffBlankCell : XlsBiffRecord {

		internal XlsBiffBlankCell(byte[] bytes, uint offset, ExcelBinaryReader reader)
			: base(bytes, offset, reader) {
		}

		/// <summary>Zero-based index of row containing this cell</summary>
		public ushort RowIndex => base.ReadUInt16(0x0);

		/// <summary>Zero-based index of column containing this cell</summary>
		public ushort ColumnIndex => base.ReadUInt16(0x2);

		/// <summary>Format used for this cell</summary>
		public ushort XFormat => base.ReadUInt16(0x4);
	}
}