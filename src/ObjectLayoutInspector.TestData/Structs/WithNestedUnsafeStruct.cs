namespace ObjectLayoutInspector.Tests
{
    public struct WithNestedUnsafeStruct
    {
        public FixedBytes fb;

        public WithNestedUnsafeStruct(FixedBytes fb) => this.fb = fb;
    }
}