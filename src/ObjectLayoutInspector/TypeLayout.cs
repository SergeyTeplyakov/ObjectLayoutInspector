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
    public struct TypeLayout : IEquatable<TypeLayout>
    {
        /// <summary>
        /// The CLR type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Returns the full size of the instance (including an overhead).
        /// </summary>
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

        /// <nodoc />
        public FieldLayoutBase[] Fields { get; }

        private TypeLayout(Type type, int size, int overhead, FieldLayoutBase[] fields, TypeLayoutCache? cache)
        {
            Type = type;
            Size = size;
            Overhead = overhead;
            Fields = fields;

            // We can't get padding information for unsafe structs.
            // Assuming there is no one.
            var thisInstancePaddings = type.IsUnsafeValueType() ? 0 : fields.OfType<Padding>().Sum(p => p.Size);

            cache ??= TypeLayoutCache.Create();

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

        /// <summary>
        /// <see cref="LayoutPrinter.Print{T}(bool)"/>
        /// </summary>
        public static void PrintLayout<T>(bool recursively = true)
        {
            LayoutPrinter.Print<T>(recursively);
        }

        /// <summary>
        /// <see cref="LayoutPrinter.Print(Type, bool)"/>
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

                Padder.AddPaddings(includePaddings, size, fieldsOffsets, layouts);

                return new TypeLayout(type, size, overhead, layouts.ToArray(), cache);
            }
        }

        /// <inheritdoc />
        public bool Equals(TypeLayout other)
        {
            return Type == other.Type && Size == other.Size && Overhead == other.Overhead && Paddings == other.Paddings;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TypeLayout && Equals((TypeLayout)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Type, Size, Overhead, Paddings).GetHashCode();
        }
    }
}