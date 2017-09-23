using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class UnsafeStructsTests
    {
        unsafe struct FixedBytes
        {
            public const int MaxLength = 33;
            private fixed byte _bytes[MaxLength];
        }

        struct WithNestedUnsafeStruct
        {
            private FixedBytes fb;
        }

        [Test]
        public void UnsafeStructHasEmptyPaddings()
        {
            var typeLayout = TypeLayout.GetLayout<WithNestedUnsafeStruct>();
            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
        }
    }
}