using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectLayoutInspector.Tests
{
    public abstract class TestsBase
    {
        protected void AssertNonRecursiveWithPadding<T>() where T : struct
        {
            TypeLayout.PrintLayout<T>();
            var structLayout = UnsafeLayout.GetLayout<T>(recursive: false);
            var typeLayout = TypeLayout.GetLayout<T>(includePaddings: true);
            CollectionAssert.AreEquivalent(typeLayout.Fields, structLayout);
            Assert.AreEqual(Unsafe.SizeOf<T>(), typeLayout.Size);
        }

        protected void AssertNonRecursive<T>() where T : struct
        {
            TypeLayout.PrintLayout<T>();
            var structLayout = UnsafeLayout.GetFieldsLayout<T>(recursive: false);
            var typeLayout = TypeLayout.GetLayout<T>(includePaddings: false);
            CollectionAssert.AreEquivalent(typeLayout.Fields, structLayout);
            Assert.AreEqual(Unsafe.SizeOf<T>(), typeLayout.Size);
        }        
    }
}