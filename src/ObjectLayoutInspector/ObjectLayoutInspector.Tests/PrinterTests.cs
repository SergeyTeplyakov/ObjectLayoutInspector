using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    public struct ClrWillReorderThisStruct
    {
        public byte m_byte1;
        public object m_obj;
        public int m_int;

        public byte m_byte2;
        public short m_short;
    }

    public struct NotAlignedStruct
    {
        public byte m_byte1;
        public int m_int;

        public byte m_byte2;
        public short m_short;
    }

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

    

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 40)]
    public struct StructWithPack1
    {
        public byte m_byte1;
        public int m_int;
        public byte m_byte2;
        public short m_short;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Weird
    {
        //private object o;
        public StructWithPack1 sp1;
        public StructWithPack1 sp2;
    }

    public class NotAlignedClass
    {
        public byte m_byte1;
        public int m_int;

        public byte m_byte2;
        public short m_short;
    }

    [TestFixture]
    public class PrinterTests
    {
        [Test]
        public void Print_ClrWillReorderThisStruct()
        {
            ObjectLayoutInspector2.Utils.Print<ClrWillReorderThisStruct>();
        }

        [Test]
        public void Print_NotAlignedStruct()
        {
            ObjectLayoutInspector2.Utils.Print<NotAlignedStruct>();
        }

        [Test]
        public void Print_NotAlignedClass()
        {
            ObjectLayoutInspector2.Utils.Print<NotAlignedClass>();
        }

        [Test]
        public void Print_StructWithExplicitLayout()
        {
            ObjectLayoutInspector2.Utils.Print<StructWithExplicitLayout>();
        }

        [Test]
        public void Print_StructWithPack1()
        {
            //var size = System.Runtime.InteropServices.Marshal.SizeOf<StructWithPack1>();
            //Console.WriteLine(size);
            ObjectLayoutInspector2.Utils.Print<StructWithPack1>();
        }

        [Test]
        public void Print_Weird()
        {
            ObjectLayoutInspector2.Utils.Print<Weird>();
        }

        [Test]
        public void Print_Node()
        {
            ObjectLayoutInspector2.Utils.Print<DerivedExpression>();
        }
    }
}
