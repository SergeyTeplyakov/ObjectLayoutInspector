using ObjectLayoutInspector.Helpers;
using System;
using System.Diagnostics;
using System.Reflection;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Base type that represents different field layouts.
    /// </summary>
    public abstract class FieldLayoutBase
    {
        /// <summary>
        /// Size of a field.
        /// </summary>
#if NET8_0_OR_GREATER
        public virtual int Size { get; }
#else
        public int Size { get; }
#endif

        /// <summary>
        /// An offset of a field from the beginning of a struct.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Type which declared this field
        /// </summary>
        public abstract Type? DeclaringType { get; }

        /// <nodoc />
        protected FieldLayoutBase(int offset, int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Must be positive");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Must be not negative");
            Offset = offset;
            Size = size;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            int fieldSize = Size;

            string byteOrBytes = fieldSize == 1 ? "byte" : "bytes";

            string offsetStr;
            if (fieldSize == 1)
                offsetStr = Offset.ToString();
            else
                offsetStr = $"{Offset}-{Offset - 1 + fieldSize}";

            return $"{offsetStr,5}: {NameOrDescription} ({fieldSize} {byteOrBytes})";
        }

        /// <nodoc />
        protected abstract string NameOrDescription { get; }
    }

    /// <summary>
    /// Represents an actual layout of a field.
    /// </summary>
    public sealed class FieldLayout : FieldLayoutBase
    {
        /// <nodoc />
        public FieldLayout(int offset, FieldInfo? fieldInfo, int size)
            : base(offset, size)
        {
            FieldInfo = fieldInfo;
#if NET8_0_OR_GREATER
            InlineArrayLength = DeclaringType?.InlineArrayLength() ?? 0;
#endif
        }


        /// <nodoc />
        public FieldInfo? FieldInfo { get; }

        /// <nodoc />
        public override Type? DeclaringType => FieldInfo?.DeclaringType;

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is FieldLayout fieldLayout
            && Offset == fieldLayout.Offset
            && Size == fieldLayout.Size
            && FieldInfo == fieldLayout.FieldInfo;

        /// <inheritdoc />
        public override int GetHashCode() => (Offset, Size, FieldInfo).GetHashCode();

        /// <inheritdoc />
        protected override string NameOrDescription =>
#if NET8_0_OR_GREATER
            InlineArrayLength > 0 ? $"{FieldInfo?.FieldType.Name}[{InlineArrayLength}] {FieldInfo?.Name} for {DeclaringType?.Name}" :
#endif
            $"{FieldInfo?.FieldType.Name} {FieldInfo?.Name} for {DeclaringType?.Name}";

#if NET8_0_OR_GREATER
        /// <inheritdoc />
        public sealed override int Size => InlineArrayLength > 0 ? InlineArrayLength * base.Size : base.Size;

        /// <nodoc />
        public int InlineArrayLength { get; }
#endif
    }

    /// <summary>
    /// Represents a padding between fields.
    /// </summary>
    public sealed class Padding : FieldLayoutBase
    {
        /// <nodoc />
        public Padding(int offset, int size, Type? declaringType) : base(offset, size)
        {
            DeclaringType = declaringType;
        }

        /// <nodoc />
        public override Type? DeclaringType { get; }

        /// <inheritdoc />
        protected override string NameOrDescription => 
            $"padding for {DeclaringType?.Name}";

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is Padding padding
            && Offset == padding.Offset
            && Size == padding.Size
            && DeclaringType == padding.DeclaringType;

        /// <inheritdoc />
        public override int GetHashCode() => (Offset, Size, DeclaringType).GetHashCode();
    }
}