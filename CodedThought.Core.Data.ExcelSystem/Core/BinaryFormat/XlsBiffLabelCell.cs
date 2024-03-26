using System.Text;

namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat
{
    /// <summary>Represents a string (max 255 bytes)</summary>
    internal class XlsBiffLabelCell : XlsBiffBlankCell
    {
        private Encoding m_UseEncoding = Encoding.Default;

        internal XlsBiffLabelCell(byte[] bytes, uint offset, ExcelBinaryReader reader)
            : base(bytes, offset, reader)
        {
        }

        /// <summary>Encoding used to deal with strings</summary>
        public Encoding UseEncoding
        {
            get => m_UseEncoding; set => m_UseEncoding = value;
        }

        /// <summary>Length of string value</summary>
        public ushort Length => base.ReadUInt16(0x6);

        /// <summary>Returns value of this cell</summary>
        public string Value
        {
            get
            {
                byte[] bts;

                if (reader.isV8())
                {
                    //issue 11636 - according to spec character data starts at byte 9 for biff8 (was using 8)
                    bts = base.ReadArray(0x9, Length * (Helpers.IsSingleByteEncoding(m_UseEncoding) ? 1 : 2));
                }
                else
                { //biff 3-5
                    bts = base.ReadArray(0x2, Length * (Helpers.IsSingleByteEncoding(m_UseEncoding) ? 1 : 2));
                }

                return m_UseEncoding.GetString(bts, 0, bts.Length);
            }
        }
    }
}