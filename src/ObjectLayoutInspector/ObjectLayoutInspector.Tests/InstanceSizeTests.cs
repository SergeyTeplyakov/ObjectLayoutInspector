using System;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class InstanceSizeTests
    {
        class Empty { }

        [Test]
        public void SizeForEmptyClassIs3PtrSizes()
        {
            var layout = TypeLayout.GetLayout<Empty>();
            Assert.That(layout.Size, Is.EqualTo(IntPtr.Size * 3));
        }

        [Test]
        public void NoFieldsForEmptyClass()
        {
            var layout = TypeLayout.GetLayout<Empty>();
            Assert.That(layout.Fields, Is.Empty);
        }
    }
}