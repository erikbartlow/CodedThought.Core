namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat
{
    /// <summary>Represents BIFF BOF record</summary>
    internal class XlsBiffBOF : XlsBiffRecord
    {
        internal XlsBiffBOF(byte[] bytes, uint offset, ExcelBinaryReader reader)
            : base(bytes, offset, reader)
        {
        }

        /// <summary>Version</summary>
        public ushort Version => ReadUInt16(0x0);

        /// <summary>Type of BIFF block</summary>
        public BIFFTYPE Type => (BIFFTYPE) ReadUInt16(0x2);

        /// <summary>Creation ID</summary>
        /// <remarks>Not used before BIFF5</remarks>
        public ushort CreationID
        {
            get
            {
                return RecordSize < 6 ? (ushort) 0 : ReadUInt16(0x4);
            }
        }

        /// <summary>Creation year</summary>
        /// <remarks>Not used before BIFF5</remarks>
        public ushort CreationYear
        {
            get
            {
                return RecordSize < 8 ? (ushort) 0 : ReadUInt16(0x6);
            }
        }

        /// <summary>File history flag</summary>
        /// <remarks>Not used before BIFF8</remarks>
        public uint HistoryFlag
        {
            get
            {
                return RecordSize < 12 ? 0 : ReadUInt32(0x8);
            }
        }

        /// <summary>Minimum Excel version to open this file</summary>
        /// <remarks>Not used before BIFF8</remarks>
        public uint MinVersionToOpen
        {
            get
            {
                return RecordSize < 16 ? 0 : ReadUInt32(0xC);
            }
        }
    }
}