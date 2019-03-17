using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Auto)]
    public struct NotAlignedStructWithAutoLayout
    {
        public byte m_byte1;
        public int m_int;

        public byte m_byte2;
        public short m_short;
    }
}