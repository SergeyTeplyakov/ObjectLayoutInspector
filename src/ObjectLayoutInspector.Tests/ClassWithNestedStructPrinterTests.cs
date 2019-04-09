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