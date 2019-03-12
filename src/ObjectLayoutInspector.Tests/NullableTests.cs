using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{

    [TestFixture]
    public class NullableTests
    {
        [Test]
        public void Nullable()
        {
            TypeLayout.PrintLayout<Nullable<bool>>();
            var typeLayout = TypeLayout.GetLayout<Nullable<bool>>(includePaddings: true);
            var size = Unsafe.SizeOf<Nullable<bool>>();
            Assert.AreEqual(typeLayout.Size, size);
        }

        [Test]
        public void NullableLongByteStruct()
        {
            // why cannot use `public struct Nullable<T> where T : struct`?
            // error CS0453: The type 'int?' must be a non-nullable value type in order to use it as parameter 'T' in the generic type or method 'UnsafeLayout.GetLayout<T>(bool)'
            // var structLayout = UnsafeLayout.GetLayout<Nullable<bool>>();
            TypeLayout.PrintLayout<Nullable<LongByteStruct>>();
            var typeLayout = TypeLayout.GetLayout<Nullable<LongByteStruct>>(includePaddings: true);
            var size = Unsafe.SizeOf<Nullable<LongByteStruct>>();
            Assert.AreEqual(typeLayout.Size, size);
        }        

        [Test]
        public void WithNullableIntStruct()
        {
            TypeLayout.PrintLayout<WithNullableIntStruct>();
            var typeLayout = TypeLayout.GetLayout<WithNullableIntStruct>(includePaddings: true);
            var unsafeLayout = UnsafeLayout.GetLayout<WithNullableIntStruct>(recursive:false);
            Assert.AreEqual(typeLayout.Fields.Count(), unsafeLayout.Count());
            Assert.AreEqual(typeLayout.Fields[0].Offset, unsafeLayout[0].Offset);
            Assert.AreEqual(typeLayout.Fields[0].Size, unsafeLayout[0].Size);
            var size = Unsafe.SizeOf<WithNullableIntStruct>();
            Assert.AreEqual(typeLayout.Size, size);
        }

        // TODO: fix UnsafeLayout
        //[Test] 
        public void WithNullableIntNullableBoolIntStructField()
        {
            TypeLayout.PrintLayout<WithNullableIntNullableBoolIntStruct>();
            var typeLayout = TypeLayout.GetLayout<WithNullableIntNullableBoolIntStruct>(includePaddings: false);
            var unsafeLayout = UnsafeLayout.GetFieldsLayout<WithNullableIntNullableBoolIntStruct>(recursive:false);
            Assert.AreEqual(typeLayout.Fields.Count(), unsafeLayout.Count());
            Assert.AreEqual(typeLayout.Fields[0].Offset, unsafeLayout[0].Offset);
            Assert.AreEqual(typeLayout.Fields[0].Size, unsafeLayout[0].Size);
            var size = Unsafe.SizeOf<WithNullableIntNullableBoolIntStruct>();
            Assert.AreEqual(typeLayout.Size, size);
        }    

        [Test]
        public void WithNullableBoolBoolStructStruct()
        {
            TypeLayout.PrintLayout<WithNullableBoolBoolStructStruct>();
            var typeLayout = TypeLayout.GetLayout<WithNullableBoolBoolStructStruct>(includePaddings: false);
            var unsafeLayout = UnsafeLayout.GetFieldsLayout<WithNullableBoolBoolStructStruct>(recursive:false);
            Assert.AreEqual(typeLayout.Fields.Count(), unsafeLayout.Count());
            Assert.AreEqual(typeLayout.Fields[0].Offset, unsafeLayout[0].Offset);
            Assert.AreEqual(typeLayout.Fields[0].Size, unsafeLayout[0].Size);
            var size = Unsafe.SizeOf<WithNullableBoolBoolStructStruct>();
            Assert.AreEqual(typeLayout.Size, size);
        }            

        // TODO: fix UnsafeLayout
        //[Test] 
        public void WithNullableBoolBoolStructStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableBoolBoolStructStruct>();
            var unsafeLayout = UnsafeLayout.GetLayout<WithNullableBoolBoolStructStruct>(recursive:true);
            Assert.AreEqual(3, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
        } 

        // TODO: fix UnsafeLayout
        //[Test] 
        public void WithNullableIntStructRecursive()
        {
            TypeLayout.PrintLayout<WithNullableIntStruct>();
            var unsafeLayout = UnsafeLayout.GetLayout<WithNullableIntStruct>(recursive:true);
            Assert.AreEqual(2, unsafeLayout.Count());
            Assert.AreEqual(0, unsafeLayout[0].Offset);
            Assert.AreEqual(1, unsafeLayout[0].Size);
        }        
    }
}