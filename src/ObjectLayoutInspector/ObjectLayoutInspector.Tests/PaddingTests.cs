using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class PaddingTests
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

        struct WithString
        {
            string s;
        }

        [Test]
        public void LayoutForInstanceWithStringShouldNotCrash()
        {
            // I know, I know. This is bad to name tests like this. But this is life:)
            var layout = TypeLayout.GetLayout<WithString>();
            Assert.That(layout.Fields.Length, Is.EqualTo(1));
            Assert.That(layout.Paddings, Is.EqualTo(0));
        }
    }
}