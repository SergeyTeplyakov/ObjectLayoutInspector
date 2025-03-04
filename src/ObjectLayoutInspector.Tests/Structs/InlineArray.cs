#if NET8_0_OR_GREATER
namespace ObjectLayoutInspector.Tests.Structs
{
    [System.Runtime.CompilerServices.InlineArray(10)]
    struct Inline10<T>
    {
        public T item;
    }

    [System.Runtime.CompilerServices.InlineArray(1)]
    struct Inline1<T>
    {
        public T item;
    }
}
#endif
