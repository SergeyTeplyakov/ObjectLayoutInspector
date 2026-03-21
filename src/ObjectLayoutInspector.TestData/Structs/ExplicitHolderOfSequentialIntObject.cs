using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Explicit)]
public struct ExplicitHolderOfSequentialIntObjectStruct
    {
        [FieldOffset(0)]
        public SequentialIntObjectStruct theSequentialIntObjectStruct;
    }
}
