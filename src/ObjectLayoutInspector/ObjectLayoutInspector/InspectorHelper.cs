using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using ObjectLayoutInspector.Helpers;

namespace ObjectLayoutInspector
{
    public static class InspectorHelper
    {
        /// <summary>
        /// Returns an instance size and the overhead for a given type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="type"/> is value type then the overhead is 0.
        /// Otherwise the overhead is 2 * PtrSize.
        /// </remarks>
        public static (int size, int overhead) GetSize(Type type)
        {
            if (type.IsValueType)
            {
                return (size: GetSizeOfValueTypeInstance(type), overhead: 0);
            }

            var size = GetSizeOfReferenceTypeInstance(type);
            return (size, 2 * IntPtr.Size);
        }

        /// <summary>
        /// Return s the size of a reference type instance excluding the overhead.
        /// </summary>
        public static int GetSizeOfReferenceTypeInstance(Type type)
        {
            Debug.Assert(!type.IsValueType);

            var fields = GetFieldOffsets(type);

            if (fields.Length == 0)
            {
                // Special case: the size of an empty class is 1 Ptr size
                return IntPtr.Size;
            }

            // The size of the reference type is computed in the following way:
            // MaxFieldOffset + SizeOfThatField
            // and round that number to closest point size boundary
            var maxValue = fields.MaxBy(tpl => tpl.offset);
            int sizeCandidate = maxValue.offset + GetFieldSize(maxValue.fieldInfo.FieldType);

            // Rounding this stuff to the nearest ptr-size boundary
            int roundTo = IntPtr.Size - 1;
            return (sizeCandidate + roundTo) & (~roundTo);
        }

        /// <summary>
        /// Returns the size of the field if the field would be of type <paramref name="t"/>.
        /// </summary>
        /// <remarks>
        /// For reference types the size is always a PtrSize.
        /// </remarks>
        public static int GetFieldSize(Type t)
        {
            if (t.IsValueType)
            {
                return GetSizeOfValueTypeInstance(t);
            }

            return IntPtr.Size;
        }

        /// <summary>
        /// Computes size for <paramref name="type"/>.
        /// </summary>
        public static int GetSizeOfValueTypeInstance(Type type)
        {
            Debug.Assert(type.IsValueType);
            // Generate a struct with two fields of type 'type'
            var generatedType = GenerateSizeComputerOf(type);
            // The offset of the second field is the size of 'type'
            var fieldsOffsets = GetFieldOffsets(generatedType);
            return fieldsOffsets[1].offset;
        }

        /// <summary>
        /// Helper struct that is used for computing the size of a struct.
        /// </summary>
        struct SizeComputer<T>
        {
#pragma warning disable 169 Unused field
            public T field1;
            public T field2;
#pragma warning restore 169 Unused field
        }

        /// <summary>
        /// Method "generates" a <code>SizeComputer&lt;FieldType&gt;</code> type at runtime.
        /// </summary>
        /// <remarks>
        /// Instead of generating a real type using <see cref="ModuleBuilder"/> for defining a new type this function
        /// generates a method that returns an empty instance of <code>SizeComputer&lt;FieldType&gt;</code>.
        /// There is a security issue with <see cref="ModuleBuilder"/>. Generated type resides in a new assembly that doesn't have
        /// a visibility to internal or private types and there is no way to work around this issue.
        /// So instead of that we're relying on the CLR and a magic of generics to generate a closed generic type at runtime.
        /// </remarks>
        private static Type GenerateSizeComputerOf(Type fieldType)
        {
            var returnType = typeof(SizeComputer<>).MakeGenericType(fieldType);

            var method = new DynamicMethod(
                name: "GenerateTypeForSizeComputation",
                returnType: typeof(object),
                parameterTypes: new Type[0],
                m: typeof(InspectorHelper).Module,
                skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();

            // This method generates the following function:
            //object GenerateTypeForSizeComputation()
            //{
            //    SizeComputer<FieldType> l1 = default(SizeComputer<FieldType>);
            //    object l2 = l1; // box the local
            //    return l2;
            //}

            ilGen.DeclareLocal(returnType);
            ilGen.DeclareLocal(typeof(object));

            // SizeComputer<FieldType> l1 = default(SizeComputer<FieldType>);
            ilGen.Emit(OpCodes.Ldloca_S, 0);
            ilGen.Emit(OpCodes.Initobj, returnType);

            // object l2 = l1; // box the local
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Box, returnType);
            ilGen.Emit(OpCodes.Stloc_1);

            // return l2;
            ilGen.Emit(OpCodes.Ldloc_1);
            ilGen.Emit(OpCodes.Ret);

            var func = (Func<object>) method.CreateDelegate(typeof(Func<object>));
            var result = func();
            return result.GetType();
        }

        public static (FieldInfo fieldInfo, int offset)[] GetFieldOffsets<T>()
        {
            return GetFieldOffsets(typeof(T));
        }

        public static (FieldInfo fieldInfo, int offset)[] GetFieldOffsets(Type t)
        {
            // GetFields does not return private fields from the base types.
            // Need to use a custom helper function.
            var fields = t.GetInstanceFields();
            //var fields2 = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            Func<object, long[]> fieldOffsetInspector = GenerateFieldOffsetInspectionFunction(fields);

            var (instance, success) = ReflectionHelper.TryCreateInstanceSafe(t);
            if (!success)
            {
                return Array.Empty<(FieldInfo, int)>();
            }

            var addresses = fieldOffsetInspector(instance);

            if (addresses.Length == 0)
            {
                return Array.Empty<(FieldInfo, int)>();
            }

            var baseLine = addresses.Min();
            
            // Converting field addresses to offsets using the first field as a baseline
            return fields
                .Select((field, index) => (field: field, offset: (int)(addresses[index] - baseLine)))
                .OrderBy(tpl => tpl.offset)
                .ToArray();
        }

        private static Func<object, long[]> GenerateFieldOffsetInspectionFunction(FieldInfo[] fields)
        {
            var method = new DynamicMethod(
                name: "GetFieldOffsets",
                returnType: typeof(long[]),
                parameterTypes: new[] { typeof(object) },
                m: typeof(InspectorHelper).Module,
                skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();

            // Declaring local variable of type long[]
            ilGen.DeclareLocal(typeof(long[]));
            // Loading array size onto evaluation stack
            ilGen.Emit(OpCodes.Ldc_I4, fields.Length);

            // Creating an array and storing it into the local
            ilGen.Emit(OpCodes.Newarr, typeof(long));
            ilGen.Emit(OpCodes.Stloc_0);

            for (int i = 0; i < fields.Length; i++)
            {
                // Loading the local with an array
                ilGen.Emit(OpCodes.Ldloc_0);

                // Loading an index of the array where we're going to store the element
                ilGen.Emit(OpCodes.Ldc_I4, i);

                // Loading object instance onto evaluation stack
                ilGen.Emit(OpCodes.Ldarg_0);

                // Getting the address for a given field
                ilGen.Emit(OpCodes.Ldflda, fields[i]);

                // Converting field offset to long
                ilGen.Emit(OpCodes.Conv_I8);

                // Storing the offset in the array
                ilGen.Emit(OpCodes.Stelem_I8);
            }

            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Ret);

            return (Func<object, long[]>)method.CreateDelegate(typeof(Func<object, long[]>));
        }
    }
}