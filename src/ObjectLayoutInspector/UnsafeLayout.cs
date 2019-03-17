using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObjectLayoutInspector.Helpers;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Gets layout via <see cref="Unsafe"/> portable.  Works in Ahead of Time (AOT).
    /// Does not use code generation or CLR debugging mechanisms or runtime intrinsic.
    /// </summary>
    /// <seealso href="https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.GetFieldOffset.html">
    /// May create field provider for that platform
    /// </seealso>
    /// <seealso href="https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.PinGCObjectAndGetAddress.html">
    /// May work with heap classes there
    /// </seealso>
    /// <seealso href="https://stackoverflow.com/questions/18937935/how-to-mutate-a-boxed-struct-using-il">
    /// Could support classes but Marshal.SizeOf does works only with blittables, may be there is other ways to Unsafe into heap for classes
    /// </seealso>
    // TODO: does .NET runs of Big Endian? Test and fix it if yes.
    //
    // TODO: 
    // 1. replace recursion with loop-trampoline
    // 2. calculate all fields needed first and then create on big array for all stuff to run through it 
    // 3. then can write more efficient scan total algorithm (which hopefully will outlive longer than API provided by .NET Core in future)
    public static class UnsafeLayout
    {
        /// <summary>
        /// Get fields layout of <typeparamref name="T"/> with no padding information ordered by offset.
        /// </summary>
        /// <typeparam name="T">To to get structure of.</>
        public static IReadOnlyList<FieldLayout> GetFieldsLayout<T>(bool recursive = true, IReadOnlyCollection<Type> primitives = null)
            where T : struct
        {
            var tree = GetLayoutTree<T>();
            var fieldsLayout = GetFieldsLayoutInternal<T>(in tree, recursive, primitives);
            return fieldsLayout.OrderBy(x => x.Offset).ToList();
        }

        /// <summary>
        /// Get layout of <typeparamref name="T"/> with padding information ordered by offset.
        /// </summary>
        public static IReadOnlyList<FieldLayoutBase> GetLayout<T>(bool recursive = true, IReadOnlyCollection<Type> primitives = null, bool hierarchical = false)
            where T : struct
        {
            if (hierarchical)
            {
                throw new NotImplementedException("Could report recursive layout with complex fields in each other");
            }

            var tree = GetLayoutTree<T>();
            var fieldsLayout = GetFieldsLayoutInternal<T>(in tree, recursive, primitives);
            return AddPaddings<T>(fieldsLayout);
        }

        private static IReadOnlyList<FieldLayout> GetFieldsLayoutInternal<T>(in RootNode tree, bool recursive, IReadOnlyCollection<Type> primitives) where T : struct
        {
            var fieldsLayout = new List<FieldLayout>();
            if (tree.IsPrimitive)
            {
                fieldsLayout.Add(new FieldLayout(tree.totalOffset, tree.info, tree.size));
                return fieldsLayout;
            }
            else if (!recursive)
            {
                if (primitives != null && primitives.Contains(typeof(T)))
                {
                    fieldsLayout.Add(new FieldLayout(tree.totalOffset, tree.info, tree.size));
                    return fieldsLayout;
                }

                for (var i = 0; i < tree.children.Length; i++)
                {
                    fieldsLayout.Add(GetLayout(ref tree.children[i]));
                }

                return fieldsLayout;
            }
            else
            {
                if (primitives != null && primitives.Contains(typeof(T)))
                {
                    fieldsLayout.Add(new FieldLayout(tree.totalOffset, tree.info, tree.size));
                    return fieldsLayout;
                }

                for (var i = 0; i < tree.children.Length; i++)
                {
                    IsTerminal check = (n) => primitives != null ? primitives.Contains(n.Type) : false;
                    GetLayout(ref tree.children[i], fieldsLayout, check);
                }

                return fieldsLayout;
            }
        }

        delegate bool IsTerminal(FieldNode node);
        delegate void Twiddler<T>(ref T seed);

        private class NullableTwiddler
        {
            private int hasValueOffset;

            public NullableTwiddler(int hasValueOffset) => this.hasValueOffset = hasValueOffset;

            public void Twiddle<T>(ref T seed)
            {
                ref var hasValue = ref Unsafe.Add(ref Unsafe.As<T, byte>(ref seed), hasValueOffset);
                hasValue = 1;
            }
        }

        private static void GetLayout(
           ref FieldNode node,
           List<FieldLayout> layout,
           IsTerminal check)
        {
            switch (node.kind)
            {
                case NodeKind.Reference:
                    layout.Add(new FieldLayout(node.ReferenceNode.totalOffset, node.ReferenceNode.info, node.ReferenceNode.size));
                    break;
                case NodeKind.Primitive:
                    layout.Add(new FieldLayout(node.PrimitiveNode.totalOffset, node.PrimitiveNode.info, node.PrimitiveNode.size));
                    break;
                case NodeKind.Nullable:
                    if (check(node))
                    {
                        layout.Add(new FieldLayout(node.NullableNode.totalOffset, node.NullableNode.info, node.NullableNode.size));
                        break;
                    }

                    for (var i = 0; i < node.NullableNode.children.Length; i++)
                    {
                        GetLayout(ref node.NullableNode.children[i], layout, check);
                    }
                    break;

                case NodeKind.Fixed:
                    layout.Add(new FieldLayout(node.FixedNode.totalOffset, node.FixedNode.info, node.FixedNode.size));
                    break;
                case NodeKind.Complex:
                    if (check(node))
                    {
                        layout.Add(new FieldLayout(node.ComplexNode.totalOffset, node.ComplexNode.info, node.ComplexNode.size));
                        break;
                    }

                    for (var i = 0; i < node.ComplexNode.children.Length; i++)
                    {
                        GetLayout(ref node.ComplexNode.children[i], layout, check);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{node.kind} is not supported");
            }
        }

        private static FieldLayout GetLayout(
           ref FieldNode node)
        {
            switch (node.kind)
            {
                case NodeKind.Reference:
                    return new FieldLayout(node.ReferenceNode.totalOffset, node.ReferenceNode.info, node.ReferenceNode.size);
                case NodeKind.Primitive:
                    return new FieldLayout(node.PrimitiveNode.totalOffset, node.PrimitiveNode.info, node.PrimitiveNode.size);
                case NodeKind.Nullable:
                    return new FieldLayout(node.NullableNode.totalOffset, node.NullableNode.info, node.NullableNode.size);
                case NodeKind.Fixed:
                    return new FieldLayout(node.FixedNode.totalOffset, node.FixedNode.info, node.FixedNode.size);
                case NodeKind.Complex:
                    return new FieldLayout(node.ComplexNode.totalOffset, node.ComplexNode.info, node.ComplexNode.size);
                default:
                    throw new NotImplementedException($"{node.kind} is not supported");
            }
        }

        private static RootNode GetLayoutTree<T>() where T : struct
        {
            var type = typeof(T);
            var fields = FieldNode.GetFieldNodes(type);
            var root = new FieldNode { kind = NodeKind.Root, RootNode = new RootNode { children = fields, totalOffset = 0, size = Unsafe.SizeOf<T>() } };
            var previous = 0;
            GetLayout<T>(ref previous, ref root, new List<Func<object, object>>(), new List<Twiddler<T>> { null });

            return root.RootNode;
        }

        private static IReadOnlyList<FieldLayoutBase> AddPaddings<T>(IReadOnlyList<FieldLayout> fieldsLayout)
        {
            var fieldsAndPaddings = new List<FieldLayoutBase>(fieldsLayout.Count);
            var ordered = fieldsLayout.OrderBy(x => x.Offset).ToList();
            while (ordered.Count() > 0)
            {
                var head = ordered[0];
                var last = fieldsAndPaddings.LastOrDefault();
                var lastOffset = last?.Offset + last?.Size ?? 0;
                var paddingSize = head.Offset - lastOffset;
                if (paddingSize > 0)
                {
                    fieldsAndPaddings.Add(new Padding(lastOffset, paddingSize));
                }

                fieldsAndPaddings.Add(head);
                ordered.RemoveAt(0);
                while (ordered.Count() > 0)
                {
                    var second = ordered[0];
                    if (head.Offset + head.Size > second.Offset + second.Size)
                    {
                        fieldsAndPaddings.Add(second);
                        ordered.RemoveAt(0);
                    }
                    else if (head.Offset + head.Size > second.Offset)
                    {
                        fieldsAndPaddings.Add(second);
                        ordered.RemoveAt(0);
                        head = second;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var lastByte = fieldsAndPaddings.Max(x => x.Offset + x.Size);
            var ending = Unsafe.SizeOf<T>() - lastByte;
            if (ending > 0)
            {
                fieldsAndPaddings.Add(new Padding(lastByte, ending));
            }

            return fieldsAndPaddings;
        }

        private static void GetLayout<T>(
           ref int previous,
           ref FieldNode node,
           List<Func<object, object>> getterHierarchy,
           List<Twiddler<T>> twiddlerHierarchy
           )
           where T : struct
        {
            switch (node.kind)
            {
                case NodeKind.Reference:
                    FindReferenceOffset<T>(ref node.ReferenceNode, getterHierarchy);
                    break;
                case NodeKind.Primitive:
                    var pType = node.PrimitiveNode.Type;
                    Func<object, bool> primitiveEmptyComparator = x => Activator.CreateInstance(pType).Equals(x);
                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.Add(node.info.GetValue);
                    }

                    FindPrimitiveOffset<T>(ref node.PrimitiveNode, getterHierarchy, primitiveEmptyComparator, twiddlerHierarchy);

                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    }

                    break;
                case NodeKind.Nullable:
                    // Activator.CreateInstance creates null for nullable
                    // cannot FieldInfo.GetValue fields of Value and HasValue as System.NotSupportedException : Specified method is not supported.  0

                    node.NullableNode.children = FieldNode.GetFieldNodes(node.NullableNode.Type);
                    getterHierarchy.Add(node.info.GetValue);

                    var propertyGetter = node.info.FieldType.GetProperty("HasValue");
                    Func<object, object> hasValueGetter = x => x != null ? propertyGetter.GetValue(x) : null;
                    getterHierarchy.Add(hasValueGetter);
                    Func<object, bool> hasValueEmptyComparator = x => x == null;
                    FindPrimitiveOffset<T>(ref node.NullableNode.children[0].PrimitiveNode, getterHierarchy, hasValueEmptyComparator, twiddlerHierarchy);
                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);

                    var underType = Nullable.GetUnderlyingType(node.NullableNode.Type);
                    Func<object, bool> nullableEmptyComparator = x => Activator.CreateInstance(underType).Equals(x);
                    var valueProperty = node.info.FieldType.GetProperty("Value"); // may cache handles 
                    getterHierarchy.Add(valueProperty.GetValue);
                    var t = new NullableTwiddler(node.NullableNode.children[0].PrimitiveNode.totalOffset);
                    twiddlerHierarchy.Add(t.Twiddle);
                    GetLayout<T>(ref previous, ref node.NullableNode.children[1], getterHierarchy, twiddlerHierarchy);
                    twiddlerHierarchy.RemoveAt(twiddlerHierarchy.Count - 1);
                    node.totalOffset = node.NullableNode.children[0].PrimitiveNode.totalOffset;
                    node.NullableNode.size = node.NullableNode.children[1].PrimitiveNode.totalOffset + node.NullableNode.children[1].PrimitiveNode.size - node.totalOffset;
                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);

                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    break;
                case NodeKind.Fixed:
                    // TODO: support non primitive fixed as soon as C# will do that https://github.com/dotnet/csharplang/issues/1494
                    var fType = node.PrimitiveNode.Type;
                    Func<object, bool> fixedEmptyComparator = x => Activator.CreateInstance(fType).Equals(x);
                    getterHierarchy.Add(node.info.GetValue);
                    FindPrimitiveOffset<T>(ref node.PrimitiveNode, getterHierarchy, fixedEmptyComparator, twiddlerHierarchy);
                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    node.FixedNode.size = node.FixedNode.size * node.FixedNode.length;
                    break;
                case NodeKind.Root:
                    if (!node.RootNode.IsPrimitive)
                    {
                        for (var i = 0; i < node.RootNode.children.Length; i++)
                        {
                            ref var child = ref node.RootNode.children[i];
                            GetLayout<T>(ref previous, ref child, getterHierarchy, twiddlerHierarchy);
                        }
                    }
                    break;

                case NodeKind.Complex:
                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.Add(node.info.GetValue);
                    }
                    node.ComplexNode.children = FieldNode.GetFieldNodes(node.ComplexNode.Type);
                    for (var i = 0; i < node.ComplexNode.children.Length; i++)
                    {
                        ref var child = ref node.ComplexNode.children[i];
                        GetLayout<T>(ref previous, ref child, getterHierarchy, twiddlerHierarchy);
                    }

                    //TODO: use ref compare-sort https://github.com/dotnet/corefx/issues/33927
                    node.ComplexNode.children = node.ComplexNode.children.OrderBy(x => x.totalOffset).ThenByDescending(x => x.size).ToArray();
                    node.totalOffset = node.ComplexNode.children[0].totalOffset;

                    for (var i = 0; i < node.ComplexNode.children.Length; i++)
                    {
                        ref var child = ref node.ComplexNode.children[i];
                        var possibleEnd = child.totalOffset + child.size;
                        node.ComplexNode.size = Math.Max(node.ComplexNode.size, possibleEnd - node.ComplexNode.totalOffset);
                    }
                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    }
                    break;
                default:
                    throw new NotImplementedException($"{node.kind} is not supported");
            }
        }

        // modifies bytes and scans for non equality
        // supports structs of all layout with overlapping, so start from zero each time (so could have different code for explicit layout for performance and padding correctness)
        private static void FindPrimitiveOffset<T>(
           ref PrimitiveNode node,
           List<Func<object, object>> getterHierarchy,
           Func<object, bool> emptyComparator,
           List<Twiddler<T>> twiddlerHierarchy)
           where T : struct
        {
            T seed = default;
            ref var seedByteRef = ref Unsafe.As<T, byte>(ref seed);
            var size = Unsafe.SizeOf<T>();
            var offset = -1;
            var fieldSize = -1;
            for (int i = 0; i < size; i++)
            {
                Unsafe.InitBlock(ref Unsafe.As<T, byte>(ref seed), 0, (uint)size); // zero, including paddings
                for (var t = 0; t < twiddlerHierarchy.Count; t++)
                {
                    var twiddler = twiddlerHierarchy[t];
                    if (twiddler != null) twiddler(ref seed);
                }

                seedByteRef = 1;
                object modifiedValue = GetValue(getterHierarchy, seed);
                if (!emptyComparator(modifiedValue) && offset < 0)
                {
                    offset = i;
                }

                if (offset >= 0)
                {
                    if (!emptyComparator(modifiedValue) || (fieldSize < 0 && size == i + 1))
                    {
                        fieldSize = i + 1 - offset;
                    }
                }

                seedByteRef = ref Unsafe.Add(ref seedByteRef, 1);
            }

            if (offset >= 0 && fieldSize >= 0)
            {
                node.totalOffset = offset;
                node.size = fieldSize;
                return;
            }

            throw new NotImplementedException($"Failed to find offset and size of {node.Type.FullName} in {typeof(T).FullName}");
        }

        private static void FindReferenceOffset<T>(ref ReferenceNode node, List<Func<object, object>> getterHierarchy)
               where T : struct
        {
            var fieldInfo = node.info;
            var endFieldType = fieldInfo.FieldType;
            T seed = default;
            ref var seedByteRef = ref Unsafe.As<T, byte>(ref seed);
            var size = Unsafe.SizeOf<T>();
            for (int i = 0; i < size; i++)
            {
                Unsafe.InitBlock(ref Unsafe.As<T, byte>(ref seed), 0, (uint)size); // zero, including paddings

                seedByteRef = byte.MaxValue;
                object value2 = fieldInfo.GetValue(GetValue(getterHierarchy, seed));
                if (value2 != null)
                {
                    node.totalOffset = i;
                    return;
                }

                seedByteRef = ref Unsafe.Add(ref seedByteRef, 1);
            }

            throw new NotImplementedException($"Failed to find offset and size of {endFieldType.FullName} in {typeof(T).FullName}");
        }

        private static int Add(int offset, int size, FieldInfo field, List<FieldLayout> fieldsLayout)
        {
            var fieldLayout = new FieldLayout(offset, field, size);
            fieldsLayout.Add(fieldLayout);
            return offset + size;
        }

        private static object GetValue<T>(List<Func<object, object>> getterHierarchy, in T rootDummy) where T : struct
        {
            object value = rootDummy;
            for (int i = 0; i < getterHierarchy.Count; i++)
            {
                var field = getterHierarchy[i];
                value = field(value);
            }

            return value;
        }
    }
}