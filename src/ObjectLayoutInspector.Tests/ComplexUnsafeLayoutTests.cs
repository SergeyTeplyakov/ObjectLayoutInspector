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
    public class ComplexUnsafeLayoutTests : TestsBase
    {
        // TODO: uncomment tests with fix for unsafe paddings according on current running architecure-runtime (what paper to read)?
        // field offsets are correct (values with actual bits)
        // out put is different. we should count padding in the end of first struct as part of it? 
        // Unsafe can support padding in the end, but not in the end and state (doubt this happens)
        // possible fixes - create property tree in UnsafeLayout code to handle siblings 
        // or just hack empty space for non recursive case after wards?
        // need to think how to fix. is padding at all part of FIRST structure?

        //[Test]
        public void VeryVeryComplexStruct() => AssertNonRecursive<VeryVeryComplexStruct>();

        //[Test]
        public void ExplicitHolderOfSequentialIntObjectStruct() => AssertNonRecursiveWithPadding<ExplicitHolderOfSequentialIntObjectStruct>();

        //[Test]
        public void ComplexStruct() => AssertNonRecursive<ComplexStruct>();


        [Test]
        public void VeryVeryComplexStructFields()
        {
            var structLayout = UnsafeLayout.GetFieldsLayout<VeryVeryComplexStruct>(recursive:true);
            var six = structLayout[5];
            Assert.AreEqual(16, six.Offset);
            Assert.AreEqual(8, six.Size);
            Assert.AreEqual(typeof(double), six.FieldInfo.FieldType);
            Assert.AreEqual(2, structLayout.Where(x=> x.FieldInfo.FieldType == (typeof(double))).Count());
        }

        [Test]
        public void MASTER_STRUCT() => AssertNonRecursiveWithPadding<MASTER_STRUCT>();

        [Test]
        public void MASTER_STRUCTFieldsRecursive()
        {
            var structLayout = UnsafeLayout.GetLayout<MASTER_STRUCT>(recursive: true, new List<Type> { typeof(Guid) });
            Assert.AreEqual(7, structLayout.Count());
        }

        [Test]
        public void STRUCT1() => AssertNonRecursiveWithPadding<STRUCT1>();

        [Test]
        public void STRUCT2() => AssertNonRecursiveWithPadding<STRUCT2>();

        [Test]
        public void MASTER_STRUCT_UNION() => AssertNonRecursiveWithPadding<MASTER_STRUCT_UNION>();

        [Test]
        public void FieldNode() => AssertNonRecursive<FieldNode>();

        [Test]
        public void ExplicitHolderOfSequentialIntObjectStructFields()
        {
            var layout = UnsafeLayout.GetFieldsLayout<ExplicitHolderOfSequentialIntObjectStruct>();
            Assert.AreEqual(2, layout.Count);
            Assert.AreEqual(0, layout[0].Offset);
            Assert.AreEqual(8, layout[0].Size);
            Assert.AreEqual(typeof(object), layout[0].FieldInfo.FieldType);
            Assert.AreEqual(8, layout[1].Offset);
            Assert.AreEqual(4, layout[1].Size);
            Assert.AreEqual(typeof(int), layout[1].FieldInfo.FieldType);
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
    }
}