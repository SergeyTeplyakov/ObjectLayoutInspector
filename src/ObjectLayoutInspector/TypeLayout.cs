using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectLayoutInspector.Helpers;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Represents layout of a given type.
    /// </summary>
    public readonly struct TypeLayout : IEquatable<TypeLayout>
    {
        /// <summary>
        /// A CLR type of the layout.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The full size of the type instance including an overhead.
        /// </summary>
        public int FullSize => Size + Overhead;

        /// <summary>
        /// Size of the type instance.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Overhead for a reference types.
        /// </summary>
        public int Overhead { get; }

        /// <summary>
        /// Size of an always empty space in the instance.
        /// </summary>
        public int Paddings { get; }

        /// <summary>
        /// Size of an always used space in the instance.
        /// </summary>
        public int Used { get; }

        /// <summary>
        /// Size of empty/or used space in the instance.
        /// </summary>
        public int Mixed { get; }

        /// <summary>
        /// Array holding used bytes
        /// </summary>
        internal BitArray UsedBytes { get; }

        /// <summary>
        /// Array holding padding bytes
        /// </summary>
        internal BitArray PaddingBytes { get; }

        /// <summary>
        /// Array holding mixed bytes
        /// </summary>
        internal BitArray MixedBytes { get; }

        /// <summary>
        /// Is the inspected type unsafe, if so the <see cref="Paddings"/> is unknown
        /// </summary>
        public bool IsUnsafeValueType { get; }

        /// <nodoc />
        public FieldLayoutBase[] Fields { get; }

        private TypeLayout(Type type, int size, int overhead, FieldLayoutBase[] fields, TypeLayoutCache? cache)
        {
            if (fields.FirstOrDefault(fl => fl.DeclaringType != type) is FieldLayoutBase fl)
                throw new ArgumentOutOfRangeException(nameof(fields), (fl, fl.DeclaringType.FullName, type.FullName), "Detected field from other type");

            Type = type;
            Size = size;
            Overhead = overhead;
            Fields = fields;
            IsUnsafeValueType = type.IsUnsafeValueType();

            cache ??= TypeLayoutCache.Create();

            UsedBytes = new BitArray(size);
            PaddingBytes = new BitArray(size);

            SetByteBits(fields, UsedBytes, PaddingBytes, cache);

            MixedBytes = UsedBytes.And(PaddingBytes);

            // We can't get padding information for unsafe structs.
            // Assuming there is no one.
            Paddings = IsUnsafeValueType ? 0 : PaddingBytes.GetSetCount() - MixedBytes.GetSetCount();
            Used = IsUnsafeValueType ? size : UsedBytes.GetSetCount() - MixedBytes.GetSetCount();
            Mixed = IsUnsafeValueType ? 0 : MixedBytes.GetSetCount();


            if (Paddings > Size)
                throw new ArgumentOutOfRangeException(nameof(fields), (size, Paddings), "Calculated padding was too big");

            // Updating the cache.
            cache.LayoutCache.AddOrUpdate(type, this, (t, layout) => layout);
        }

#if NET8_0_OR_GREATER
        internal static void SetByteBits(FieldLayoutBase[] fields, BitArray used, BitArray padding, TypeLayoutCache? cache, int offset = 0, FieldLayout flParent = default)
#else
        internal static void SetByteBits(FieldLayoutBase[] fields, BitArray used, BitArray padding, TypeLayoutCache? cache, int offset = 0)
#endif
        {
            foreach (var field in fields)
            {
                if (field is FieldLayout fl)
                {
                    // Need to include paddings for value types only
                    // because we can't tell if the reference is exclusive or shared.
                    // Primitive types can be recursive.
                    if (fl.FieldInfo.FieldType.IsValueType && 
                        !fl.FieldInfo.FieldType.IsPrimitive && 
                        GetLayout(fl.FieldInfo.FieldType, cache, includePaddings: true) is var lo && 
                        !lo.IsUnsafeValueType)
                    {
#if NET8_0_OR_GREATER
                        SetByteBits(lo.Fields, used, padding, cache, offset + fl.Offset, fl);
#else
                        SetByteBits(lo.Fields, used, padding, cache, offset + fl.Offset);
#endif
                    }
                    else
                    {
#if NET8_0_OR_GREATER
                        if(flParent?.InlineArrayLength is int ial && ial > 0)
                        {
                            for (int i = 0; i < ial; i++)
                            {
                                used.SetRange(field, offset + i * flParent.Size / ial);
                            }
                        }
                        else
#endif
                            used.SetRange(field, offset);
                    }
                }
                else
                {
#if NET8_0_OR_GREATER
                    if (flParent?.InlineArrayLength is int ial && ial > 0)
                    {
                        for (int i = 0; i < ial; i++)
                        {
                            padding.SetRange(field, offset + i * flParent.Size / ial);
                        }
                    }
                    else
#endif
                        padding.SetRange(field, offset);
                }
            }
        }

        /// <summary>
        /// <see cref="LayoutPrinter.Print{T}(bool)"/>
        /// </summary>
        public static void PrintLayout<T>(bool recursively = true)
        {
            LayoutPrinter.Print<T>(recursively);
        }

        /// <summary>
        /// <see cref="LayoutPrinter.Print(System.Type, bool)"/>
        /// </summary>
        public static void PrintLayout(Type type, bool recursively = true)
        {
            LayoutPrinter.Print(type, recursively);
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(recursively: true);

        /// <nodoc />
        public string ToString(bool recursively)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Type layout for '{Type.Name}'");

            int emptiness = (Paddings * 100) / Size;
            sb.AppendLine($"Size: {Size} bytes. Paddings: {Paddings} bytes (%{emptiness} of empty space)");

            sb.AppendLine(LayoutPrinter.TypeLayoutAsString(this, recursively: recursively));

            return sb.ToString();
        }

        /// <summary>
        /// Tries to get a layout of a given <paramref name="type"/> from <paramref name="cache"/>.
        /// </summary>
        public static TypeLayout? TryGetLayout(Type type, TypeLayoutCache cache)
        {
            if (type.CanCreateInstance())
            {
                return GetLayout(type, cache);
            }

            return null;
        }

        /// <summary>
        /// Gets a layout of <typeparamref name="T"/>.
        /// </summary>
        public static TypeLayout GetLayout<T>(TypeLayoutCache? cache = null, bool includePaddings = true)
        {
            return GetLayout(typeof(T), cache, includePaddings);
        }

        /// <summary>
        /// Gets a layout of a given <paramref name="type"/>.
        /// </summary>
        public static TypeLayout GetLayout(Type type, TypeLayoutCache? cache = null, bool includePaddings = true)
        {
            if (cache?.LayoutCache.TryGetValue(type, out var result) ?? false)
            {
                return result;
            }

            try
            {
                result = DoGetLayout();
                cache?.LayoutCache.TryAdd(type, result);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create an instance of type {type}: {e}.");
                throw;
            }

            TypeLayout DoGetLayout()
            {
                var (size, overhead) = TypeInspector.GetSize(type);

                // fields with no paddings
                var fieldsAndOffsets = TypeInspector.GetFieldOffsets(type);
                var fieldsOffsets = fieldsAndOffsets
                                    .Select(x => new FieldLayout(x.offset, x.fieldInfo, TypeInspector.GetFieldSize(x.fieldInfo.FieldType)))
                                    .ToArray();


                var layouts = new List<FieldLayoutBase>();

                Padder.AddPaddings(includePaddings, size, fieldsOffsets, layouts, type);

                return new TypeLayout(type, size, overhead, layouts.ToArray(), cache);
            }
        }

        /// <inheritdoc />
        public  bool Equals(TypeLayout other) => Type == other.Type && Size == other.Size && Overhead == other.Overhead && Paddings == other.Paddings;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is TypeLayout layout && Equals(layout);

        /// <inheritdoc />
        public override int GetHashCode() => (Type, Size, Overhead, Paddings).GetHashCode();
    }
}