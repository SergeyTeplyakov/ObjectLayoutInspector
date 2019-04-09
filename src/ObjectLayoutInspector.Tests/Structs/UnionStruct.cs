using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UnionStruct
    {
        [FieldOffset(0)]
        public float one;

        [FieldOffset(0)]
        public long two;
    }
}
