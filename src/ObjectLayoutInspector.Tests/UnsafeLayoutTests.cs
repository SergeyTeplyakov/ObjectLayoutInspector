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

        // TODO: BUG: fix for unsafe
        //[Test]
        public void VeryVeryComplexStruct() => AssertNonRecursive<VeryVeryComplexStruct>();

        // TODO: BUG: fix for unsafe
        //[Test]
        public void ComplexStruct()
        {
            // TODO: out put is different. we should count padding in the end of first struct as part of it? 
            // Unsafe can support padding in the end, but not in the end and state (doubt this happens)
            // possible fixes - crearte propert tree in UnsafeLayout code to handle siblings 
            // or just hack empty space for non recursive case after wards?
            // need to think how to fix. is padding at all part of FIRST structure?
            // Standard Output Messages:
            //  Type layout for 'ComplexStruct'
            //  Size: 24 bytes. Paddings: 7 bytes (%29 of empty space)
            //  |=========================================|
            //  |  0-15: DoubleFloatStruct one (16 bytes) |
            //  | |=============================|         |
            //  | |   0-7: Double one (8 bytes) |         |
            //  | |-----------------------------|         |
            //  | |  8-11: Single two (4 bytes) |         |
            //  | |-----------------------------|         |
            //  | | 12-15: padding (4 bytes)    |         |
            //  | |=============================|         |
            //  |-----------------------------------------|
            //  | 16-23: EnumIntStruct two (8 bytes)      |
            //  | |==============================|        |
            //  | |     0: ByteEnum one (1 byte) |        |
            //  | |------------------------------|        |
            //  | |   1-3: padding (3 bytes)     |        |
            //  | |------------------------------|        |
            //  | |   4-7: Int32 two (4 bytes)   |        |
            //  | |==============================|        |
            //  |=========================================|
            AssertNonRecursive<ComplexStruct>();
        }
        
        [Test]
        public void GetFieldsLayoutStructsWorkFineAndThisIsMostImportantForSerialization()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<ComplexStruct>();
            Assert.AreEqual(4, structLayout.Count());
            Assert.AreEqual(0, structLayout[0].Offset);
            Assert.AreEqual(8, structLayout[0].Size);
            Assert.AreEqual(typeof(double), structLayout[0].FieldInfo.FieldType);
            Assert.AreEqual(8, structLayout[1].Offset);
            Assert.AreEqual(4, structLayout[1].Size);
            Assert.AreEqual(typeof(Single), structLayout[1].FieldInfo.FieldType);
            Assert.AreEqual(16, structLayout[2].Offset);
            Assert.AreEqual(1, structLayout[2].Size);
            Assert.AreEqual(typeof(ByteEnum), structLayout[2].FieldInfo.FieldType);
            Assert.AreEqual(20, structLayout[3].Offset);
            Assert.AreEqual(4, structLayout[3].Size);
            Assert.AreEqual(typeof(int), structLayout[3].FieldInfo.FieldType);
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
            Assert.AreEqual(0, structLayout.Count());
        }

        [Test]
        public void GetFieldsLayoutByte()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<byte>();
            Assert.AreEqual(0, structLayout.Count());
        }

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

        [Test]
        public void MASTER_STRUCT() => AssertNonRecursiveWithPadding<MASTER_STRUCT>();

        [Test]
        public void MASTER_STRUCTFieldsRecursive()
        {
            var structLayout = UnsafeLayout.GetLayout<MASTER_STRUCT>(recursive: true, new List<Type> { typeof(Guid) });
            Assert.AreEqual(6, structLayout.Count());
        }
    }
}