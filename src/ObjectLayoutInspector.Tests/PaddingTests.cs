using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class PaddingTests : TestsBase
    {
        // TODO: BUG: fix it for UnsafeLayout. Propagate fixed from within inside. test on ComplexFixedStruct either
        //[Test]
        public void UnsafeStructHasEmptyPaddings()
        {
            // TODO: out put is different. we should count oadding 
            var typeLayout = TypeLayout.GetLayout<WithNestedUnsafeStruct>(includePaddings: true);
            var structLayout = UnsafeLayout.GetLayout<WithNestedUnsafeStruct>(recursive:false);
            Assert.AreEqual(Unsafe.SizeOf<WithNestedUnsafeStruct>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
        }

       [Test]
        public void UnsafeStructRecursive()
        {
            TypeLayout.PrintLayout<WithNestedUnsafeStruct>();
            var structLayout = UnsafeLayout.GetLayout<WithNestedUnsafeStruct>();
            Assert.AreEqual(1, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(33, structLayout[0].Size);
        }        

        [Test]
        public void LayoutForInstanceWithStringShouldNotCrash()
        {
            AssertNonRecursiveWithPadding<WithString>();

            // I know, I know. This is bad to name tests like this. But this is life:)
            var layout = TypeLayout.GetLayout<WithString>();
            Assert.That(layout.Fields.Length, Is.EqualTo(1));
            Assert.That(layout.Paddings, Is.EqualTo(0));
        }
    }
}