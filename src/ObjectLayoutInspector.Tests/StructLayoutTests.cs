using NUnit.Framework;
using System.Runtime.CompilerServices;
using System;
using ObjectLayoutInspector.Tests.Structs;
using System.Linq;
using System.Numerics;

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
            Assert.That(typeLayout.FullSize, Is.EqualTo(8 + IntPtr.Size));
        }

#if NET8_0_OR_GREATER
        [Test]
        public void Print_Inline1_byte()
        {
            TypeLayout.PrintLayout<Inline1<byte>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<byte>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<byte>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(sizeof(byte)));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_byte()
        {
            TypeLayout.PrintLayout<Inline10<byte>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<byte>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<byte>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * sizeof(byte)));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline1_long()
        {
            TypeLayout.PrintLayout<Inline1<long>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<long>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<long>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(sizeof(long)));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_long()
        {
            TypeLayout.PrintLayout<Inline10<long>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<long>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<long>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * sizeof(long)));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline1_object()
        {
            TypeLayout.PrintLayout<Inline1<object>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<object>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<object>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(IntPtr.Size));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_object()
        {
            TypeLayout.PrintLayout<Inline10<object>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<object>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<object>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * IntPtr.Size));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline1_Complex()
        {
            TypeLayout.PrintLayout<Inline1<Complex>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<Complex>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<Complex>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(Unsafe.SizeOf<Complex>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_Complex()
        {
            TypeLayout.PrintLayout<Inline10<Complex>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<Complex>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<Complex>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * Unsafe.SizeOf<Complex>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline1_ComplexNullable()
        {
            TypeLayout.PrintLayout<Inline1<Complex?>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<Complex?>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<Complex?>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(Unsafe.SizeOf<Complex?>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_ComplexNullable()
        {
            TypeLayout.PrintLayout<Inline10<Complex?>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<Complex?>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<Complex?>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * Unsafe.SizeOf<Complex?>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline1_BoolBoolStruct()
        {
            TypeLayout.PrintLayout<Inline1<BoolBoolStruct>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<BoolBoolStruct>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<BoolBoolStruct>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(Unsafe.SizeOf<BoolBoolStruct>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_BoolBoolStruct()
        {
            TypeLayout.PrintLayout<Inline10<BoolBoolStruct>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<BoolBoolStruct>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<BoolBoolStruct>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * Unsafe.SizeOf<BoolBoolStruct>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline1_EmptyStruct()
        {
            TypeLayout.PrintLayout<Inline1<EmptyStruct>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline1<EmptyStruct>>();
            var typeLayout = TypeLayout.GetLayout<Inline1<EmptyStruct>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(Unsafe.SizeOf<EmptyStruct>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }

        [Test]
        public void Print_Inline10_EmptyStruct()
        {
            TypeLayout.PrintLayout<Inline10<EmptyStruct>>();
            var unsafeLayout = UnsafeLayout.GetLayout<Inline10<EmptyStruct>>();
            var typeLayout = TypeLayout.GetLayout<Inline10<EmptyStruct>>();
            Assert.That(typeLayout.FullSize, Is.EqualTo(10 * Unsafe.SizeOf<EmptyStruct>()));
            Assert.That(typeLayout.Fields.All(x => x is FieldLayout), "Has no padding");
        }
#endif
    }
}