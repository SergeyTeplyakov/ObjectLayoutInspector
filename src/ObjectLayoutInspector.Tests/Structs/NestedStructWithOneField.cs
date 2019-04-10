namespace ObjectLayoutInspector.Tests
{
    public struct NestedStructWithOneField
    {
        public WithOneField Field1;
        public WithTwoFields Field2;
    }

    public struct WithOneField
    {
        public int Value;
    }

    public struct WithTwoFields
    {
        public int Value1;
        public int Value2;
    }
}