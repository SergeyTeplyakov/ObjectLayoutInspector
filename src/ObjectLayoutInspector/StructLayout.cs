﻿using System;
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
        public static IReadOnlyList<FieldLayout> GetFieldLayout<T>(bool recursive = true, IReadOnlyCollection<Type> primitives = null)
            where T : struct
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

            return fieldsLayout.OrderBy(x => x.Offset).ToList();
        }

        /// <summary>
        /// Get layout of <typeparamref name="T"/> with padding information ordered by offset.
        /// </summary>
        public static IReadOnlyList<FieldLayoutBase> GetLayout<T>(bool recursive = true, IReadOnlyCollection<Type> primitives = null)
            where T : struct
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

            return AddPaddings<T>(fieldsLayout);
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

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                (Type, int) fixedData = default;
                if (IsPrimitive(fieldType) || (primitives?.Contains(fieldType) == true) || !fieldType.IsValueType || IsFixed(field, out fixedData) || !recursive)
                {
                    fieldsHierarchy.Add(field);

                    var realStructOffset = FindOffset<T>(fieldsHierarchy);
                    fieldsHierarchy.RemoveAt(fieldsHierarchy.Count() - 1);

                    var size = fieldType.IsEnum ? Marshal.SizeOf(fieldType.GetEnumUnderlyingType())
                              : !fieldType.IsValueType ? Unsafe.SizeOf<IntPtr>()
                              : fixedData.Item1 != null ? Marshal.SizeOf(fixedData.Item1) * fixedData.Item2
                              : Marshal.SizeOf(fieldType);
                    var fieldLayout = new FieldLayout(realStructOffset, field, size);
                    previousOffset = realStructOffset + size;
                    fieldsLayout.Add(fieldLayout);
                }        
                else
                {
                    typeHierarchy.Add(field.FieldType);
                    fieldsHierarchy.Add(field);
                    GetLayout<T>(ref previousOffset, typeHierarchy, fieldsHierarchy, fieldsLayout, recursive, primitives);
                    typeHierarchy.RemoveAt(typeHierarchy.Count() - 1);
                    fieldsHierarchy.RemoveAt(fieldsHierarchy.Count() - 1);
                }
            }
        }

        private static bool IsFixed(FieldInfo fieldInfo, out (Type,int) fixedBuffer)
        {
           var fixedCheck = fieldInfo
                            .CustomAttributes.Where(x => x.AttributeType.Equals(typeof(FixedBufferAttribute)))
                            .Select(x => x.ConstructorArguments)
                            .FirstOrDefault();
            if (fixedCheck != null)
            {
                fixedBuffer = ((Type)fixedCheck[0].Value, (int)fixedCheck[1].Value);
                return true;
            }

            fixedBuffer = default;
            return false;
        }

        private static bool IsPrimitive(Type componentType)
        {
            return componentType == typeof(decimal)
                                || componentType == typeof(char)
                                || componentType.IsEnum
                                || componentType.IsPrimitive
                                || componentType == typeof(TimeSpan)
                                || componentType == typeof(DateTime)
                                || componentType == typeof(DateTimeOffset);
        }

        // we support stucts of all layout with overlapping, so start from zero each time
        // may split sequential layout and overlapped to improve performance if needed
        // next does could support classes (not sure if will work with non blittable)
        // https://stackoverflow.com/questions/18937935/how-to-mutate-a-boxed-struct-using-il
        // next could work in unity if unsafe-marshal fails to work (non portable)
        // https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Unity.Collections.LowLevel.Unsafe.UnsafeUtility.html        
        private static int FindOffset<T>(List<FieldInfo> fieldsHierarchy)
            where T : struct
        {
            T dummy = default;
            ref var dummyRef = ref Unsafe.As<T, byte>(ref dummy);
            var size = Unsafe.SizeOf<T>();
            Zero(ref dummyRef, size);
            for (int i = 0; i < size; i++)
            {
                object value1 = GetValueBefore(fieldsHierarchy, dummy);

                dummyRef = byte.MaxValue;

                object value2 = GetValueAfter(fieldsHierarchy, dummy);

                if (fieldsHierarchy.Last().FieldType.IsValueType)
                {
                    if (!value1.Equals(value2))
                    {
                        return i;
                    }
                }
                else
                {
                    if (!Object.ReferenceEquals(value1, value2))
                    {
                        return i;
                    }
                }


                dummyRef = ref Unsafe.Add(ref dummyRef, 1);
            }

            throw new InvalidOperationException("Cannot happen");
        }

        private static object GetValueAfter<T>(List<FieldInfo> fieldsHierarchy, T dummy) where T : struct
        {
            object value2 = dummy;
            foreach (var field in fieldsHierarchy)
            {
                value2 = field.GetValue(value2);
            }

            return value2;
        }

        private static object GetValueBefore<T>(List<FieldInfo> fieldsHierarchy, T dummy) where T : struct
        {
            object value1 = dummy;
            foreach (var field in fieldsHierarchy)
            {
                value1 = field.GetValue(value1);
            }

            return value1;
        }

        // paddings are not zeroed and need to pad after each iteration of searchin offset
        private static void Zero(ref byte dummyByte, int size)
        {
            for (int i = 0; i < size; i++)
            {
                dummyByte = 0;
                Unsafe.Add(ref dummyByte, 1);
            }
        }
    }
}
