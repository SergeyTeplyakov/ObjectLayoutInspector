using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class UnsafeLayoutTests : TestsBase
    {
        [Test]
        public void GetFieldsLayoutLongByte() => AssertNonRecursive<LongByteStruct>();

        [Test]
        public void GetLayoutLongByte() => AssertNonRecursiveWithPadding<LongByteStruct>();

        [Test]
        public void GetFieldsLayoutEnumIntStruct() => AssertNonRecursive<EnumIntStruct>();

        [Test]
        public void GetLayoutEnumIntStruct() => AssertNonRecursiveWithPadding<EnumIntStruct>();

        [Test]
        public void GetFieldsLayoutFloatFloatFloatStruct() => AssertNonRecursive<FloatFloatFloatStruct>();

        [Test]
        public void GetFieldsLayoutFloatFloatStruct()
        {
            TypeLayout.PrintLayout<FloatFloatStruct>();
            AssertNonRecursive<FloatFloatStruct>();
        }

        [Test]
        public void GetFieldsLayoutDoubleFloatStruct()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<DoubleFloatStruct>();
            Assert.AreEqual(2, structLayout.Count(), 2);
            Assert.AreEqual(0, structLayout[0].Offset, 0);
            Assert.AreEqual(8, structLayout[0].Size, 8);
            Assert.AreEqual(typeof(double), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(8, structLayout[1].Offset);
            Assert.AreEqual(4, structLayout[1].Size);
            Assert.AreEqual(typeof(float), structLayout[1].FieldInfo.FieldType);
        }

        [Test]
        public void GetFieldsLayoutByteEnum()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<ByteEnum>();
            Assert.AreEqual(1, structLayout.Count());
        }

        [Test]
        public void GetFieldsLayoutByteRecursive()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<byte>();
            Assert.AreEqual(1, structLayout.Count());
        }

        [Test]
        public void GetFieldsLayoutByte() => AssertNonRecursive<byte>();

        [Test]
        public void GetFieldsLayoutZero()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<EmptyStruct>();
            Assert.AreEqual(0, structLayout.Count());
        }

        [Test]
        public void GetFieldsLayoutByteEnumStruct()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<EnumStruct>();
            Assert.AreEqual(1, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(1, structLayout[0].Size);
            Assert.AreEqual(typeof(ByteEnum), structLayout[0].FieldInfo.FieldType);
        }

        [Test]
        public void UnionStruct() => AssertNonRecursive<UnionStruct>();

        [Test]
        public void UnionStructPadding() => AssertNonRecursiveWithPadding<UnionStruct>();

        [Test]
        public void ExplicitLayoutOverlapStruct() => AssertNonRecursive<ExplicitLayoutOverlapStruct>();

        [Test]
        public void ExplicitLayoutOverlapStructPadding() => AssertNonRecursiveWithPadding<ExplicitLayoutOverlapStruct>();

        [Test]
        public void ExplicitLayoutNoOverlapStruct() => AssertNonRecursive<ExplicitLayoutNoOverlapStruct>();

        [Test]
        public void ExplicitLayoutNoOverlapStructPadding() => AssertNonRecursiveWithPadding<ExplicitLayoutNoOverlapStruct>();

        [Test]
        public void BoolBoolStruct() => AssertNonRecursiveWithPadding<BoolBoolStruct>();
    }
}