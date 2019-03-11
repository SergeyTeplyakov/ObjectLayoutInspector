namespace ObjectLayoutInspector.Tests
{
    unsafe struct FixedBytes
    {
        public const int MaxLength = 33;
        private fixed byte _bytes[MaxLength];
    }
}