namespace ObjectLayoutInspector.Tests
{
    public struct WithNullableIntIntStruct
    {
        public int? theNullableInt;
        public int theInt;

        public WithNullableIntIntStruct(int? theNullableInt, int theInt) => (this.theNullableInt, this.theInt) = (theNullableInt, theInt);
    }

    public struct WithNullableIntNullableBoolIntStruct
    {
        public int? one;
        public bool? two;
        public int three;

        public WithNullableIntNullableBoolIntStruct(int? one, bool? two, int three) => (this.one, this.two, this.three) = (one, two, three);
    }    
}