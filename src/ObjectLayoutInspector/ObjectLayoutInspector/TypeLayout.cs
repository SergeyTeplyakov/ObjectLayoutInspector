using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectLayoutInspector.Helpers;

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

        private TypeLayout(Type type, int size, int overhead, FieldLayoutBase[] fields, TypeLayoutCache cache)
        {
            Type = type;
            Size = size;
            Overhead = overhead;
            Fields = fields;

            // We can't get padding information for unsafe structs.
            // Assuming there is no one.
            var thisInstancePaddings = type.IsUnsafeValueType() ? 0 : fields.OfType<Padding>().Sum(p => p.Size);

            cache = cache ?? TypeLayoutCache.Create();

            var nestedPaddings = fields
                // Need to include paddings for value types only
                // because we can't tell if the reference is exclusive or shared.
                .OfType<FieldLayout>()
                // Primitive types can be recursive.
                .Where(fl => fl.FieldInfo.FieldType.IsValueType && !fl.FieldInfo.FieldType.IsPrimitive)
                .Select(fl => GetLayout(fl.FieldInfo.FieldType, cache, includePaddings: true))
                .Sum(tl => tl.Paddings);
            
            Paddings = thisInstancePaddings + nestedPaddings;

            // Updating the cache.
            cache.LayoutCache.AddOrUpdate(type, this, (t, layout) => layout);
        }

        public static void PrintLayout<T>(bool recursively = true)
        {
            LayoutPrinter.Print<T>(recursively);
        }

        public static void PrintLayout(Type type, bool recursively = true)
        {
            LayoutPrinter.Print(type, recursively);
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(recursively: true);

        public string ToString(bool recursively)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Type layout for '{Type.Name}'");

            int emptiness = (Paddings * 100) / Size;
            sb.AppendLine($"Size: {Size} bytes. Paddings: {Paddings} bytes (%{emptiness} of empty space)");

            sb.AppendLine(LayoutPrinter.TypeLayoutAsString(this, recursively: recursively));

            return sb.ToString();
        }

        public static TypeLayout? TryGetLayout(Type type, TypeLayoutCache cache)
        {
            if (type.CanCreateInstance())
            {
                return GetLayout(type, cache);
            }

            return null;
        }

        public static TypeLayout GetLayout<T>(TypeLayoutCache cache = null, bool includePaddings = true)
        {
            return GetLayout(typeof(T), cache, includePaddings);
        }

        public static TypeLayout GetLayout(Type type, TypeLayoutCache cache = null, bool includePaddings = true)
        {
            if (cache != null && cache.LayoutCache.TryGetValue(type, out var result))
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
                Console.WriteLine($"Failed to create an instance of type {type}");
                throw;
            }            

            TypeLayout DoGetLayout()
            {
                var (size, overhead) = InspectorHelper.GetSize(type);

                var fieldsOffsets = InspectorHelper.GetFieldOffsets(type);
                var fields = new List<FieldLayoutBase>();

                if (includePaddings && fieldsOffsets.Length != 0 && fieldsOffsets[0].offset != 0)
                {
                    fields.Add(new Padding(fieldsOffsets[0].offset, 0));
                }

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

                return new TypeLayout(type, size, overhead, fields.ToArray(), cache);
            }
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