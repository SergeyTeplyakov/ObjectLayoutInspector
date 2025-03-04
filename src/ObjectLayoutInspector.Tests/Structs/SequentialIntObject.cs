using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SequentialIntObjectStruct
    {
        public int theInt;

        public object theObject;
    }
}