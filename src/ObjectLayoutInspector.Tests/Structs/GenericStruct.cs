namespace ObjectLayoutInspector.Tests.Structs
{
    public struct GenericStruct<T> where T : struct
    {
        public bool one;
        public T two;
    }
}