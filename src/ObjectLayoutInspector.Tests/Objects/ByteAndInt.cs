namespace ObjectLayoutInspector.Tests.Objects
{
    class ByteAndInt
    {
        public byte b { get; }
        public int n;

        public ByteAndInt(byte b, int n) => (this.b, this.n) = (b, n);
    }
}