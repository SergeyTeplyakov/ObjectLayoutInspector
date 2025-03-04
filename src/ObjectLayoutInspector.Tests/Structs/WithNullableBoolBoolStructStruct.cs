namespace ObjectLayoutInspector.Tests.Structs
{
    public struct WithNullableBoolBoolStructStruct
    {
        public BoolBoolStruct? one;

        public WithNullableBoolBoolStructStruct(BoolBoolStruct? one) => this.one = one;
    }
}