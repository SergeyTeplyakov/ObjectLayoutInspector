using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectLayoutInspector
{
    internal static class LayoutPrinter
    {
        /// <summary>
        /// Prints the given <typeparamref name="T"/> to the console.
        /// </summary>
        public static void Print<T>(bool recursively = true) => Print(typeof(T), recursively);

        /// <summary>
        /// Prints the given <paramref name="type"/> to the console.
        /// </summary>
        public static void Print(Type type, bool recursively = true)
        {
            Console.WriteLine($"Type layout for '{type.Name}'");

            var layout = TypeLayout.GetLayout(type);
            int emptiness = (layout.Paddings * 100) / layout.Size;
            Console.WriteLine($"Size: {layout.Size} bytes. Paddings: {layout.Paddings} bytes (%{emptiness} of empty space)");

            Console.WriteLine(LayoutAsString(type, recursively));
        }

        public static void Print(TypeLayout layout, bool recursively = true)
        {
            Console.WriteLine($"Type layout for '{layout.Type.Name}'");

            int emptiness = (layout.Paddings * 100) / layout.Size;
            Console.WriteLine($"Size: {layout.Size} bytes. Paddings: {layout.Paddings} bytes (%{emptiness} of empty space)");

            Console.WriteLine(TypeLayoutAsString(layout, recursively));
        }

        private static string LayoutAsString(Type type, bool recursively = true)
        {
            var layout = TypeLayout.GetLayout(type);
            return TypeLayoutAsString(layout, recursively);
        }

        private static string TypeLayoutAsString(TypeLayout layout, bool recursively = true)
        {
            var fieldAsStrings = new List<string>(layout.Fields.Length);

            // Header description strings for a reference type
            if (!layout.Type.IsValueType)
            {
                var (header, mtPtr) = PrintHeader();
                fieldAsStrings.Add(header);
                fieldAsStrings.Add(mtPtr);
            }

            foreach (var field in layout.Fields)
            {
                string fieldAsString = field.ToString();

                // It make no sense to print fields of reference types recursively,
                // because technically, they're allocated in separate memory location.
                if (recursively && field is FieldLayout fl && fl.FieldInfo.FieldType.IsValueType)
                {
                    var fieldLayout = TypeLayout.GetLayout(fl.FieldInfo.FieldType);
                    if (fieldLayout.Fields.Length > 1)
                    {
                        fieldAsString += $"\r\n{LayoutAsString(fl.FieldInfo.FieldType)}";
                    }
                }

                fieldAsStrings.Add(fieldAsString);
            }

            int maxLength = fieldAsStrings.Count > 0
                ? fieldAsStrings
                    .SelectMany(s => s.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
                    .Max(s => s.Length + 4)
                : 2;

            string stringRep = PrintLayout(layout.Type.IsValueType);

            return stringRep;

            (string header, string methodTablePtr) PrintHeader()
            {
                int ptrSize = IntPtr.Size;
                return (
                    header: $"Object Header ({ptrSize} bytes)",
                    methodTablePtr: $"Method Table Ptr ({ptrSize} bytes)"
                    );
            }

            string PrintLayout(bool isValueType)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"|{Repeat('=')}|");
                int startIndex = 0;
                if (!isValueType)
                {
                    sb.AppendLine(WithTrailingSpaces($"| {fieldAsStrings[0]} "))
                        .AppendLine($"|{Repeat('-')}|")
                        .AppendLine(WithTrailingSpaces($"| {fieldAsStrings[1]} "))
                        .AppendLine($"|{Repeat('=')}|");

                    startIndex = 2;
                }

                for (int i = startIndex; i < fieldAsStrings.Count; i++)
                {
                    foreach (var str in fieldAsStrings[i].Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sb.AppendLine(WithTrailingSpaces($"| {str} "));
                    }

                    if (i != fieldAsStrings.Count - 1)
                    {
                        sb.AppendLine($"|{Repeat('-')}|");
                    }
                }

                sb.AppendLine($"|{Repeat('=')}|");
                return sb.ToString();

                string WithTrailingSpaces(string str)
                {
                    int size = maxLength - str.Length - 1;

                    return size > 0 ? $"{str}{new string(' ', size)}|" : $"{str}|";
                }

                string Repeat(char c) => new string(c, maxLength - 2);
            }
        }
    }
}