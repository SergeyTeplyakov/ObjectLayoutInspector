using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Explicit)]
    public struct StructWithExplicitLayout
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
    public struct StructWithExplicitLayoutAndOffsetForFirstField
    {
        [FieldOffset(10)]
        public byte m_byte1;
        [FieldOffset(11)]
        public byte m_byte1_1;
    }

    [TestFixture]
    public class StructWithExplicitLayoutTests
    {
        [Test]
        public void Print_NotAlignedStruct()
        {
            TypeLayout.PrintLayout<NotAlignedStruct>();
        }

        [Test]
        public void Print_StructWithExplicitLayout()
        {
            TypeLayout.PrintLayout<StructWithExplicitLayout>();
        }

        [Test]
        public void Print_StructWithExplicitLayoutAndOffsetForFirstField()
        {
            TypeLayout.PrintLayout<StructWithExplicitLayoutAndOffsetForFirstField>();
        }
    }
}
