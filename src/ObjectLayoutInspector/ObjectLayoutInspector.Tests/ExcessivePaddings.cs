using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
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
            TypeLayout.PrintLayout<StructMultipleByteWrappers>();
        }

        internal struct ByteWrapper
        {
            public byte b;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class ClassMultipleByteWrappers
        {
            public object o;
            public ByteWrapper bw1;
            public ByteWrapper bw2;
            public ByteWrapper bw3;
        }

        [Test]
        public void PrintClassMultipleByteWrappersLayout()
        {
            TypeLayout.PrintLayout<ClassMultipleByteWrappers>();
        }
    }
}