using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{

    [TestFixture]
    public class FixedTests
    {
        [Test]
        public void FixedBytes()
        {
            TypeLayout.PrintLayout<FixedBytes>();
            var structLayout = UnsafeLayout.GetLayout<FixedBytes>();
            var typeLayout = TypeLayout.GetLayout<FixedBytes>(includePaddings: true);
            Assert.AreEqual(typeLayout.Fields[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout.Fields[0].Size, structLayout[0].Size);

            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
        }
    }
}