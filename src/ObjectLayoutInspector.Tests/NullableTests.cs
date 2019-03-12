using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class NullableTests : TestsBase
    {
        [Test]
        public void Nullable()
        {
            TypeLayout.PrintLayout<Nullable<bool>>();
            var typeLayout = TypeLayout.GetLayout<Nullable<bool>>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<Nullable<bool>>(), typeLayout.Size);
        }

        [Test]
        public void NullableLongByteStruct()
        {
            // why cannot use `public struct Nullable<T> where T : struct`?
            // error CS0453: The type 'int?' must be a non-nullable value type in order to use it as parameter 'T' in the generic type or method 'UnsafeLayout.GetLayout<T>(bool)'
            // var structLayout = UnsafeLayout.GetLayout<Nullable<bool>>();
            // how to do same without generic?
            TypeLayout.PrintLayout<Nullable<LongByteStruct>>();
            var typeLayout = TypeLayout.GetLayout<Nullable<LongByteStruct>>(includePaddings: true);
            var size = Unsafe.SizeOf<Nullable<LongByteStruct>>();
            Assert.AreEqual(typeLayout.Size, size);
        }

        [Test]
        public void WithNullableIntStruct()
        {
            TypeLayout.PrintLayout<WithNullableIntStruct>();
            AssertNonRecursive<WithNullableIntStruct>();
        }

        // TODO: fix UnsafeLayout
        //[Test] 
        public void WithNullableIntNullableBoolIntStructField()
        {
            TypeLayout.PrintLayout<WithNullableIntNullableBoolIntStruct>();
            AssertNonRecursive<WithNullableIntNullableBoolIntStruct>();
        }

        [Test]
        public void WithNullableBoolBoolStructStruct()
        {
            TypeLayout.PrintLayout<WithNullableBoolBoolStructStruct>();
            AssertNonRecursive<WithNullableBoolBoolStructStruct>();
        }

        // TODO: fix UnsafeLayout
        //[Test] 
        public void WithNullableBoolBoolStructStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableBoolBoolStructStruct>();
            var unsafeLayout = UnsafeLayout.GetLayout<WithNullableBoolBoolStructStruct>(recursive: true);
            Assert.AreEqual(3, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
        }

        // TODO: fix UnsafeLayout
        //[Test] 
        public void WithNullableIntStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableIntStruct>();
            var unsafeLayout = UnsafeLayout.GetLayout<WithNullableIntStruct>(recursive: true);
            Assert.AreEqual(2, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
        }
    }
}