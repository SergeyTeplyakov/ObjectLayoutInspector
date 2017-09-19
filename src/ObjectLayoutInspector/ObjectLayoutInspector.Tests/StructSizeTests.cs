using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Sequential, Size = 42)]
    struct MyStruct
    {
        
    }

    [TestFixture]
    public class StructSizeTests
    {
        [Test]
        public void TestStructWithExplicitSize()
        {
            Console.WriteLine(Marshal.SizeOf(typeof(MyStruct)));
        }
    }
}