namespace ObjectLayoutInspector.Tests
{
    unsafe struct ComplexFixedStruct
    {
        public const int MaxLength = 13;
        private fixed byte _bytes[MaxLength];
        private FixedBytes fb;

        bool three;
    }
}