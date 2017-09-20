using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Represents layout of a given type.
    /// </summary>
    public struct TypeLayout
    {
        public Type Type { get; }

        public int FullSize => Size + Overhead;

        /// <summary>
        /// Size of the type instance
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Overhead for a reference types
        /// </summary>
        public int Overhead { get; }

        /// <summary>
        /// Size of an empty space in the instance
        /// </summary>
        public int Paddings { get; }

        public FieldLayoutBase[] Fields { get; }

        public TypeLayout(Type type, int size, int overhead, FieldLayoutBase[] fields)
        {
            Type = type;
            Size = size;
            Overhead = overhead;
            Fields = fields;
            var thisInstancePaddings = fields.OfType<Padding>().Sum(p => p.Size);

            // TODO: this can be expensive.
            var nestedPaddings = fields
                // Skipping primitive types are recursive and they don't have any paddings.
                .OfType<FieldLayout>().Where(fl => !fl.FieldInfo.FieldType.IsPrimitive).Select(fl => GetLayout(fl.FieldInfo.FieldType))
                .Sum(tl => tl.Paddings);

            Paddings = thisInstancePaddings + nestedPaddings;
        }

        public static void PrintLayout<T>(bool recursively = true)
        {
            LayoutPrinter.Print<T>(recursively);
        }

        public static TypeLayout GetLayout<T>(bool includePaddings = true)
        {
            return GetLayout(typeof(T), includePaddings);
        }

        public static TypeLayout GetLayout(Type type, bool includePaddings = true)
        {
            var (size, overhead) = InspectorHelper.GetSize(type);

            var fieldsOffsets = InspectorHelper.GetFieldOffsets(type);
            var fields = new List<FieldLayoutBase>();

            for (var index = 0; index < fieldsOffsets.Length; index++)
            {
                var fieldOffset = fieldsOffsets[index];
                var fieldInfo = new FieldLayout(fieldOffset.offset, fieldOffset.fieldInfo);
                fields.Add(fieldInfo);

                if (includePaddings)
                {
                    int nextOffsetOrSize = size;
                    if (index != fieldsOffsets.Length - 1)
                    {
                        // This is not a last field.
                        nextOffsetOrSize = fieldsOffsets[index + 1].offset;
                    }

                    var nextSectionOffsetCandidate = fieldInfo.Offset + fieldInfo.Size;
                    if (nextSectionOffsetCandidate < nextOffsetOrSize)
                    {
                        // we have padding
                        fields.Add(new Padding(nextOffsetOrSize - nextSectionOffsetCandidate, nextSectionOffsetCandidate));
                    }
                }
            }

            return new TypeLayout(type, size, overhead, fields.ToArray());
        }

        public bool Equals(TypeLayout other)
        {
            return Type == other.Type && Size == other.Size && Overhead == other.Overhead && Paddings == other.Paddings;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypeLayout && Equals((TypeLayout)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Size;
                hashCode = (hashCode * 397) ^ Overhead;
                hashCode = (hashCode * 397) ^ Paddings;
                return hashCode;
            }
        }

    }
}