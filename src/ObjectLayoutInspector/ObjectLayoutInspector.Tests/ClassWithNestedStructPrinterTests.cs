using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    //[StructLayout(LayoutKind.Auto)]
    public struct NotAlignedStruct
    {
        public byte m_byte1;
        public int m_int;
        public byte m_byte2;
        public short m_short;
    }

    public class ClassWithNestedCustomStruct
    {
        public byte b;
        public NotAlignedStruct sp1;
    }

    [TestFixture]
    public class ClassWithNestedStructPrinterTests
    {
        [Test]
        public void Print()
        {
            TypeLayout.PrintLayout<ClassWithNestedCustomStruct>();
        }
    }
}