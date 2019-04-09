using System.Linq;
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
    public class ExcessivePaddings : TestsBase
    {
        internal struct StructMultipleByteWrappers
        {
            public ByteWrapper bw1;
            public ByteWrapper bw2;
            public ByteWrapper bw3;

            public StructMultipleByteWrappers(ByteWrapper bw1, ByteWrapper bw2, ByteWrapper bw3) => (this.bw1, this.bw2, this.bw3) = (bw1, bw2, bw3);
        }

        [Test]
        public void PrintStructMultipleByteWrappersLayout()
        {
            AssertNonRecursiveWithPadding<StructMultipleByteWrappers>();

            // If the layout is sequential, then structs are aligned properly with no paddings
            TypeLayout.PrintLayout<StructMultipleByteWrappers>();
        }

        internal struct ByteWrapper
        {
            public byte b;

            public ByteWrapper(byte b) => this.b = b;
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
            public int hashCode;      // Lower 31 bits of hash code, -1 if unused
            public T value;
            public int next;          // Index of next entry, -1 if last

            public Slot(int hashCode, T value, int next) => (this.hashCode, this.value, this.next) = (hashCode, value, next);
        }

        [Test]
        public void PrintClassMultipleByteWrappersLayout()
        {
            AssertNonRecursiveWithPadding<Slot<long>>();
            // In this case every field aligned on the pointer boundaries
            TypeLayout.PrintLayout<Slot<long>>();
            //Assert.That(TypeLayout.GetLayout<Slot>().Size, Is.EqualTo(24));
        }
    }
}