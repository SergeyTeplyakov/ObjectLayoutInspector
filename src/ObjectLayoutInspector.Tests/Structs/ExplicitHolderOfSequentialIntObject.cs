using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct ExplicitHolderOfSequentialIntObjectStruct
    {
        [FieldOffset(0)]
        public SequentialIntObjectStruct theSequentialIntObjectStruct;
    }
}