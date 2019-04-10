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
        /// Prints a layout of a given <paramref name="type"/> to the console.
        /// </summary>
        public static void Print(Type type, bool recursively = true)
        {
            Print(TypeLayout.GetLayout(type), recursively);
        }

        /// <summary>
        /// Prints a given <paramref name="layout"/> to the console.
        /// </summary>
        public static void Print(TypeLayout layout, bool recursively = true)
        {
            Console.WriteLine(layout.ToString(recursively));
        }

        private static string LayoutAsString(Type type, bool recursively = true)
        {
            var layout = TypeLayout.GetLayout(type);
            return TypeLayoutAsString(layout, recursively);
        }

        /// <summary>
        /// Creates a string representation of a given <paramref name="layout"/>.
        /// </summary>
        public static string TypeLayoutAsString(TypeLayout layout, bool recursively = true)
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
                    
                    // Printing the nested structure of a field only if the field's type has fields and the field's type is not a primitive
                    // The second part is crucial otherwise types like Int32 will cause infinite recursion.
                    if (fieldLayout.Fields.Length > 0 && !fieldLayout.Type.IsPrimitive)
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