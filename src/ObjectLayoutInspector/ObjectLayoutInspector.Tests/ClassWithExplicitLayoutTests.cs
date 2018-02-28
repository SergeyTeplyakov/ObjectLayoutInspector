using System.Runtime.InteropServices;
using NUnit.Framework;

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

    [StructLayout(LayoutKind.Explicit, Size = 100)]
    public class ClassWithExplicitLayoutAndOffsetForFirstField
    {
        [FieldOffset(10)]
        public byte m_byte1;
        [FieldOffset(11)]
        public byte m_byte1_1;
    }

    [TestFixture]
    public class ClassWithExplicitLayoutTests
    {
        [Test]
        public void Print_StructWithExplicitLayout()
        {
            TypeLayout.PrintLayout<ClassWithExplicitLayout>();
        }

        [Test]
        public void Print_StructWithExplicitLayoutAndOffsetForFirstField()
        {
            TypeLayout.PrintLayout<ClassWithExplicitLayoutAndOffsetForFirstField>();
        }
    }
}