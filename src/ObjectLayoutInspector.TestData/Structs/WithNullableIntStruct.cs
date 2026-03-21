namespace ObjectLayoutInspector.Tests
{
    public struct WithNullableIntStruct
    {
        public int? one;

        public WithNullableIntStruct(int? one) => this.one = one;
    }
}