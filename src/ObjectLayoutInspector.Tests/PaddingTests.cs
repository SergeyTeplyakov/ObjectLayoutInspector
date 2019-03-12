﻿using System.Linq;
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
            // TODO: for some reason output is different
            var typeLayout = TypeLayout.GetLayout<WithNestedUnsafeStruct>(includePaddings: true);
            var structLayout = UnsafeLayout.GetLayout<WithNestedUnsafeStruct>();
            Assert.AreEqual(Unsafe.SizeOf<WithNestedUnsafeStruct>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            Assert.That(typeLayout.Paddings, Is.EqualTo(0));
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