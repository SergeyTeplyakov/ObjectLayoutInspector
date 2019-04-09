using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class FixedTests : TestsBase
    {
        [Test]
        public void FixedBytesSame() => AssertNonRecursiveWithPadding<FixedBytes>();

        [Test]
        public void FixedBytesNoPadding()
        {
            var typeLayout = TypeLayout.GetLayout<FixedBytes>(includePaddings: true);
            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
        }
    }
}