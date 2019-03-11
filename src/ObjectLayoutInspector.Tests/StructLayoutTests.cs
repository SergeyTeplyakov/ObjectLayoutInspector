using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class StructLayoutTests
    {
        [Test]
        public void TestNonAlignedStruct()
        {
            var structLayout = UnsafeLayout.GetLayout<NotAlignedStruct>();
            var typeLayout = TypeLayout.GetLayout<NotAlignedStruct>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<NotAlignedStruct>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());
            Assert.AreEqual(typeLayoutFields[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayoutFields[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayoutFields[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayoutFields[1].Size, structLayout[1].Size);
            Assert.AreEqual(typeLayoutFields[2].Offset, structLayout[2].Offset);
            Assert.AreEqual(typeLayoutFields[2].Size, structLayout[2].Size);
            Assert.AreEqual(typeLayoutFields[3].Offset, structLayout[3].Offset);
            Assert.AreEqual(typeLayoutFields[3].Size, structLayout[3].Size);
        }

        [Test]
        public void TestNonAlignedStructWithPack1()
        {
            var structLayout = UnsafeLayout.GetLayout<NotAlignedStructWithPack1>();
            var typeLayout = TypeLayout.GetLayout<NotAlignedStructWithPack1>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<NotAlignedStructWithPack1>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            TypeLayout.PrintLayout<NotAlignedStructWithPack1>();
        }

        [Test]
        public void TestNonAlignedStructWithAutoLayout()
        {
            var structLayout = UnsafeLayout.GetLayout<NotAlignedStructWithAutoLayout>();
            var typeLayout = TypeLayout.GetLayout<NotAlignedStructWithAutoLayout>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<NotAlignedStructWithAutoLayout>(), typeLayout.Size);
            var typeLayoutFields = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            TypeLayout.PrintLayout<NotAlignedStructWithAutoLayout>();
        }

        [Test]
        public void Print_ClrWillReorderThisStruct()
        {
            var structLayout = UnsafeLayout.GetLayout<ClrWillReorderThisStruct>();
            var typeLayout = TypeLayout.GetLayout<ClrWillReorderThisStruct>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<ClrWillReorderThisStruct>(), typeLayout.Size);
            var typeLayoutFields  = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            TypeLayout.PrintLayout<ClrWillReorderThisStruct>();
        }

        [Test]
        public void Print_WithVolatile()
        {
            var structLayout = UnsafeLayout.GetLayout<WithVolatile>();
            var typeLayout = TypeLayout.GetLayout<WithVolatile>();
            Assert.AreEqual(Unsafe.SizeOf<WithVolatile>(), typeLayout.Size);
            var typeLayoutFields  = typeLayout.Fields;
            Assert.AreEqual(typeLayoutFields.Count(), structLayout.Count());

            Console.WriteLine(Marshal.SizeOf<WithVolatile>());

            TypeLayout.PrintLayout<WithVolatile>();

            Assert.That(typeLayout.FullSize, Is.EqualTo(16));
        }
    }
}