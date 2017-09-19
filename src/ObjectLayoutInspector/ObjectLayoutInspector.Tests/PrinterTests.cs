using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

    [TestFixture]
    public class PrinterTests
    {
        [Test]
        public void Print_NotAlignedStruct()
        {
            ObjectLayoutInspector.InspectorHelper.Print<NotAlignedStruct>();
        }

        [Test]
        public void Print_StructWithExplicitLayout()
        {
            ObjectLayoutInspector.InspectorHelper.Print<StructWithExplicitLayout>();
        }

        [Test]
        public void Print_Node()
        {
            ObjectLayoutInspector.InspectorHelper.Print<DerivedExpression>();
        }
    }
}
