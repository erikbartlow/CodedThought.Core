namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat {

	/// <summary>Represents a string stored in SST</summary>
	internal class XlsBiffLabelSSTCell : XlsBiffBlankCell {

		internal XlsBiffLabelSSTCell(byte[] bytes, uint offset, ExcelBinaryReader reader)
			: base(bytes, offset, reader) {
		}

		/// <summary>Index of string in Shared String Table</summary>
		public uint SSTIndex => base.ReadUInt32(0x6);

		/// <summary>Returns text using specified SST</summary>
		/// <param name="sst">Shared String Table record</param>
		/// <returns></returns>
		public string Text(XlsBiffSST sst) => sst.GetString(SSTIndex);
	}
}