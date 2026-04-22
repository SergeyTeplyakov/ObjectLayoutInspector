using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ObjectLayoutInspector.Helpers;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Represents the memory layout of an array instance.
    /// </summary>
    public sealed class ArrayLayout
    {
        /// <summary>
        /// The element type of the array.
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// Number of elements in the array.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Size of a single element in bytes.
        /// </summary>
        public int ElementSize { get; }

        /// <summary>
        /// Object header size (sync block + method table pointer).
        /// </summary>
        public int HeaderSize { get; }

        /// <summary>
        /// Size of the array length field (always 4 bytes — DWORD m_NumComponents in CoreCLR).
        /// </summary>
        public int LengthFieldSize { get; }

        /// <summary>
        /// Padding after the length field to align element data to pointer-size boundary.
        /// </summary>
        public int LengthPadding { get; }

        /// <summary>
        /// Total size of the array instance in bytes (header + length + elements + padding).
        /// </summary>
        public int TotalSize { get; }

        /// <summary>
        /// Padding at the end to align to pointer-size boundary.
        /// </summary>
        public int TrailingPadding { get; }

        private ArrayLayout(Type elementType, int length)
        {
            ElementType = elementType;
            Length = length;

            HeaderSize = 2 * IntPtr.Size; // Object Header + Method Table Ptr
            LengthFieldSize = 4; // DWORD m_NumComponents in CoreCLR

            // Align element data start to pointer-size boundary
            int dataOffset = HeaderSize + LengthFieldSize;
            int roundTo = IntPtr.Size - 1;
            dataOffset = (dataOffset + roundTo) & (~roundTo);
            LengthPadding = dataOffset - HeaderSize - LengthFieldSize;

            ElementSize = elementType.IsValueType
                ? SizeOfValueType(elementType)
                : IntPtr.Size;

            int dataSize = dataOffset + ElementSize * Length;

            // Round up to pointer-size boundary
            TotalSize = (dataSize + roundTo) & (~roundTo);
            TrailingPadding = TotalSize - dataSize;
        }

        /// <summary>
        /// Gets the layout of an array of <typeparamref name="T"/> with the specified <paramref name="length"/>.
        /// </summary>
        public static ArrayLayout GetLayout<T>(int length) => GetLayout(typeof(T), length);

        /// <summary>
        /// Gets the layout of an array of <paramref name="elementType"/> with the specified <paramref name="length"/>.
        /// </summary>
        public static ArrayLayout GetLayout(Type elementType, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            return new ArrayLayout(elementType, length);
        }

        /// <summary>
        /// Prints the array layout to the console.
        /// </summary>
        public static void PrintLayout<T>(int length, bool showElementLayout = false)
        {
            Console.WriteLine(GetLayout<T>(length).ToString(showElementLayout));
        }

        /// <summary>
        /// Prints the array layout to the console.
        /// </summary>
        public static void PrintLayout(Type elementType, int length, bool showElementLayout = false)
        {
            Console.WriteLine(GetLayout(elementType, length).ToString(showElementLayout));
        }

        /// <summary>
        /// Prints the layout of an actual array instance, showing field values for each element.
        /// </summary>
        public static void PrintLayout<T>(T[] array) where T : struct
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            Console.WriteLine(GetLayout<T>(array.Length).ToStringWithValues(array));
        }

        /// <summary>
        /// Prints the layout of an actual array instance, showing field values for each element.
        /// Accepts any array type, including <c>Nullable&lt;T&gt;[]</c>.
        /// </summary>
        public static void PrintLayout(Array array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            var elementType = array.GetType().GetElementType()!;
            Console.WriteLine(GetLayout(elementType, array.Length).ToStringWithValues(array));
        }

        /// <inheritdoc />
        public override string ToString() => ToString(showElementLayout: false);

        /// <summary>
        /// Returns a string representation of the array layout.
        /// When <paramref name="showElementLayout"/> is true and the element type is a reference type,
        /// prints the array layout side-by-side with the element's heap layout.
        /// </summary>
        public string ToString(bool showElementLayout)
        {
            var sb = new StringBuilder();
            string typeName = $"{ElementType.Name}[{Length}]";
            sb.AppendLine($"Array layout for '{typeName}'");
            sb.AppendLine($"Element type: {ElementType.Name}. Element size: {ElementSize} bytes. Length: {Length}");
            sb.AppendLine($"Total size: {TotalSize} bytes (Header: {HeaderSize}, Length: {LengthFieldSize}, Length Padding: {LengthPadding}, Data: {ElementSize * Length}, Trailing Padding: {TrailingPadding})");
            sb.AppendLine();

            if (showElementLayout && !ElementType.IsValueType && ElementType.CanCreateInstance())
            {
                sb.Append(ElementLayoutString());
            }
            else
            {
                sb.Append(LayoutAsString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representation of the array layout with actual field values per element.
        /// </summary>
        public string ToStringWithValues(Array array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (array.GetType().GetElementType() != ElementType)
                throw new ArgumentException($"Array element type mismatch. Expected {ElementType.Name}.");
            if (array.Length != Length)
                throw new ArgumentException($"Array length mismatch. Expected {Length}, got {array.Length}.");

            var sb = new StringBuilder();
            string typeName = $"{ElementType.Name}[{Length}]";
            sb.AppendLine($"Array layout for '{typeName}' (with values)");
            sb.AppendLine($"Element type: {ElementType.Name}. Element size: {ElementSize} bytes. Length: {Length}");
            sb.AppendLine($"Total size: {TotalSize} bytes (Header: {HeaderSize}, Length: {LengthFieldSize}, Length Padding: {LengthPadding}, Data: {ElementSize * Length}, Trailing Padding: {TrailingPadding})");
            sb.AppendLine();

            var boxed = BuildBoxLines(LayoutLinesWithValues(array));
            foreach (var line in boxed)
                sb.AppendLine(line);
            return sb.ToString();
        }

        private string ElementLayoutString()
        {
            var sb = new StringBuilder();
            sb.Append(LayoutAsString());
            sb.AppendLine();
            sb.AppendLine("  Element layout:");
            sb.AppendLine();

            var elementLayout = TypeLayout.GetLayout(ElementType);
            string elementStr = elementLayout.ToString(recursively: true);
            var elementLines = elementStr.Split(["\r\n", "\n"], StringSplitOptions.None);

            foreach (var line in elementLines)
            {
                if (line.Length > 0)
                    sb.AppendLine("  " + line);
                else
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private string[] LayoutLines()
        {
            var lines = new List<string>();

            int ptrSize = IntPtr.Size;
            lines.Add($"Object Header ({ptrSize} bytes)");
            lines.Add($"Method Table Ptr ({ptrSize} bytes)");
            lines.Add($"Length ({LengthFieldSize} bytes) = {Length}");

            if (LengthPadding > 0)
            {
                lines.Add($"padding ({LengthPadding} bytes)");
            }

            int dataStart = HeaderSize + LengthFieldSize + LengthPadding;

            if (Length == 0)
            {
                // no elements
            }
            else if (ElementType.IsValueType && !ElementType.IsPrimitive && ElementSize > 0)
            {
                // For non-primitive value types, embed the element as a nested box
                // (matching how struct fields are rendered inside classes).
                var elementLayout = TypeLayout.GetLayout(ElementType, includePaddings: true);
                string elementBox = LayoutPrinter.TypeLayoutAsString(elementLayout, recursively: true)
                    .TrimEnd()
                    .Replace("\r\n", "\n");
                int showCount = Math.Min(Length, 3);
                for (int i = 0; i < showCount; i++)
                {
                    int offset = dataStart + i * ElementSize;
                    int end = offset + ElementSize - 1;
                    lines.Add($"{offset,5}-{end}: [{i}] {ElementType.Name} ({ElementSize} bytes)\n{elementBox}");
                }

                if (Length > showCount)
                    lines.Add($"  ... ({Length - showCount} more elements)");
            }
            else
            {
                string elementDesc = ElementType.IsValueType ? ElementType.Name : $"{ElementType.Name} (reference)";
                int showCount = Math.Min(Length, 3);
                for (int i = 0; i < showCount; i++)
                {
                    int offset = dataStart + i * ElementSize;
                    int end = offset + ElementSize - 1;
                    if (ElementSize == 1)
                        lines.Add($"   {offset,2}: [{i}] {elementDesc} ({ElementSize} byte)");
                    else
                        lines.Add($"{offset,5}-{end}: [{i}] {elementDesc} ({ElementSize} bytes)");
                }

                if (Length > showCount)
                    lines.Add($"  ... ({Length - showCount} more elements)");
            }

            if (TrailingPadding > 0)
            {
                int padStart = TotalSize - TrailingPadding;
                int padEnd = TotalSize - 1;
                lines.Add($"{padStart,5}-{padEnd}: padding ({TrailingPadding} bytes)");
            }

            return lines.ToArray();
        }

        private string[] LayoutLinesWithValues(Array array)
        {
            var lines = new List<string>();

            int ptrSize = IntPtr.Size;
            lines.Add($"Object Header ({ptrSize} bytes)");
            lines.Add($"Method Table Ptr ({ptrSize} bytes)");
            lines.Add($"Length ({LengthFieldSize} bytes) = {Length}");

            if (LengthPadding > 0)
            {
                lines.Add($"padding ({LengthPadding} bytes)");
            }

            int dataStart = HeaderSize + LengthFieldSize + LengthPadding;

            if (Length > 0 && ElementType.IsValueType && !ElementType.IsPrimitive)
            {
                var elementLayout = TypeLayout.GetLayout(ElementType, includePaddings: true);
                bool isNullable = Nullable.GetUnderlyingType(ElementType) != null;
                int showCount = Math.Min(Length, 3);
                for (int i = 0; i < showCount; i++)
                {
                    object? element = array.GetValue(i);
                    int offset = dataStart + i * ElementSize;
                    int end = offset + ElementSize - 1;

                    if (isNullable && element == null)
                    {
                        // Null nullable — show fields with default/null indicator
                        var entry = $"{offset,5}-{end}: [{i}] {ElementType.Name} ({ElementSize} bytes) = null";
                        lines.Add(entry);
                    }
                    else
                    {
                        var entry = $"{offset,5}-{end}: [{i}] {ElementType.Name} ({ElementSize} bytes)";

                        var fieldStrings = new List<string>();
                        foreach (var field in elementLayout.Fields)
                        {
                            string str = field.ToString();
                            if (field is FieldLayout fl)
                            {
                                object? value;
                                if (isNullable)
                                {
                                    // Boxing a non-null Nullable<T> produces the underlying T,
                                    // so we can't reflect on Nullable<T> fields directly.
                                    if (fl.FieldInfo.Name == "hasValue" || fl.FieldInfo.Name == "has_value")
                                        value = true;
                                    else
                                        value = element;
                                }
                                else
                                {
                                    value = GetFieldValue(fl.FieldInfo, element);
                                }
                                str += $" = {FormatValue(value)}";
                            }
                            fieldStrings.Add(str);
                        }

                        entry += $"\n{BuildNestedBox(fieldStrings)}";
                        lines.Add(entry);
                    }
                }

                if (Length > showCount)
                    lines.Add($"  ... ({Length - showCount} more elements)");
            }
            else if (Length > 0)
            {
                string elementDesc = ElementType.IsValueType ? ElementType.Name : $"{ElementType.Name} (reference)";
                int showCount = Math.Min(Length, 3);
                for (int i = 0; i < showCount; i++)
                {
                    object? element = array.GetValue(i);
                    int offset = dataStart + i * ElementSize;
                    int end = offset + ElementSize - 1;
                    string valStr = ElementType.IsValueType
                        ? $" = {FormatValue(element)}"
                        : element != null ? $" -> {FormatValue(element)}" : " -> null";
                    if (ElementSize == 1)
                        lines.Add($"   {offset,2}: [{i}] {elementDesc} ({ElementSize} byte){valStr}");
                    else
                        lines.Add($"{offset,5}-{end}: [{i}] {elementDesc} ({ElementSize} bytes){valStr}");
                }

                if (Length > showCount)
                    lines.Add($"  ... ({Length - showCount} more elements)");
            }

            if (TrailingPadding > 0)
            {
                int padStart = TotalSize - TrailingPadding;
                int padEnd = TotalSize - 1;
                lines.Add($"{padStart,5}-{padEnd}: padding ({TrailingPadding} bytes)");
            }

            return lines.ToArray();
        }

        private static string BuildNestedBox(List<string> fieldStrings)
        {
            int maxFieldLen = 0;
            foreach (var s in fieldStrings)
                if (s.Length > maxFieldLen) maxFieldLen = s.Length;

            int boxWidth = maxFieldLen + 4; // "| " + field + " |"
            string eq = new string('=', boxWidth - 2);
            string dash = new string('-', boxWidth - 2);

            var sb = new StringBuilder();
            sb.Append($"|{eq}|");
            for (int j = 0; j < fieldStrings.Count; j++)
            {
                sb.Append('\n');
                int pad = boxWidth - fieldStrings[j].Length - 4;
                sb.Append(pad > 0
                    ? $"| {fieldStrings[j]} {new string(' ', pad)}|"
                    : $"| {fieldStrings[j]} |");
                if (j < fieldStrings.Count - 1)
                {
                    sb.Append('\n');
                    sb.Append($"|{dash}|");
                }
            }
            sb.Append('\n');
            sb.Append($"|{eq}|");
            return sb.ToString();
        }

        private static string FormatValue(object? value)
        {
            if (value == null) return "null";
            if (value is string s) return $"\"{s}\"";
            if (value is bool b) return b ? "true" : "false";
            return value.ToString() ?? "null";
        }

        private static object? GetFieldValue(FieldInfo fieldInfo, object? obj)
        {
            try
            {
                return fieldInfo.GetValue(obj);
            }
            catch
            {
                // Private fields or type mismatch — try with NonPublic binding
                try
                {
                    var field = obj?.GetType().GetField(
                        fieldInfo.Name,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    return field?.GetValue(obj);
                }
                catch
                {
                    return "?";
                }
            }
        }

        private static string[] BuildBoxLines(string[] lines)
        {
            // Entries may contain embedded \n for grouped sub-lines (e.g. element fields).
            // Compute max width across all sub-lines.
            int maxLen = 0;
            foreach (var line in lines)
            {
                foreach (var sub in line.Split('\n'))
                    if (sub.Length + 4 > maxLen) maxLen = sub.Length + 4;
            }

            var result = new System.Collections.Generic.List<string>();
            string eq = new string('=', maxLen - 2);
            string dash = new string('-', maxLen - 2);

            result.Add($"|{eq}|");
            result.Add(PadLine($"| {lines[0]} ", maxLen));
            result.Add($"|{dash}|");
            result.Add(PadLine($"| {lines[1]} ", maxLen));
            result.Add($"|{eq}|");

            for (int i = 2; i < lines.Length; i++)
            {
                // Split multi-line entries and render sub-lines without separators.
                var subLines = lines[i].Split('\n');
                foreach (var sub in subLines)
                    result.Add(PadLine($"| {sub} ", maxLen));

                if (i < lines.Length - 1)
                    result.Add($"|{dash}|");
            }

            result.Add($"|{eq}|");
            return result.ToArray();

            static string PadLine(string s, int width)
            {
                int spaces = width - s.Length - 1;
                return spaces > 0 ? $"{s}{new string(' ', spaces)}|" : $"{s}|";
            }
        }

        private string LayoutAsString()
        {
            var boxed = BuildBoxLines(LayoutLines());
            var sb = new StringBuilder();
            foreach (var line in boxed)
                sb.AppendLine(line);
            return sb.ToString();
        }

        private static int SizeOfValueType(Type type)
        {
            // Use Unsafe.SizeOf via reflection for the element type
            var method = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf)).MakeGenericMethod(type);
            return (int)method.Invoke(null, null);
        }
    }
}
