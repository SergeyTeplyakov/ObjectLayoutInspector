using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class StructLayoutTests
    {
        public struct NotAlignedStruct
        {
            public byte m_byte1;
            public int m_int;

            public byte m_byte2;
            public short m_short;
        }

        [Test]
        public void TestNonAlignedStruct()
        {
            TypeLayout.PrintLayout<NotAlignedStruct>();
        }

        public struct NotAlignedStructWithPack1
        {
            public byte m_byte1;
            public int m_int;

            public byte m_byte2;
            public short m_short;
        }

        [Test]
        public void TestNonAlignedStructWithPack1()
        {
            TypeLayout.PrintLayout<NotAlignedStructWithPack1>();
        }

        [StructLayout(LayoutKind.Auto)]
        public struct NotAlignedStructWithAutoLayout
        {
            public byte m_byte1;
            public int m_int;

            public byte m_byte2;
            public short m_short;
        }

        [Test]
        public void TestNonAlignedStructWithAutoLayout()
        {
            TypeLayout.PrintLayout<NotAlignedStructWithAutoLayout>();
        }

        // The following attribute has no effect on the layout!
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ClrWillReorderThisStruct
        {
            public byte m_byte1;
            public object m_obj;
            public int m_int;

            public byte m_byte2;
            public short m_short;
        }

        [Test]
        public void Print_ClrWillReorderThisStruct()
        {
            TypeLayout.PrintLayout<ClrWillReorderThisStruct>();
        }

        public struct WithVolatile
        {
            private object obj;
            private int i1;
            private int i2;
        }

        [Test]
        public void Print_WithVolatile()
        {
            Console.WriteLine(Marshal.SizeOf<WithVolatile>());
            
            TypeLayout.PrintLayout<WithVolatile>();

            var layout = TypeLayout.GetLayout<WithVolatile>();
            Assert.That(layout.FullSize, Is.EqualTo(16));
        }
    }
}