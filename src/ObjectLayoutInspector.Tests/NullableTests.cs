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
        // cannot test UnsafeLayout.GetLayout<Nullable<bool>>() for  `public struct Nullable<T> where T : struct`?
        // error CS0453: The type 'int?' must be a non-nullable value type in order to use it as parameter 'T' in the generic type or method 'UnsafeLayout.GetLayout<T>(bool)'
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
            TypeLayout.PrintLayout<Nullable<LongByteStruct>>();
            var typeLayout = TypeLayout.GetLayout<Nullable<LongByteStruct>>(includePaddings: true);
            var size = Unsafe.SizeOf<Nullable<LongByteStruct>>();
            Assert.AreEqual(typeLayout.Size, size);
        }

        [Test]
        public void WithNullableIntStruct()
        {
            AssertNonRecursive<WithNullableIntStruct>();
        }

        [Test]
        public void WithNullableIntNullableBoolIntStructField()
        {
            AssertNonRecursive<WithNullableIntNullableBoolIntStruct>();
        }

        [Test]
        public void WithNullableIntIntStruct()
        {
            AssertNonRecursive<WithNullableIntIntStruct>();
        }

        [Test]
        public void WithNullableBoolBoolStructStruct()
        {
            AssertNonRecursive<WithNullableBoolBoolStructStruct>();
        }

        [Test]
        public void WithNullableBoolBoolStructStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableBoolBoolStructStruct>();
            var unsafeLayout = UnsafeLayout.GetFieldsLayout<WithNullableBoolBoolStructStruct>(recursive: true);
            Assert.AreEqual(3, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(1, unsafeLayout[1].Offset);
            Assert.AreEqual(1, unsafeLayout[1].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[1].FieldInfo.FieldType);
            Assert.AreEqual(2, unsafeLayout[2].Offset);
            Assert.AreEqual(1, unsafeLayout[2].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[2].FieldInfo.FieldType);
        }

        [Test]
        public void WithNullableBoolBoolStructStructStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableBoolBoolStructStructStruct>();
            var unsafeLayout = UnsafeLayout.GetFieldsLayout<WithNullableBoolBoolStructStructStruct>(recursive: true);
            Assert.AreEqual(4, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(1, unsafeLayout[1].Offset);
            Assert.AreEqual(1, unsafeLayout[1].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[1].FieldInfo.FieldType);
            Assert.AreEqual(2, unsafeLayout[2].Offset);
            Assert.AreEqual(1, unsafeLayout[2].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[2].FieldInfo.FieldType);
            Assert.AreEqual(3, unsafeLayout[3].Offset);
            Assert.AreEqual(1, unsafeLayout[3].Size);
            Assert.AreEqual(typeof(bool), unsafeLayout[3].FieldInfo.FieldType);
        }

        [Test]
        public void WithNullableIntStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableIntStruct>();
            var unsafeLayout = UnsafeLayout.GetFieldsLayout<WithNullableIntStruct>(recursive: true);
            Assert.AreEqual(2, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
            Assert.AreEqual(4, unsafeLayout[1].Offset);
            Assert.AreEqual(4, unsafeLayout[1].Size);
        }
    }
}