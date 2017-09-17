using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    //[StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct FullSymbol
    {
        public byte Number;
    }

    public class Expression
    {
        public FullSymbol Fs;
        public int number;
        public int anotherNumber;
    }

    public class Node
    {
        public LineInfo LineInfo;
    }

    public struct LineInfo
    {
        // 8
        object ptr;
        // 4
        int number1;
        // 4
        int number2;
    }

    //[StructLayout(LayoutKind.Auto, Pack = 1)]
    public class DerivedExpression //: Node
    {
        // 40
        // 16 - header
        // 16 - base
        // 4 - number
        // 4 - padding
        // 4 - Fs
        // 4 - padding
        public FullSymbol FS;
        public FullSymbol FS2;
        //public int number;
        //public int number2;
    }
}