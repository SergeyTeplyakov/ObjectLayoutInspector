using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    // Uncomment the following line to see the difference.
//#define UseAlias

#if UseAlias
    using ByteWrapper = System.Byte;
#else
    internal struct ByteWrapper
    {
        public byte b;
    }
#endif

    [TestFixture]
    public class ExcessivePaddings
    {
        internal struct StructMultipleByteWrappers
        {
            public ByteWrapper bw1;
            public ByteWrapper bw2;
            public ByteWrapper bw3;
        }

        [Test]
        public void PrintStructMultipleByteWrappersLayout()
        {
            // If the layout is sequential, then structs are aligned properly with no paddings
            TypeLayout.PrintLayout<StructMultipleByteWrappers>();
            Assert.That(TypeLayout.GetLayout<StructMultipleByteWrappers>().Size, Is.EqualTo(3));
        }

        [StructLayout(LayoutKind.Auto)]
        internal class StructWithAutomaticLayout
        {
            public ByteWrapper bw1;
            public ByteWrapper bw2;
            public ByteWrapper bw3;
        }

        [Test]
        public void PrintClassMultipleByteWrappersLayout()
        {
            // In this case every field aligned on the pointer boundaries
            TypeLayout.PrintLayout<StructWithAutomaticLayout>();
            Assert.That(TypeLayout.GetLayout<StructWithAutomaticLayout>().Size, Is.EqualTo(24));
        }
    }
}