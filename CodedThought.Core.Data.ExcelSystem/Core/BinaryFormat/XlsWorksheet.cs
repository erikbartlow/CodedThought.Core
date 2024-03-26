namespace CodedThought.Core.Data.ExcelSystem.Core.BinaryFormat
{
    /// <summary>Represents Worksheet section in workbook</summary>
    internal class XlsWorksheet
    {
        private readonly uint m_dataOffset;
        private readonly int m_Index;
        private readonly string m_Name = string.Empty;
        private XlsBiffSimpleValueRecord m_CalcCount;
        private XlsBiffSimpleValueRecord m_CalcMode;
        private XlsBiffRecord m_Delta;
        private XlsBiffDimensions m_Dimensions;
        private XlsBiffSimpleValueRecord m_Iteration;
        private XlsBiffSimpleValueRecord m_RefMode;
        private XlsBiffRecord m_Window;

        public XlsWorksheet(int index, XlsBiffBoundSheet refSheet)
        {
            m_Index = index;
            m_Name = refSheet.SheetName;
            m_dataOffset = refSheet.StartOffset;
        }

        /// <summary>Name of worksheet</summary>
        public string Name => m_Name;

        /// <summary>Zero-based index of worksheet</summary>
        public int Index => m_Index;

        /// <summary>Offset of worksheet data</summary>
        public uint DataOffset => m_dataOffset;

        public XlsBiffSimpleValueRecord CalcMode
        {
            get => m_CalcMode; set => m_CalcMode = value;
        }

        public XlsBiffSimpleValueRecord CalcCount
        {
            get => m_CalcCount; set => m_CalcCount = value;
        }

        public XlsBiffSimpleValueRecord RefMode
        {
            get => m_RefMode; set => m_RefMode = value;
        }

        public XlsBiffSimpleValueRecord Iteration
        {
            get => m_Iteration; set => m_Iteration = value;
        }

        public XlsBiffRecord Delta
        {
            get => m_Delta; set => m_Delta = value;
        }

        /// <summary>Dimensions of worksheet</summary>
        public XlsBiffDimensions Dimensions
        {
            get => m_Dimensions; set => m_Dimensions = value;
        }

        public XlsBiffRecord Window
        {
            get => m_Window; set => m_Window = value;
        }
    }
}