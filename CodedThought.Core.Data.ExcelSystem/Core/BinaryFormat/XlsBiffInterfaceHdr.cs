namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat
{
    /// <summary>Represents InterfaceHdr record in Wokrbook Globals</summary>
    internal class XlsBiffInterfaceHdr : XlsBiffRecord
    {
        internal XlsBiffInterfaceHdr(byte[] bytes, uint offset, ExcelBinaryReader reader)
            : base(bytes, offset, reader)
        {
        }

        /// <summary>Returns CodePage for Interface Header</summary>
        public ushort CodePage => base.ReadUInt16(0x0);
    }
}