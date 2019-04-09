namespace ObjectLayoutInspector.Tests
{
    public struct WithNestedUnsafeUnsafeStruct
    {
        public WithNestedUnsafeStruct fb;

        public WithNestedUnsafeUnsafeStruct(WithNestedUnsafeStruct fb) => this.fb = fb;
    }    
}