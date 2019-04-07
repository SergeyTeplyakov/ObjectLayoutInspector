using System;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class InstanceSizeTests
    {
        [Test]
        public void SizeForEmptyClassIs3PtrSizes()
        {
            var layout = TypeLayout.GetLayout<EmptyClass>();
            Assert.That(layout.FullSize, Is.EqualTo(IntPtr.Size * 3));
        }

        [Test]
        public void NoFieldsForEmptyClass()
        {
            var layout = TypeLayout.GetLayout<EmptyClass>();
            Assert.That(layout.Fields, Is.Empty);
        }
    }
}