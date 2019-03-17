using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ExplicitLayoutNoOverlapStruct
    {
        [FieldOffset(0)]
        public float one;

        [FieldOffset(10)]
        public long two;
    }
}
