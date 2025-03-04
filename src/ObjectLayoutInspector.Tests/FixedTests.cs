using NUnit.Framework;
using ObjectLayoutInspector.Tests.Structs;

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
            TypeLayout.PrintLayout<FixedBytes>(recursively: true);
            var typeLayout = TypeLayout.GetLayout<FixedBytes>(includePaddings: true);
            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
        }
    }
}