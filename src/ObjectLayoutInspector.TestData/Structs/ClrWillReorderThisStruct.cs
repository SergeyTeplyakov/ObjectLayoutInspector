using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    // The following attribute has no effect on the layout!
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClrWillReorderThisStruct
    {
        public byte m_byte1;
        public object m_obj;
        public int m_int;

        public byte m_byte2;
        public short m_short;
    }
}