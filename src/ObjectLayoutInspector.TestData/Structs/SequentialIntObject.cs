using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Sequential)]
public struct SequentialIntObjectStruct
    {
        public int theInt;

        public object theObject;
    }
}
