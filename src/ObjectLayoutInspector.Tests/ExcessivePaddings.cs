using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    // Uncomment the following line to see the difference.
//#define UseAlias

#if UseAlias
    using ByteWrapper = System.Byte;
#else
    
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

        internal struct ByteWrapper
        {
            public byte b;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class StructWithAutomaticLayout
        {
            public ByteWrapper bw1;
            public ByteWrapper bw2;
            public ByteWrapper bw3;
        }

        internal struct Slot<T>
        {
            internal int hashCode;      // Lower 31 bits of hash code, -1 if unused
            internal T value;
            internal int next;          // Index of next entry, -1 if last
        }

        [Test]
        public void PrintClassMultipleByteWrappersLayout()
        {
            // In this case every field aligned on the pointer boundaries
            TypeLayout.PrintLayout<Slot<long>>();
            //Assert.That(TypeLayout.GetLayout<Slot>().Size, Is.EqualTo(24));
        }
    }
}