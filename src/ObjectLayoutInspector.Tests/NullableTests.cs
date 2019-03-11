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
    }
}