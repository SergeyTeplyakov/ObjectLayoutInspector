using NUnit.Framework;
using ObjectLayoutInspector.Tests.Objects;
using ObjectLayoutInspector.Tests.Structs;
using System;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class ClassWithNestedStructPrinterTests
    {
        [Test]
        public void Print()
        {
            TypeLayout.PrintLayout<ClassWithNestedCustomStruct>();
            var lo = TypeLayout.GetLayout<ClassWithNestedCustomStruct>();
        }

        [Test]
        public void PrintString()
        {
            TypeLayout.PrintLayout<string>();
            var lo = TypeLayout.GetLayout<string>();
        }

        [Test]
        public void PrintObject()
        {
            TypeLayout.PrintLayout<object>();
            var lo = TypeLayout.GetLayout<object>();
        }

        [Test]
        public void PrintNullableInt()
        {
            TypeLayout.PrintLayout<int?>();
            var lo = TypeLayout.GetLayout<int?>();
        }

        [Test]
        public void PrintNullable()
        {
            TypeLayout.PrintLayout<EmptyStruct?>();
            var lo = TypeLayout.GetLayout<EmptyStruct?>();
        }
    }
}