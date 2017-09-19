using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class ClassLayoutTests
    {
        [StructLayout(LayoutKind.Auto)]
        public class NotAlignedClass
        {
            public byte m_byte1;
            public int m_int;

            public byte m_byte2;
            public short m_short;
        }

        [Test]
        public void Print_NotAlignedClass()
        {
            TypeLayout.PrintLayout<NotAlignedClass>();
        }
    }
}