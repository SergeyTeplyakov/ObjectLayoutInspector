using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class PaddingTests : TestsBase
    {
        [Test]
        public void UnsafeStructHasEmptyPaddings()
        {
            AssertNonRecursiveWithPadding<WithNestedUnsafeStruct>();
            var typeLayout = TypeLayout.GetLayout<WithNestedUnsafeStruct>(includePaddings: true);
            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
        }

        // TODO: fix fixed inside struct paddings rules and test (fixed + bool, bool + fixed, fixed byte 1/2/3/4,5/6/7 w/wo bool)        
        //[Test]
        public void WithNestedUnsafeUnsafeStruct()
        {
            AssertNonRecursiveWithPadding<WithNestedUnsafeUnsafeStruct>();
            var typeLayout = TypeLayout.GetLayout<WithNestedUnsafeUnsafeStruct>(includePaddings: true);
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