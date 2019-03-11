using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class UnsafeLayoutTests
    {
        [Test]
        public void GetFieldLayoutLongByte()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<LongByteStruct>();
            var typeLayout = TypeLayout.GetLayout<LongByteStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeof(long), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
            Assert.AreEqual(typeof(byte), structLayout[1].FieldInfo.FieldType);
        }

        [Test]
        public void GetLayoutLongByte()
        {
            var structLayout = UnsafeLayout.GetLayout<LongByteStruct>();
            var typeLayout = TypeLayout.GetLayout<LongByteStruct>(includePaddings: true).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
            Assert.AreEqual(typeLayout[2].Offset, structLayout[2].Offset);
            Assert.AreEqual(typeLayout[2].Size, structLayout[2].Size);
        }

        [Test]
        public void GetFieldLayoutEnumIntStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<EnumIntStruct>();
            var typeLayout = TypeLayout.GetLayout<EnumIntStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeof(ByteEnum), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
            Assert.AreEqual(typeof(int), structLayout[1].FieldInfo.FieldType);
        }

        [Test]
        public void GetLayoutEnumIntStruct()
        {
            var structLayout = UnsafeLayout.GetLayout<EnumIntStruct>();
            var typeLayout = TypeLayout.GetLayout<EnumIntStruct>(includePaddings: true).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            TypeLayout.PrintLayout<EnumIntStruct>();
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
        }

        [Test]
        public void GetFieldLayoutFloatFloatFloatStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<FloatFloatFloatStruct>();
            var typeLayout = TypeLayout.GetLayout<FloatFloatFloatStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeof(float), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
            Assert.AreEqual(typeof(float), structLayout[1].FieldInfo.FieldType);
            Assert.AreEqual(typeLayout[2].Offset, structLayout[2].Offset);
            Assert.AreEqual(typeLayout[2].Size, structLayout[2].Size);
            Assert.AreEqual(typeof(float), structLayout[2].FieldInfo.FieldType);
        }

        [Test]
        public void GetFieldLayoutFloatFloatStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<FloatFloatStruct>();
            Assert.AreEqual(2, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(4, structLayout[0].Size);
            Assert.AreEqual(typeof(float), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(4, structLayout[1].Offset);
            Assert.AreEqual(4, structLayout[1].Size);
            Assert.AreEqual(typeof(float), structLayout[1].FieldInfo.FieldType);
        }




        [Test]
        public void GetFieldLayoutStructsNonRecursive()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<ComplexStruct>(recursive: false);
            var typeLayout = TypeLayout.GetLayout<ComplexStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void GetFieldLayoutStructs()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<ComplexStruct>();
            Assert.AreEqual(4, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(8, structLayout[1].Offset);
            Assert.AreEqual(16, structLayout[2].Offset);
            Assert.AreEqual(20, structLayout[3].Offset);
        }

        [Test]
        public void GetFieldLayoutDoubleFloatStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<DoubleFloatStruct>();
            Assert.AreEqual(2, structLayout.Count(), 2);
            Assert.AreEqual(0, structLayout[0].Offset, 0);
            Assert.AreEqual(8, structLayout[0].Size, 8);
            Assert.AreEqual(typeof(double), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(8, structLayout[1].Offset);
            Assert.AreEqual(4, structLayout[1].Size);
            Assert.AreEqual(typeof(float), structLayout[1].FieldInfo.FieldType);
        }

        [Test]
        public void GetFieldLayoutByteEnum()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<ByteEnum>();
            Assert.AreEqual(0, structLayout.Count());
        }

        [Test]
        public void GetFieldLayoutByte()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<byte>();
            Assert.AreEqual(0, structLayout.Count());
        }

        [Test]
        public void GetFieldLayoutZero()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<EmptyStruct>();
            Assert.AreEqual(0, structLayout.Count());
        }

        [Test]
        public void GetFieldLayoutByteEnumStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<EnumStruct>();
            Assert.AreEqual(1, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(1, structLayout[0].Size);
            Assert.AreEqual(typeof(ByteEnum), structLayout[0].FieldInfo.FieldType);
        }

        [Test]
        public void UnionStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<UnionStruct>();
            var typeLayout = TypeLayout.GetLayout<UnionStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void UnionStructPadding()
        {
            var structLayout = UnsafeLayout.GetLayout<UnionStruct>();
            var typeLayout = TypeLayout.GetLayout<UnionStruct>(includePaddings: true).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void ExplicitLayoutOverlapStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<ExplicitLayoutOverlapStruct>();
            var typeLayout = TypeLayout.GetLayout<ExplicitLayoutOverlapStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void ExplicitLayoutOverlapStructPadding()
        {
            var structLayout = UnsafeLayout.GetLayout<ExplicitLayoutOverlapStruct>();
            var typeLayout = TypeLayout.GetLayout<ExplicitLayoutOverlapStruct>(includePaddings: true).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void ExplicitLayoutNoOverlapStruct()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<ExplicitLayoutNoOverlapStruct>();
            var typeLayout = TypeLayout.GetLayout<ExplicitLayoutNoOverlapStruct>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void ExplicitLayoutNoOverlapStructPadding()
        {
            var structLayout = UnsafeLayout.GetLayout<ExplicitLayoutNoOverlapStruct>();
            var typeLayout = TypeLayout.GetLayout<ExplicitLayoutNoOverlapStruct>(includePaddings: true).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
        }

        [Test]
        public void NullableInt()
        {
            // why cannot use `public struct Nullable<T> where T : struct`?
            // error CS0453: The type 'int?' must be a non-nullable value type in order to use it as parameter 'T' in the generic type or method 'UnsafeLayout.GetLayout<T>(bool)'
            // var structLayout = UnsafeLayout.GetLayout<Nullable<int>>();
        }

        [Test]
        public void ComplexComplex()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<System.Numerics.Complex>();
            var typeLayout = TypeLayout.GetLayout<System.Numerics.Complex>(includePaddings: false).Fields;
            Assert.AreEqual(typeLayout.Count(), structLayout.Count());
            Assert.AreEqual(typeLayout[0].Offset, structLayout[0].Offset);
            Assert.AreEqual(typeLayout[0].Size, structLayout[0].Size);
            Assert.AreEqual(typeLayout[1].Offset, structLayout[1].Offset);
            Assert.AreEqual(typeLayout[1].Size, structLayout[1].Size);
        }

        [Test]
        public void ComplexInsideAsPrimitive()
        {
            var structLayout = UnsafeLayout.GetFieldLayout<CustomPrimitive>(primitives: new HashSet<Type> { typeof(Complex) });
            Assert.AreEqual(1, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(16, structLayout[0].Size);
        }

        [Test]
        public void Vector2AsPrimitive()
        {
            TypeLayout.PrintLayout<Vector2>();
            var structLayout = UnsafeLayout.GetFieldLayout<Vector2>(primitives: new HashSet<Type> { typeof(Vector2) });
            Assert.AreEqual(0, structLayout.Count());            
        }
    }
}