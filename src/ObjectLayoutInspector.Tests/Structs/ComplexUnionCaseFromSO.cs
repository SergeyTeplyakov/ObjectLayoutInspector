using System;
using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{

    [StructLayout(LayoutKind.Sequential)]
    internal struct STRUCT1
    {
        public Guid guid;

        public String str1;
        public String str2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct STRUCT2
    {
        public Guid guid;

        public String str1;
        public String str2;

        public Int32 i1;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MASTER_STRUCT_UNION
    {
        [FieldOffset(0)]
        public STRUCT1 Struct1;

        [FieldOffset(0)]
        public STRUCT2 Struct2;
    }

    //https://stackoverflow.com/questions/14024483/unions-in-c-structure-members-do-not-seem-to-be-aligned
    [StructLayout(LayoutKind.Sequential)]
    internal struct MASTER_STRUCT
    {
        public MASTER_STRUCT_UNION Union;
    }
}