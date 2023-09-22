namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat {

	/// <summary>Represents multiple RK number cells</summary>
	internal class XlsBiffMulRKCell : XlsBiffBlankCell {

		internal XlsBiffMulRKCell(byte[] bytes, uint offset, ExcelBinaryReader reader)
			: base(bytes, offset, reader) {
		}

		/// <summary>Returns zero-based index of last described column</summary>
		public ushort LastColumnIndex => base.ReadUInt16(RecordSize - 2);

		/// <summary>Returns format for specified column</summary>
		/// <param name="ColumnIdx">Index of column, must be between ColumnIndex and LastColumnIndex</param>
		/// <returns></returns>
		public ushort GetXF(ushort ColumnIdx) {
			int ofs = 4 + 6 * (ColumnIdx - ColumnIndex);
			return ofs > RecordSize - 2 ? (ushort)0 : base.ReadUInt16(ofs);
		}

		/// <summary>Returns value for specified column</summary>
		/// <param name="ColumnIdx">Index of column, must be between ColumnIndex and LastColumnIndex</param>
		/// <returns></returns>
		public double GetValue(ushort ColumnIdx) {
			int ofs = 6 + 6 * (ColumnIdx - ColumnIndex);
			return ofs > RecordSize ? 0 : XlsBiffRKCell.NumFromRK(base.ReadUInt32(ofs));
		}
	}
}