namespace ObjectLayoutInspector.Tests
{
    public struct GenericStruct<T> where T: struct
    {
        public bool one;
        public T two;
    }
}