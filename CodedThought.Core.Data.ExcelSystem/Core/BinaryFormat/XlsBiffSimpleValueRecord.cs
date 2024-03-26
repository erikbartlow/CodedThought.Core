namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat
{
    /// <summary>Represents record with the only two-bytes value</summary>
    internal class XlsBiffSimpleValueRecord : XlsBiffRecord
    {
        internal XlsBiffSimpleValueRecord(byte[] bytes, uint offset, ExcelBinaryReader reader)
            : base(bytes, offset, reader)
        {
        }

        /// <summary>Returns value</summary>
        public ushort Value => ReadUInt16(0x0);
    }
}