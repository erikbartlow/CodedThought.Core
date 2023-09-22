namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat {

	/// <summary>Represents multiple Blank cell</summary>
	internal class XlsBiffMulBlankCell : XlsBiffBlankCell {

		internal XlsBiffMulBlankCell(byte[] bytes, uint offset, ExcelBinaryReader reader)
			: base(bytes, offset, reader) {
		}

		/// <summary>Zero-based index of last described column</summary>
		public ushort LastColumnIndex => base.ReadUInt16(RecordSize - 2);

		/// <summary>Returns format forspecified column, column must be between ColumnIndex and LastColumnIndex</summary>
		/// <param name="ColumnIdx">Index of column</param>
		/// <returns>Format</returns>
		public ushort GetXF(ushort ColumnIdx) {
			int ofs = 4 + 6 * (ColumnIdx - ColumnIndex);
			return ofs > RecordSize - 2 ? (ushort)0 : base.ReadUInt16(ofs);
		}
	}
}