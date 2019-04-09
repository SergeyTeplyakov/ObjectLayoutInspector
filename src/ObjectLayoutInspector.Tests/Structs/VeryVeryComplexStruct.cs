namespace ObjectLayoutInspector.Tests
{
    public unsafe struct VeryVeryComplexStruct
    {
        public StructWithExplicitLayout one;
        public DoubleByteStructByteStruct two;
        public ComplexFixedStruct three;
        public ComplexStruct four;

        public WithNullableBoolBoolStructStructStruct? theNullable;
    }
}