using System.Linq;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class GenericStructsTests : TestsBase
    {
        [Test]
        public void GenericStructInt()
        {
            TypeLayout.PrintLayout<GenericStruct<int>>();
            AssertNonRecursiveWithPadding<GenericStruct<int>>();
        }

        [Test]
        public void GenericGenericStructInt()
        {
            TypeLayout.PrintLayout<GenericStruct<GenericStruct<int>>>();
            AssertNonRecursiveWithPadding<GenericStruct<GenericStruct<int>>>();
        }

        [Test]
        public void GenericGenericStructIntFields()
        {
            TypeLayout.PrintLayout<GenericStruct<GenericStruct<int>>>();
            var structLayout = UnsafeLayout.GetFieldsLayout<GenericStruct<GenericStruct<int>>>(recursive: true);
            Assert.AreEqual(3, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(1, structLayout[0].Size);
            Assert.AreEqual(4, structLayout[1].Offset);
            Assert.AreEqual(1, structLayout[1].Size);
            Assert.AreEqual(8, structLayout[2].Offset);
            Assert.AreEqual(4, structLayout[2].Size);
        }
    }
}