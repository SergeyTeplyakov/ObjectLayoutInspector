using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    public abstract class TestsBase
    {
        protected static void AssertNonRecursiveWithPadding<T>() where T : struct
        {
            TypeLayout.PrintLayout<T>();
            var structLayout = UnsafeLayout.GetLayout<T>(recursive: false);
            var typeLayout = TypeLayout.GetLayout<T>(includePaddings: true);
            Assert.AreEqual(Unsafe.SizeOf<T>(), typeLayout.Size);
            CollectionAssert.AreEquivalent(typeLayout.Fields, structLayout);
        }

        protected static void AssertNonRecursive<T>() where T : struct
        {
            TypeLayout.PrintLayout<T>();
            var structLayout = UnsafeLayout.GetFieldsLayout<T>(recursive: false);
            var typeLayout = TypeLayout.GetLayout<T>(includePaddings: false);
            Assert.AreEqual(Unsafe.SizeOf<T>(), typeLayout.Size);
            CollectionAssert.AreEquivalent(typeLayout.Fields, structLayout);
        }        
    }
}