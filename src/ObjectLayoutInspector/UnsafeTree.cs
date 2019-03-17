using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObjectLayoutInspector.Helpers;

namespace ObjectLayoutInspector
{
    internal enum NodeKind : byte
    {
        Primitive,
        Complex,

        Nullable,

        Root,

        Fixed,

        Reference
    }

    // could make usual object or boxed nodes, but eating own dog food for fun and performance(fun)
    [StructLayout(LayoutKind.Explicit)]
    internal struct FieldNode
    {
        public static FieldNode[] GetFieldNodes(Type type)
        {
            return ReflectionHelper.GetInstanceFields(type).Select(x =>
            {
                int length = 0;
                return
                    x.FieldType.IsClass
                    ? new FieldNode { kind = NodeKind.Reference, ReferenceNode = new ReferenceNode(x) }
                    : Detectors.IsPrimitive(x)
                    ? new FieldNode { kind = NodeKind.Primitive, PrimitiveNode = new PrimitiveNode { info = x } }
                    : Detectors.IsNullable(x)
                    ? new FieldNode { kind = NodeKind.Nullable, NullableNode = new NullableNode { info = x } }
                    : Detectors.IsFixed(x, out length)
                    ? new FieldNode { kind = NodeKind.Fixed, FixedNode = new FixedNode { info = x, length = length } }
                    : new FieldNode { kind = NodeKind.Complex, ComplexNode = new ComplexNode { info = x } };
            }).ToArray();
        }

        [FieldOffset(0)]
        public NodeKind kind;

        [FieldOffset(8)]
        public int size;

        [FieldOffset(12)]
        public int totalOffset;

        [FieldOffset(24)]
        public FieldInfo info;

        [FieldOffset(8)]
        public PrimitiveNode PrimitiveNode;
        [FieldOffset(8)]
        public ComplexNode ComplexNode;
        [FieldOffset(8)]
        public RootNode RootNode;
        [FieldOffset(8)]
        public NullableNode NullableNode;
        [FieldOffset(8)]
        public FixedNode FixedNode;
        [FieldOffset(8)]
        public ReferenceNode ReferenceNode;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ReferenceNode
    {
        public ReferenceNode(FieldInfo info)
        {
            size = IntPtr.Size;
            this.info = info;
            totalOffset = -1;
        }

        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct NullableNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        // TODO: think of more typed modeling of hasValue field and value field (which may me primitive or complex, but not fixed or reference or nullable)

        [FieldOffset(24)]
        public FieldNode[] children;

        public Type Type => info.FieldType;
    }


    [StructLayout(LayoutKind.Explicit)]
    internal struct FixedNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(8)]
        public int length;

        [FieldOffset(16)]
        public FieldInfo info;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct PrimitiveNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ComplexNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        [FieldOffset(24)]
        public FieldNode[] children;

        public Type Type => info.FieldType;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct RootNode
    {
        [FieldOffset(0)]
        public int size;

        [FieldOffset(4)]
        public int totalOffset;

        [FieldOffset(16)]
        public FieldInfo info;

        [FieldOffset(24)]
        public FieldNode[] children;

        public bool IsPrimitive => children == null;
    }
}