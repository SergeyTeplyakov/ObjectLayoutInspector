using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class CorefxTypesTests
    {
        [Test]
        public void ComplexComplex()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<System.Numerics.Complex>();
            var typeLayout = TypeLayout.GetLayout<System.Numerics.Complex>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
        }

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
            Assert.AreEqual(0, structLayout.Count());
        }

        [Test]
        public void DateTimeOffset()
        {
            TypeLayout.PrintLayout<DateTimeOffset>();
            var typeLayout = TypeLayout.GetLayout<DateTimeOffset>(includePaddings: true);
            var structLayout = UnsafeLayout.GetLayout<DateTimeOffset>();
            Assert.AreEqual(typeLayout.Fields.Count(), structLayout.Count());
        }

        [Test]
        public void DateTime()
        {
            TypeLayout.PrintLayout<DateTime>();
            var typeLayout = TypeLayout.GetLayout<DateTime>(includePaddings: true);
            var structLayout = UnsafeLayout.GetLayout<DateTime>();
            Assert.AreEqual(typeLayout.Fields.Count(), structLayout.Count());
        }

        [Test]
        public void TimeSpan()
        {
            TypeLayout.PrintLayout<TimeSpan>();
            var typeLayout = TypeLayout.GetLayout<TimeSpan>(includePaddings: true);
            var structLayout = UnsafeLayout.GetLayout<TimeSpan>();
            Assert.AreEqual(typeLayout.Fields.Count(), structLayout.Count());
        }

        [Test]
        public void Tuple()
        {
            TypeLayout.PrintLayout<(bool, int, float)>();
            var typeLayout = TypeLayout.GetLayout<(bool, int, float)>(includePaddings: true);
            var structLayout = UnsafeLayout.GetLayout<(bool, int, float)>();
            Assert.AreEqual(typeLayout.Fields.Count(), structLayout.Count());
        }        
    }
}