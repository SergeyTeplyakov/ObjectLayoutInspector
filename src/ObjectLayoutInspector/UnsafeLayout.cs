using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ValueGetter = System.Func<object?, object?>;
using ValueComparer = System.Func<object?, bool>;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Gets layout via <see cref="Unsafe"/> portable.  Works in Ahead of Time (AOT).
    /// Does not use code generation or CLR debugging mechanisms or runtime intrinsic.
    /// </summary>
    /// <remarks>
    /// Works on Little Endian ARM Android and iOS AOT(IL2CPP).
    /// </remarks>
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
    // 4. as field value got and activator created instance - reuse it for all siblings instead of GC churn (not one by one, but batch)
    public static class UnsafeLayout
    {
        /// <summary>
        /// Get fields layout of <typeparamref name="T"/> with no padding information ordered by offset.
        /// </summary>
        /// <typeparam name="T">To to get structure of.</typeparam>
        /// <param name="considerPrimitives">Eny type in collection will be considered primitive and will not be splitted into several fields.</param>
        /// <param name="recursive">If true the resulting list will have fields for all nested types as well.</param>
        public static IReadOnlyList<FieldLayout> GetFieldsLayout<T>(bool recursive = true, IReadOnlyCollection<Type>? considerPrimitives = null)
            where T : struct
        {
            var tree = GetLayoutTree<T>();
            IEnumerable<FieldLayout> fieldsLayout = GetFieldsLayoutInternal<T>(in tree, recursive, considerPrimitives).OrderBy(x => x.Offset);
            return fieldsLayout.ToList();
        }

        /// <summary>
        /// Get layout of <typeparamref name="T"/> with padding information ordered by offset.
        /// </summary>
        /// <typeparam name="T">To to get structure of.</typeparam>
        /// <param name="considerPrimitives">Eny type in collection will be considered primitive and will not be splitted into several fields.</param>
        /// <param name="recursive">If true the resulting list will have fields for all nested types as well.</param>
        /// <param name="hierarchical">Not implemented yet if specified.</param>
        public static IReadOnlyList<FieldLayoutBase> GetLayout<T>(bool recursive = true, IReadOnlyCollection<Type>? considerPrimitives = null, bool hierarchical = false)
            where T : struct
        {
            if (hierarchical)
            {
                throw new NotImplementedException("Could report recursive layout with complex fields in each other");
            }

            var tree = GetLayoutTree<T>();
            IEnumerable<FieldLayout> fieldsLayout =
                GetFieldsLayoutInternal<T>(in tree, recursive, considerPrimitives)
                .OrderBy(x => x.Offset);

            var layouts = new List<FieldLayoutBase>();
            Padder.AddPaddings(true, Unsafe.SizeOf<T>(), fieldsLayout.OrderBy(x => x.Offset).ToArray(), layouts);
            return layouts;
        }

        private static IReadOnlyList<FieldLayout> GetFieldsLayoutInternal<T>(in RootNode tree, bool recursive, IReadOnlyCollection<Type>? primitives) where T : struct
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
                    var child = GetLayout(ref tree.children[i]);
                    fieldsLayout.Add(child);
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
                    layout.Add(new FieldLayout(node.referenceNode.totalOffset, node.referenceNode.info, node.referenceNode.size));
                    break;
                case NodeKind.Primitive:
                    layout.Add(new FieldLayout(node.primitiveNode.totalOffset, node.primitiveNode.info, node.primitiveNode.size));
                    break;
                case NodeKind.Nullable:
                    if (check(node))
                    {
                        layout.Add(new FieldLayout(node.nullableNode.totalOffset, node.nullableNode.info, node.nullableNode.size));
                        break;
                    }

                    GetLayout(ref node.nullableNode.hasValue.value, layout, check);
                    GetLayout(ref node.nullableNode.value.value, layout, check);

                    break;

                case NodeKind.Fixed:
                    layout.Add(new FieldLayout(node.fixedNode.totalOffset, node.fixedNode.info, node.fixedNode.size));
                    break;
                case NodeKind.Complex:
                    if (check(node))
                    {
                        layout.Add(new FieldLayout(node.complexNode.totalOffset, node.complexNode.info, node.complexNode.size));
                        break;
                    }

                    for (var i = 0; i < node.complexNode.children.Length; i++)
                    {
                        GetLayout(ref node.complexNode.children[i], layout, check);
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
                    return new FieldLayout(node.referenceNode.totalOffset, node.referenceNode.info, node.referenceNode.size);
                case NodeKind.Primitive:
                    return new FieldLayout(node.primitiveNode.totalOffset, node.primitiveNode.info, node.primitiveNode.size);
                case NodeKind.Nullable:
                    return new FieldLayout(node.nullableNode.totalOffset, node.nullableNode.info, node.nullableNode.size);
                case NodeKind.Fixed:
                    return new FieldLayout(node.fixedNode.totalOffset, node.fixedNode.info, node.fixedNode.size);
                case NodeKind.Complex:
                    {
                        var lastNodeKind = node.complexNode.children[node.complexNode.children.Length - 1].kind;
                        var layout = new FieldLayout(node.complexNode.totalOffset, node.complexNode.info, node.complexNode.size);
                        return lastNodeKind != NodeKind.Fixed ? FixPadding(layout) : layout;
                    }

                default:
                    throw new NotImplementedException($"{node.kind} is not supported");
            }
        }

        // paddings according on current running architecure-runtime
        // counts padding in the end of first struct as part of it
        // Unsafe can support padding in the end, but not in the end and state (doubt this happens)
        // https://en.wikipedia.org/wiki/Data_structure_alignment
        private static FieldLayout FixPadding(FieldLayout x)
        {
            if (x.Size > 8 && x.Size % 8 != 0 && !Detectors.IsFixed(x.FieldInfo, out var _))
            {
                return new FieldLayout(x.Offset, x.FieldInfo, x.Size + x.Size % 8);
            }

            return x;
        }

        private static RootNode GetLayoutTree<T>() where T : struct
        {
            var type = typeof(T);
            var fields = FieldNode.GetFieldNodes(type);
            var root = new FieldNode { kind = NodeKind.Root, rootNode = new RootNode { children = fields, totalOffset = 0, size = Unsafe.SizeOf<T>() } };
            var previous = 0;
            GetLayout<T>(ref previous, ref root, new List<ValueGetter>(), new List<Twiddler<T>?> { null });

            return root.rootNode;
        }

        private static void GetLayout<T>(
           ref int previous,
           ref FieldNode node,
           List<Func<object?, object?>> getterHierarchy,
           List<Twiddler<T>?> twiddlerHierarchy)
           where T : struct
        {
            switch (node.kind)
            {
                case NodeKind.Reference:
                    FindReferenceOffset<T>(ref node.referenceNode, getterHierarchy);
                    break;
                case NodeKind.Primitive:
                    var pType = node.primitiveNode.Type;
                    Func<object?, bool> primitiveEmptyComparator = x => Activator.CreateInstance(pType).Equals(x);
                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.Add(node.info.GetValue);
                    }

                    FindPrimitiveOffset<T>(ref node.primitiveNode, getterHierarchy, primitiveEmptyComparator, twiddlerHierarchy);

                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    }

                    break;
                case NodeKind.Nullable:
                    // Activator.CreateInstance creates null for nullable
                    // cannot FieldInfo.GetValue fields of Value and HasValue as System.NotSupportedException : Specified method is not supported.  0

                    var nullable = FieldNode.GetFieldNodes(node.nullableNode.Type);
                    node.nullableNode.hasValue = new Ref<FieldNode>(nullable[0]);
                    node.nullableNode.value = new Ref<FieldNode>(nullable[1]);
                    ref var hasValue = ref node.nullableNode.hasValue.value;
                    getterHierarchy.Add(node.info.GetValue);

                    var propertyGetter = node.info.FieldType.GetProperty("HasValue");
                    Func<object?, object?> hasValueGetter = x => x != null ? propertyGetter.GetValue(x) : null;
                    getterHierarchy.Add(hasValueGetter);
                    ValueComparer hasValueEmptyComparator = x => x == null;
                    FindPrimitiveOffset<T>(ref hasValue.primitiveNode, getterHierarchy, hasValueEmptyComparator, twiddlerHierarchy);
                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);

                    var underType = Nullable.GetUnderlyingType(node.nullableNode.Type);
                    ValueComparer nullableEmptyComparator = x => Activator.CreateInstance(underType).Equals(x);
                    var valueProperty = node.info.FieldType.GetProperty("Value"); // may cache handles 
                    getterHierarchy.Add(valueProperty.GetValue);
                    var t = new NullableTwiddler(hasValue.totalOffset);
                    twiddlerHierarchy.Add(t.Twiddle);
                    GetLayout<T>(ref previous, ref node.nullableNode.value.value, getterHierarchy, twiddlerHierarchy);
                    twiddlerHierarchy.RemoveAt(twiddlerHierarchy.Count - 1);
                    node.totalOffset = hasValue.primitiveNode.totalOffset;
                    node.nullableNode.size = node.nullableNode.value.value.totalOffset + node.nullableNode.value.value.size - node.totalOffset;
                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);

                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    break;
                case NodeKind.Fixed:
                    // TODO: support non primitive fixed as soon as C# will do that https://github.com/dotnet/csharplang/issues/1494
                    var fType = node.primitiveNode.Type;
                    ValueComparer fixedEmptyComparator = x => Activator.CreateInstance(fType).Equals(x);
                    getterHierarchy.Add(node.info.GetValue);
                    FindPrimitiveOffset<T>(ref node.primitiveNode, getterHierarchy, fixedEmptyComparator, twiddlerHierarchy);
                    getterHierarchy.RemoveAt(getterHierarchy.Count - 1);
                    node.fixedNode.size = node.fixedNode.size * node.fixedNode.length;
                    break;
                case NodeKind.Root:
                    if (!node.rootNode.IsPrimitive)
                    {
                        for (var i = 0; i < node.rootNode.children.Length; i++)
                        {
                            ref var child = ref node.rootNode.children[i];
                            GetLayout<T>(ref previous, ref child, getterHierarchy, twiddlerHierarchy);
                        }
                    }
                    break;

                case NodeKind.Complex:
                    if (!Detectors.IsNullable(node.info.DeclaringType))
                    {
                        getterHierarchy.Add(node.info.GetValue);
                    }

                    node.complexNode.children = FieldNode.GetFieldNodes(node.complexNode.Type);
                    for (var i = 0; i < node.complexNode.children.Length; i++)
                    {
                        ref var child = ref node.complexNode.children[i];
                        GetLayout<T>(ref previous, ref child, getterHierarchy, twiddlerHierarchy);
                    }

                    //TODO: use ref compare-sort https://github.com/dotnet/corefx/issues/33927
                    node.complexNode.children = node.complexNode.children.OrderBy(x => x.totalOffset).ThenByDescending(x => x.size).ToArray();
                    node.totalOffset = node.complexNode.children[0].totalOffset;

                    for (var i = 0; i < node.complexNode.children.Length; i++)
                    {
                        ref var child = ref node.complexNode.children[i];
                        var possibleEnd = child.totalOffset + child.size;
                        node.complexNode.size = Math.Max(node.complexNode.size, possibleEnd - node.complexNode.totalOffset);
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
           List<ValueGetter> getterHierarchy,
           Func<object?, bool> emptyComparator,
           List<Twiddler<T>?> twiddlerHierarchy)
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
                    twiddlerHierarchy[t]?.Invoke(ref seed);
                }

                seedByteRef = 1;
                object? modifiedValue = GetValue(getterHierarchy, seed);
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

        private static void FindReferenceOffset<T>(ref ReferenceNode node, List<Func<object?, object?>> getterHierarchy)
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

        private static object? GetValue<T>(List<ValueGetter> getterHierarchy, in T rootDummy) where T : struct
        {
            object? value = rootDummy;
            for (int i = 0; i < getterHierarchy.Count; i++)
            {
                Func<object?, object?> field = getterHierarchy[i];
                value = field(value);
            }

            return value;
        }
    }
}
