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
            TypeLayout.PrintLayout<WithNullableIntStruct>();
            var typeLayout = TypeLayout.GetLayout<WithNullableIntStruct>(includePaddings: true);
            // why cannot use `public struct Nullable<T> where T : struct`?
            // error CS0453: The type 'int?' must be a non-nullable value type in order to use it as parameter 'T' in the generic type or method 'UnsafeLayout.GetLayout<T>(bool)'
            // var structLayout = UnsafeLayout.GetLayout<Nullable<int>>();
            //
            // BUG:  System.NotSupportedException : Specified method is not supported. fix it
            //var unsafeLayout = UnsafeLayout.GetLayout<WithNullableIntStruct>();
            var size = Unsafe.SizeOf<WithNullableIntStruct>();
            Assert.AreEqual(typeLayout.Size, size);
        }
    }
}