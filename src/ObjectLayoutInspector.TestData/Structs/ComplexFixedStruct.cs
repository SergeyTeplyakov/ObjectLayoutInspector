namespace ObjectLayoutInspector.Tests
{
    public unsafe struct ComplexFixedStruct
    {
        public const int MaxLength = 13;
        public fixed short _bytes[MaxLength];
        public FixedBytes fb;

        public bool three;
    }
}