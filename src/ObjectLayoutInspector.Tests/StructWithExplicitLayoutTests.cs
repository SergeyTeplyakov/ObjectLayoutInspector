using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class StructWithExplicitLayoutTests
    {
        [Test]
        public void Print_NotAlignedStruct()
        {
            var structLayout = UnsafeLayout.GetLayout<NotAlignedStruct>();
            var typeLayout = TypeLayout.GetLayout<NotAlignedStruct>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<NotAlignedStruct>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            TypeLayout.PrintLayout<NotAlignedStruct>();
        }

        [Test]
        public void Print_StructWithExplicitLayout()
        {
            var structLayout = UnsafeLayout.GetLayout<StructWithExplicitLayout>();
            var typeLayout = TypeLayout.GetLayout<StructWithExplicitLayout>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<StructWithExplicitLayout>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            TypeLayout.PrintLayout<StructWithExplicitLayout>();
        }

        [Test]
        public void Print_StructWithExplicitLayoutAndOffsetForFirstField()
        {
            var structLayout = UnsafeLayout.GetLayout<StructWithExplicitLayoutAndOffsetForFirstField>();
            var typeLayout = TypeLayout.GetLayout<StructWithExplicitLayoutAndOffsetForFirstField>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<StructWithExplicitLayoutAndOffsetForFirstField>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            TypeLayout.PrintLayout<StructWithExplicitLayoutAndOffsetForFirstField>();
        }
    }
}
