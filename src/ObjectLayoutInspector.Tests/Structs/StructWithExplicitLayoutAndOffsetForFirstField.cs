﻿using System.Runtime.InteropServices;

namespace ObjectLayoutInspector.Tests
{
    [StructLayout(LayoutKind.Explicit, Size = 100)]
    public struct StructWithExplicitLayoutAndOffsetForFirstField
    {
        [FieldOffset(10)]
        public byte m_byte1;
        [FieldOffset(11)]
        public byte m_byte1_1;
    }
}
