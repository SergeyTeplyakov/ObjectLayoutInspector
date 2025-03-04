using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ExplicitLayoutOverlapStruct
    {
        [FieldOffset(0)]
        public float one;

        [FieldOffset(2)]
        public long two;
    }
}
