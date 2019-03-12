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
    /// Gets layout via Unsafe. Does not use code generation and works in Ahead of Time (AOT).
    /// </summary>
    public static class UnsafeLayout
    {
        /// <summary>
        /// Get fields layout of <typeparamref name="T"/> with no padding information ordered by offset.
        /// </summary>
        /// <typeparam name="T">To to get structure of.</>
        public static IReadOnlyList<FieldLayout> GetFieldsLayout<T>(bool recursive = true, IReadOnlyCollection<Type> primitives = null)
            where T : struct
        {
            var fieldsLayout = GetFieldsLayoutInternal<T>(recursive, primitives);
            return fieldsLayout.OrderBy(x => x.Offset).ToList();
        }

        /// <summary>
        /// Get layout of <typeparamref name="T"/> with padding information ordered by offset.
        /// </summary>
        public static IReadOnlyList<FieldLayoutBase> GetLayout<T>(bool recursive = true, IReadOnlyCollection<Type> primitives = null, bool hierarchical = false)
            where T : struct
        {
            if (hierarchical) throw new NotImplementedException("Could report recursive layout with complex fields in each other");
            var fields = GetFieldsLayoutInternal<T>(recursive, primitives);
            return AddPaddings<T>(fields);
        }

        private static List<FieldLayout> GetFieldsLayoutInternal<T>(bool recursive, IReadOnlyCollection<Type> primitives) where T : struct
        {
            var offsets = 0;
            var type = typeof(T);
            var fieldsLayout = new List<FieldLayout>();
            if (!IsPrimitive(type) && !(primitives?.Contains(type) == true))
            {
                var typeHierarchy = new List<Type> { type };
                var fieldsHierarchy = new List<FieldInfo>();
                GetLayout<T>(ref offsets, typeHierarchy, fieldsHierarchy, fieldsLayout, recursive, primitives);
            }

            return fieldsLayout;
        }

        private static IReadOnlyList<FieldLayoutBase> AddPaddings<T>(List<FieldLayout> fieldsLayout)
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
                    fieldsAndPaddings.Add(new Padding(lastOffset, paddingSize));

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
                    else break;
                }
            }

            var lastByte = fieldsAndPaddings.Max(x => x.Offset + x.Size);
            var ending = Unsafe.SizeOf<T>() - lastByte;
            if (ending > 0)
                fieldsAndPaddings.Add(new Padding(lastByte, ending));

            return fieldsAndPaddings;
        }
        public struct nullable<T> where T : struct
        {
            private bool a;
            private T b;
        }
        private static void GetLayout<T>(
           ref int previousOffset,
           List<Type> typeHierarchy,
           List<FieldInfo> fieldsHierarchy,
           List<FieldLayout> fieldsLayout,
           bool recursive,
           IReadOnlyCollection<Type> primitives)
           where T : struct
        {
            var fields = ReflectionHelper.GetInstanceFields(typeHierarchy.Last());

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                var fieldType = field.FieldType;
                int? fixedData = null;
                if (IsPrimitive(fieldType)
                    || (primitives?.Contains(fieldType) == true)
                    || !fieldType.IsValueType
                    || IsNullable(fieldType)
                    || IsFixed(field, out fixedData)
                    || !recursive)
                {
                    fieldsHierarchy.Add(field);
                    FindOffset<T>(ref previousOffset, typeHierarchy, fieldsHierarchy, fieldsLayout, recursive, primitives, fixedData);
                    fieldsHierarchy.RemoveAt(fieldsHierarchy.Count - 1);

                }
                else
                {
                    typeHierarchy.Add(field.FieldType);
                    fieldsHierarchy.Add(field);
                    GetLayout<T>(ref previousOffset, typeHierarchy, fieldsHierarchy, fieldsLayout, recursive, primitives);
                    typeHierarchy.RemoveAt(typeHierarchy.Count - 1);
                    fieldsHierarchy.RemoveAt(fieldsHierarchy.Count - 1);
                }
            }
        }

        private static bool IsNullable(Type t) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

        private static bool IsFixed(FieldInfo fieldInfo, out int? fixedBuffer)
        {
            var fixedCheck = fieldInfo
                             .CustomAttributes.Where(x => x.AttributeType.Equals(typeof(FixedBufferAttribute)))
                             .Select(x => x.ConstructorArguments)
                             .FirstOrDefault();
            if (fixedCheck != null)
            {
                fixedBuffer = (int)fixedCheck[1].Value;
                return true;
            }

            fixedBuffer = null;
            return false;
        }


        // must be of same layout as Nullable<T>

        [Serializable]
        private struct NullableLayout<T> where T : struct
        {
            private readonly bool hasValue; // Do not rename (binary serialization)
            internal T value;
        }

        private static bool IsPrimitive(Type componentType) =>
            componentType.IsPrimitive || componentType.IsEnum || componentType == typeof(decimal);

        // we support structs of all layout with overlapping, so start from zero each time
        // may split sequential layout and overlapped to improve performance if needed
        //
        // cannot use Marshal.SizeOf as does not works with generics and for (bool, bool) gives 8, not 2   
        //
        // next could support classes (how to get Unsafe into heap of object fields porable?)
        // https://stackoverflow.com/questions/18937935/how-to-mutate-a-boxed-struct-using-il
        //
        // next could work in unity if unsafe fails to work (non portable)
        // https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.html  
        //   
        private static void FindOffset<T>(
           ref int previousOffset,
           List<Type> typeHierarchy,
           List<FieldInfo> fieldsHierarchy,
           List<FieldLayout> fieldsLayout,
           bool recursive,
           IReadOnlyCollection<Type> primitives,
           int? fixedData)
           where T : struct
        {
            var fieldInfo = fieldsHierarchy.Last();
            var endFieldType = fieldInfo.FieldType;
            T dummy = default;
            ref var dummyRef = ref Unsafe.As<T, byte>(ref dummy);
            var size = Unsafe.SizeOf<T>();
            var offset = -1;
            var fieldSize = -1;
            for (int i = 0; i < size; i++)
            {
                Unsafe.InitBlock(ref Unsafe.As<T, byte>(ref dummy), 0, (uint)size); // zero, including paddings
                if (endFieldType.IsValueType && IsNullable(endFieldType))
                {
                    if (offset >= 0)
                    {
                        ref var hasValue = ref Unsafe.Add(ref Unsafe.As<T, byte>(ref dummy), offset);
                        hasValue = 1;
                        dummyRef = byte.MaxValue;
                        // Activator.CreateInstance creates null for nullable
                        // cannot FieldInfo.GetValue fields of Value and HasValue as System.NotSupportedException : Specified method is not supported.                        
                        var valueProperty = endFieldType.GetProperty("Value"); // TODO: cache handles 
                        object modifiedValue = GetValue(fieldsHierarchy, dummy);
                        var valueInside = valueProperty.GetValue(modifiedValue);
                        var empty = Activator.CreateInstance(Nullable.GetUnderlyingType(endFieldType));
                        if (!empty.Equals(valueInside))
                        {
                            if (fieldSize == -1)
                                fieldSize = -2;
                        }
                        else if (fieldSize == -2)
                        {
                            fieldSize = i + 1 - offset;
                        }

                        if (fieldSize == -2 && size == i + 1)
                            fieldSize = i + 1 - offset;
                    }
                    else
                    {
                        dummyRef = 1;
                        var hasValueProperty = endFieldType.GetProperty("HasValue");
                        object modifiedValue = GetValue(fieldsHierarchy, dummy);
                        if (modifiedValue != null && (bool)(hasValueProperty.GetValue(modifiedValue)) == true)
                        {
                            if (offset < 0)
                                offset = i;
                        }
                    }
                }
                else if (endFieldType.IsValueType)
                {
                    var empty = Activator.CreateInstance(endFieldType);
                    dummyRef = byte.MaxValue;
                    object modifiedValue = GetValue(fieldsHierarchy, dummy);
                    if (!empty.Equals(modifiedValue) && offset < 0)
                        offset = i;

                    if (offset >= 0)
                        if (!empty.Equals(modifiedValue) || (fieldSize < 0 && size == i + 1))
                            fieldSize = i + 1 - offset;
                }
                else
                {
                    dummyRef = byte.MaxValue;
                    object value2 = GetValue(fieldsHierarchy, dummy);
                    if (value2 != null)
                        previousOffset = Add(i, Unsafe.SizeOf<IntPtr>(), fieldInfo, fieldsLayout);
                        return;
                }

                dummyRef = ref Unsafe.Add(ref dummyRef, 1);
            }

            if (offset >= 0 && fieldSize >= 0)
            {
                if (fixedData.HasValue)
                {
                    previousOffset = Add(offset, fieldSize * fixedData.Value, fieldInfo, fieldsLayout);
                    return;
                }

                previousOffset = Add(offset, fieldSize, fieldInfo, fieldsLayout);
                return;
            }

            throw new NotImplementedException($"Failed to find offset and size of {endFieldType.FullName} in {typeof(T).FullName}");
        }

        private static int Add(int offset, int size, FieldInfo field, List<FieldLayout> fieldsLayout)
        {
            var fieldLayout = new FieldLayout(offset, field, size);
            fieldsLayout.Add(fieldLayout);
            return offset + size;
        }

        private static object GetValue<T>(List<FieldInfo> fieldsHierarchy, T rootDummy) where T : struct
        {
            object value = rootDummy;
            for (int i = 0; i < fieldsHierarchy.Count; i++)
            {
                FieldInfo field = fieldsHierarchy[i];
                value = field.GetValue(value);
            }

            return value;
        }
    }
}
