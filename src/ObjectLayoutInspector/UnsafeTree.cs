using System;
using System.Reflection;
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
        public static (FieldNode[] fields, Type[] types) GetFieldNodes(Type type)
        {
            var (fields, types) = ReflectionHelper.GetInstanceFields(type);

            var fieldNodes = new FieldNode[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var x = fields[i];
                if (x.FieldType.IsClass)
                    fieldNodes[i] = new FieldNode { kind = NodeKind.Reference, referenceNode = new ReferenceNode(x) };
                else if (Detectors.IsPrimitive(x))
                    fieldNodes[i] = new FieldNode { kind = NodeKind.Primitive, primitiveNode = new PrimitiveNode { info = x } };
                else if (Detectors.IsNullable(x))
                    fieldNodes[i] = new FieldNode { kind = NodeKind.Nullable, nullableNode = new NullableNode { info = x } };
                else if (Detectors.IsFixed(x, out int length))
                    fieldNodes[i] = new FieldNode { kind = NodeKind.Fixed, fixedNode = new FixedNode { info = x, length = length } };
                else
                    fieldNodes[i] = new FieldNode { kind = NodeKind.Complex, complexNode = new ComplexNode { info = x } };
            }

            return (fieldNodes, types);
        }

        [FieldOffset(0)]
        public NodeKind kind;

        [FieldOffset(8)]
        public int size;

        [FieldOffset(8)]
        public PrimitiveNode primitiveNode;

        [FieldOffset(8)]
        public ComplexNode complexNode;

        [FieldOffset(8)]
        public RootNode rootNode;

        [FieldOffset(8)]
        public NullableNode nullableNode;

        [FieldOffset(8)]
        public FixedNode fixedNode;

        [FieldOffset(8)]
        public ReferenceNode referenceNode;

        [FieldOffset(12)]
        public int totalOffset;

        [FieldOffset(24)]
        public FieldInfo info;

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

        // Nullable can be only of Complex or Primitive, so may model FieldNode as remain part only of 2
        // validate of same size and field layout it test and Unsafe.As map to reinterpret cast
        /// <summary>
        /// Boolean.
        /// </summary>
        [FieldOffset(24)]
        public Ref<FieldNode> hasValue;

        [FieldOffset(32)]
        public Ref<FieldNode> value;

        public Type Type => info.FieldType;
    }

    internal class Ref<T> where T : struct
    {
        public T value;

        public Ref(T value)
        {
            this.value = value;
        }
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

        public bool IsPrimitive => children is null;
    }
}