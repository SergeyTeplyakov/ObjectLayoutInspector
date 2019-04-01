namespace ObjectLayoutInspector.Tests
{
    public unsafe struct ComplexFixedStruct
    {
        public const int MaxLength = 13;
        private fixed short _bytes[MaxLength];
        private FixedBytes fb;

        bool three;
    }
}