using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class StructLayoutTests : TestsBase
    {
        [Test]
        public void TestNestedStructWithOneField()
        {
            AssertNonRecursiveWithPadding<NestedStructWithOneField>();
        }

        [Test]
        public void TestNonAlignedStruct()
        {
            AssertNonRecursiveWithPadding<NotAlignedStruct>();
        }

        [Test]
        public void TestNonAlignedStructWithPack1()
        {
            TypeLayout.PrintLayout<NotAlignedStructWithPack1>();
            AssertNonRecursiveWithPadding<NotAlignedStructWithPack1>();
        }

        [Test]
        public void TestNonAlignedStructWithAutoLayout()
        {
            TypeLayout.PrintLayout<NotAlignedStructWithAutoLayout>();
            AssertNonRecursiveWithPadding<NotAlignedStructWithAutoLayout>();
        }

        [Test]
        public void Print_ClrWillReorderThisStruct()
        {
            TypeLayout.PrintLayout<ClrWillReorderThisStruct>();
            AssertNonRecursiveWithPadding<ClrWillReorderThisStruct>();
        }

        [Test]
        public void Print_WithVolatile()
        {
            AssertNonRecursive<WithVolatile>();

            TypeLayout.PrintLayout<WithVolatile>();
            var typeLayout = TypeLayout.GetLayout<WithVolatile>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(16));
        }
    }
}