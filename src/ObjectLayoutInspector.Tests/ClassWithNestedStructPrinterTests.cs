using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
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