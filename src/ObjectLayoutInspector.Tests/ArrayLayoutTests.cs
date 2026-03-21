using System;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    [TestFixture]
    public class ArrayLayoutTests
    {
        [Test]
        public void IntArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<int>(10);

            Assert.AreEqual(typeof(int), layout.ElementType);
            Assert.AreEqual(10, layout.Length);
            Assert.AreEqual(4, layout.ElementSize);
            Assert.AreEqual(2 * IntPtr.Size, layout.HeaderSize);
            Assert.AreEqual(4, layout.LengthFieldSize);
        }

        [Test]
        public void ByteArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<byte>(5);

            Assert.AreEqual(typeof(byte), layout.ElementType);
            Assert.AreEqual(5, layout.Length);
            Assert.AreEqual(1, layout.ElementSize);
        }

        [Test]
        public void ObjectArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<object>(3);

            Assert.AreEqual(typeof(object), layout.ElementType);
            Assert.AreEqual(3, layout.Length);
            Assert.AreEqual(IntPtr.Size, layout.ElementSize);
        }

        [Test]
        public void StringArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<string>(4);

            Assert.AreEqual(typeof(string), layout.ElementType);
            Assert.AreEqual(4, layout.Length);
            Assert.AreEqual(IntPtr.Size, layout.ElementSize);
        }

        [Test]
        public void EmptyArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<int>(0);

            Assert.AreEqual(0, layout.Length);
            // Header + Length field, rounded to ptr boundary
            Assert.AreEqual(3 * IntPtr.Size, layout.TotalSize);
        }

        [Test]
        public void TotalSizeIsAlignedToPointerBoundary()
        {
            // byte[1]: header (16) + length (4) + length padding (4) + 1 byte = 25 → rounded to 32
            var layout = ArrayLayout.GetLayout<byte>(1);
            Assert.AreEqual(0, layout.TotalSize % IntPtr.Size);
        }

        [Test]
        public void StructArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<NotAlignedStruct>(2);

            Assert.AreEqual(typeof(NotAlignedStruct), layout.ElementType);
            Assert.AreEqual(2, layout.Length);
            Assert.That(layout.ElementSize, Is.GreaterThan(0));
        }

        [Test]
        public void NegativeLengthThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ArrayLayout.GetLayout<int>(-1));
        }

        [Test]
        public void PrintLayoutDoesNotThrow()
        {
            Assert.DoesNotThrow(() => ArrayLayout.PrintLayout<int>(5));
        }

        [Test]
        public void PrintLayoutObjectArrayDoesNotThrow()
        {
            Assert.DoesNotThrow(() => ArrayLayout.PrintLayout<object>(3));
        }

        [Test]
        public void ToStringContainsTypeName()
        {
            var layout = ArrayLayout.GetLayout<int>(10);
            var str = layout.ToString();
            Assert.That(str, Does.Contain("Int32[10]"));
            Assert.That(str, Does.Contain("Element size: 4 bytes"));
        }

        [Test]
        public void ToStringContainsHeaderInfo()
        {
            var layout = ArrayLayout.GetLayout<double>(5);
            var str = layout.ToString();
            Assert.That(str, Does.Contain("Object Header"));
            Assert.That(str, Does.Contain("Method Table Ptr"));
            Assert.That(str, Does.Contain("Length"));
        }

        [Test]
        public void TypeLayoutPrintLayout_IntArray_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TypeLayout.PrintLayout<int[]>());
        }

        [Test]
        public void TypeLayoutPrintLayout_ObjectArray_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TypeLayout.PrintLayout<object[]>());
        }

        [Test]
        public void TypeLayoutPrintLayout_StringArray_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TypeLayout.PrintLayout<string[]>());
        }

        [Test]
        public void PointRefArrayLayout()
        {
            var layout = ArrayLayout.GetLayout<PointRef>(3);

            Assert.AreEqual(typeof(PointRef), layout.ElementType);
            Assert.AreEqual(3, layout.Length);
            Assert.AreEqual(IntPtr.Size, layout.ElementSize);
        }

        [Test]
        public void PointRefArrayWithElementLayout()
        {
            var layout = ArrayLayout.GetLayout<PointRef>(3);
            var str = layout.ToString(showElementLayout: true);

            Console.WriteLine(str);

            // Array part
            Assert.That(str, Does.Contain("PointRef (reference)"));
            Assert.That(str, Does.Contain("[0]"));
            Assert.That(str, Does.Contain("[1]"));
            Assert.That(str, Does.Contain("[2]"));

            // Element layout at the bottom
            Assert.That(str, Does.Contain("Element layout:"));
            Assert.That(str, Does.Contain("Int32 X"));
            Assert.That(str, Does.Contain("Int32 Y"));
        }

        [Test]
        public void TypeLayoutPrintLayout_PointRefArray_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TypeLayout.PrintLayout<PointRef[]>());
        }
    }
}
