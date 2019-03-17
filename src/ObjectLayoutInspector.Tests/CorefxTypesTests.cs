using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class CorefxTypesTests : TestsBase
    {
        [Test]
        public void ComplexComplex() => AssertNonRecursive<System.Numerics.Complex>();

        [Test]
        public void ComplexInsideAsPrimitive()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<CustomPrimitive>(primitives: new HashSet<Type> { typeof(Complex) });
            Assert.AreEqual(1, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(16, structLayout[0].Size);
        }

        [Test]
        public void Vector2AsPrimitive()
        {
            TypeLayout.PrintLayout<Vector2>();
            var structLayout = UnsafeLayout.GetFieldsLayout<Vector2>(primitives: new HashSet<Type> { typeof(Vector2) });
            Assert.AreEqual(1, structLayout.Count());
        }

        [Test]
        public void DateTimeOffset()
        {
            TypeLayout.PrintLayout<DateTimeOffset>();
            AssertNonRecursiveWithPadding<DateTimeOffset>();
        }

        [Test]
        public void DateTime()
        {
            TypeLayout.PrintLayout<DateTime>();
            AssertNonRecursiveWithPadding<DateTime>();
        }

        [Test]
        public void TimeSpan()
        {
            TypeLayout.PrintLayout<TimeSpan>();
            AssertNonRecursiveWithPadding<TimeSpan>();
        }

        [Test]
        public void Tuple()
        {
            TypeLayout.PrintLayout<(bool, int, float)>();
            AssertNonRecursiveWithPadding<(bool, int, float)>();
        }
    }
}