using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace ObjectLayoutInspector2
{
    public abstract class FieldLayoutBase
    {
        public int Size { get; }
        public int Offset { get; }

        protected FieldLayoutBase(int size, int offset)
        {
            Size = size;
            Offset = offset;
        }

        public override string ToString()
        {
            int fieldSize = Size;
            string byteOrBytes = fieldSize == 1 ? "byte" : "bytes";

            string offsetStr = $"{Offset}-{Offset - 1 + fieldSize}";
            if (fieldSize == 1)
            {
                offsetStr = Offset.ToString();
            }

            return $"{offsetStr,7}: {NameOrDescription} ({fieldSize} {byteOrBytes})";
        }

        protected abstract string NameOrDescription { get; }
    }

    public sealed class FieldLayout : FieldLayoutBase
    {
        public FieldLayout(int offset, FieldInfo fieldInfo) 
            : base(Utils.GetFieldSize(fieldInfo.FieldType), offset)
        {
            FieldInfo = fieldInfo;
        }

        public FieldInfo FieldInfo { get; }

        protected override string NameOrDescription => 
            $"{FieldInfo.FieldType.Name} {FieldInfo.Name}";
    }

    public sealed class Padding : FieldLayoutBase
    {
        public Padding(int size, int offset) : base(size, offset)
        {
        }

        protected override string NameOrDescription => "padding";
    }

    public struct TypeLayout
    {
        public int Size { get; }
        public FieldLayoutBase[] Fields { get; }

        public TypeLayout(int size, FieldLayoutBase[] fields)
        {
            Size = size;
            Fields = fields;
        }

        public static TypeLayout GetLayout<T>(bool includePaddings = true)
        {
            var (size, overhead) = Utils.GetSize<T>();

            var fieldsOffsets = Utils.GetFieldOffsets<T>();
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

            return new TypeLayout(size + overhead, fields.ToArray());
        }

        //public (FieldInfo fieldInfo, int offset)[] Fields { get; }
    }



    public static class Utils
    {
        private static Type GenerateStructWithTwoFields(Type fieldType)
        {
            // Create assembly and module
            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");

            // Open a new Type for definition
            var typeBuilder = moduleBuilder.DefineType("MyCustomType",
                TypeAttributes.Public |
                TypeAttributes.Sealed |
                TypeAttributes.SequentialLayout |
                TypeAttributes.Serializable |
                TypeAttributes.AnsiClass,
                typeof(ValueType));

            typeBuilder.DefineField(
                "field1",
                fieldType,
                FieldAttributes.Public);

            typeBuilder.DefineField(
                "field2",
                fieldType,
                FieldAttributes.Public);

            var type = typeBuilder.CreateType();
            return type;
        }

        public static (int size, int overhead) GetSize<T>()
        {
            if (typeof(T).IsValueType)
            {
                var generatedType = GenerateStructWithTwoFields(typeof(T));
                var fieldsOffsets = GetFieldOffsets(generatedType);
                return (fieldsOffsets[1].offset, 0);
            }

            var fields = GetFieldOffsets<T>();
            if (fields.Length == 0)
            {
                return (IntPtr.Size, 2 * IntPtr.Size);
            }

            var maxValue = fields.MaxBy(tpl => tpl.offset);
            int sizeCandidate = maxValue.offset + GetFieldSize(maxValue.fieldInfo.FieldType);
            var reminder = sizeCandidate % IntPtr.Size;
            int size = reminder == 0 ? sizeCandidate : sizeCandidate - reminder + IntPtr.Size;

            return (size, 2 * IntPtr.Size);
        }

        public static int GetFieldSize(Type t)
        {
            if (t.IsValueType)
            {
                var generatedType = GenerateStructWithTwoFields(t);
                var fieldsOffsets = GetFieldOffsets(generatedType);
                return fieldsOffsets[1].offset;
            }

            return IntPtr.Size;
        }

        public static void Print<T>()
        {
            Console.WriteLine($"Type layout for '{typeof(T).FullName}'");

            var layout = TypeLayout.GetLayout<T>();
            Console.WriteLine($"Size: {layout.Size}");

            //var fieldsOffsets = GetFieldOffsets<T>();
            var fieldAsStrings = new List<string>(layout.Fields.Length);

            // Header description strings for a reference type
            if (!typeof(T).IsValueType)
            {
                var (header, mtPtr) = PrintHeader();
                fieldAsStrings.Add(header);
                fieldAsStrings.Add(mtPtr);
            }

            foreach (var field in layout.Fields)
            {
                fieldAsStrings.Add(field.ToString());
            }
            //for (var index = 0; index < fieldsOffsets.Length; index++)
            //{
            //    var fieldOffset = fieldsOffsets[index];
            //    fieldAsStrings.Add(ShortFieldName(fieldOffset.offset, fieldOffset.fieldInfo));

            //    if (index != fieldsOffsets.Length - 1)
            //    {
            //        // This is not a last field.
            //        //
            //    }
            //}

            int maxLength = fieldAsStrings.Count > 0 ? fieldAsStrings.Max(s => s.Length + 4) : 2;

            string stringRep = PrintLayout(typeof(T).IsValueType);
            Console.WriteLine(stringRep);

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
                    sb.AppendLine(WithTrailingSpaces($"| {fieldAsStrings[i]} "));

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
                    return $"{str}{new string(' ', size)}|";
                }
                string Repeat(char c) => new string(c, maxLength - 2);

            }
        }

        /*
        |==============================|
        | Object Header (4 bytes)      |
        |==============================|
        | Method Table Ptr (4 bytes)   |
        |==============================|
        |  0: FullSymbol FS (4 bytes)  |
        |------------------------------|
        | 12: FullSymbol FS2 (4 bytes) |
        |==============================|
    

         */

        /*
         * Instructions
============


ManagedObjectSize.LineInfo
Unreachable code detected

Instructions
============
ldc.i4.3
newarr System.UInt64
stloc.0 // System.UInt64[] ret
ldloc.0 // System.UInt64[] ret
ldc.i4.0
ldarg.0
ldflda System.Object ptr
conv.u8
stelem.i8
ldloc.0 // System.UInt64[] ret
ldc.i4.1
ldarg.0
ldflda Int32 number1
conv.u8
stelem.i8
ldloc.0 // System.UInt64[] ret
ldc.i4.2
ldarg.0
ldflda Int32 number2
conv.u8
stelem.i8
ldloc.0 // System.UInt64[] ret
ret
         * 
         * 
         * */

        public static (FieldInfo fieldInfo, int offset)[] GetFieldOffsets<T>()
        {
            var t = typeof(T);
            return GetFieldOffsets(t);
            //var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

            //var method = new DynamicMethod(
            //    name: "lambda",
            //    returnType: typeof(ulong[]),
            //    parameterTypes: new[] { typeof(T) },
            //    m: typeof(Utils).Module,
            //    skipVisibility: true);

            //ILGenerator ilGen = method.GetILGenerator();
            //ilGen.DeclareLocal(typeof(long[]));

            //// ldc.i4.4
            //// newarr System.UInt64
            //// stloc.0 // System.Int64[] ret
            //// var r = new ulong[fields.Length];
            //ilGen.Emit(OpCodes.Ldc_I4, fields.Length);
            //ilGen.Emit(OpCodes.Newarr, typeof(long));
            //ilGen.Emit(OpCodes.Stloc_0);

            //for (int i = 0; i < fields.Length; i++)
            //{
            //    //ldc.i4.0
            //    //ldarg.0
            //    //ldflda Byte m_byte1
            //    //conv.u8
            //    //stelem.i8
            //    ilGen.Emit(OpCodes.Ldloc_0);
            //    ilGen.Emit(OpCodes.Ldc_I4, i);

            //    ilGen.Emit(OpCodes.Ldarg_0); // Loading T onto evaluation stack

            //    ilGen.Emit(OpCodes.Ldflda, fields[i]);
            //    ilGen.Emit(OpCodes.Conv_I8);
            //    ilGen.Emit(OpCodes.Stelem_I8);
            //}

            //ilGen.Emit(OpCodes.Ldloc_0);
            //ilGen.Emit(OpCodes.Ret);

            //var getFieldOffsetsFn = (Func<T, long[]>)method.CreateDelegate(typeof(Func<T, long[]>));

            //var addresses = getFieldOffsetsFn(Activator.CreateInstance<T>());

            //if (addresses.Length == 0)
            //{
            //    return Array.Empty<(FieldInfo, int)>();
            //}

            //var min = addresses.Min();
            //return fields
            //    .Select((field, index) => (field: field, offset: (int)(addresses[index] - min)))
            //    .OrderBy(tpl => tpl.offset)
            //    .ToArray();
        }

        public static (FieldInfo fieldInfo, int offset)[] GetFieldOffsets(Type t)
        {
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

            var method = new DynamicMethod(
                name: "lambda",
                returnType: typeof(ulong[]),
                parameterTypes: new[] { typeof(object) },
                m: typeof(Utils).Module,
                skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();
            ilGen.DeclareLocal(typeof(long[]));

            // ldc.i4.4
            // newarr System.UInt64
            // stloc.0 // System.Int64[] ret
            // var r = new ulong[fields.Length];
            ilGen.Emit(OpCodes.Ldc_I4, fields.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(long));
            ilGen.Emit(OpCodes.Stloc_0);

            for (int i = 0; i < fields.Length; i++)
            {
                //ldc.i4.0
                //ldarg.0
                //ldflda Byte m_byte1
                //conv.u8
                //stelem.i8
                ilGen.Emit(OpCodes.Ldloc_0);
                ilGen.Emit(OpCodes.Ldc_I4, i);

                ilGen.Emit(OpCodes.Ldarg_0); // Loading T onto evaluation stack

                ilGen.Emit(OpCodes.Ldflda, fields[i]);
                ilGen.Emit(OpCodes.Conv_I8);
                ilGen.Emit(OpCodes.Stelem_I8);
            }

            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Ret);
            
            var getFieldOffsetsFn = (Func<object, long[]>)method.CreateDelegate(typeof(Func<object, long[]>));

            var addresses = getFieldOffsetsFn(Activator.CreateInstance(t));

            if (addresses.Length == 0)
            {
                return Array.Empty<(FieldInfo, int)>();
            }

            var min = addresses.Min();
            return fields
                .Select((field, index) => (field: field, offset: (int)(addresses[index] - min)))
                .OrderBy(tpl => tpl.offset)
                .ToArray();
        }

    }
}