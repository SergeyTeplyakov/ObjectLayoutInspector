using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Explicit)]
    public class ClassWithExplicitLayout
    {
        [FieldOffset(0)]
        public byte m_byte1;
        [FieldOffset(0)]
        public byte m_byte1_1;
        [FieldOffset(6)]
        public int m_int;
        [FieldOffset(1)]
        public byte m_byte2;
        [FieldOffset(2)]
        public short m_short;
    }
}