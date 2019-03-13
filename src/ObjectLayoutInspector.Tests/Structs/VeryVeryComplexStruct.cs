using System.Numerics;

namespace ObjectLayoutInspector.Tests
{
    public unsafe struct VeryVeryComplexStruct
    {
        StructWithExplicitLayout one;
        DoubleByteStructByteStruct two;
        ComplexFixedStruct three;
        ComplexStruct four;
    }
}