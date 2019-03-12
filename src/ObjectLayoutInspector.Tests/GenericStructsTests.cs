using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{

    [TestFixture]
    public class GenericStructsTests
    {
        [Test]
        public void GenericStructInt()
        {
            TypeLayout.PrintLayout<GenericStruct<int>>();
            var structLayout = UnsafeLayout.GetLayout<GenericStruct<int>>();
            var typeLayout = TypeLayout.GetLayout<GenericStruct<int>>(includePaddings: true);
            Assert.AreEqual(typeLayout.Fields[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout.Fields[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayout.Fields[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout.Fields[1].Size, structLayout[1].Size);
            Assert.AreEqual(typeLayout.Fields[2].Offset, structLayout[2].Offset);
            Assert.AreEqual(typeLayout.Fields[2].Size, structLayout[2].Size);
        }

        [Test]
        public void GenericGenericStructInt()
        {
            TypeLayout.PrintLayout<GenericStruct<GenericStruct<int>>>();
            var structLayout = UnsafeLayout.GetFieldsLayout<GenericStruct<GenericStruct<int>>>(recursive: false);
            var typeLayout = TypeLayout.GetLayout<GenericStruct<GenericStruct<int>>>(includePaddings: false);
            Assert.AreEqual(typeLayout.Fields.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout.Fields[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout.Fields[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayout.Fields[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout.Fields[1].Size, structLayout[1].Size);
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