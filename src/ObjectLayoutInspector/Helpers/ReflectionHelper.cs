using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ObjectLayoutInspector.Helpers
{
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Returns all instance fields including the fields declared in all base types.
        /// </summary>
        public static (FieldInfo[] fields, Type[] types) GetInstanceFields(this Type type)
        {
            var types = GetBaseTypesAndThis(type).Reverse().ToArray();

            return (types.SelectMany(t => GetDeclaredFields(t)).Where(fi => !fi.IsStatic).ToArray(), types);

            IEnumerable<Type> GetBaseTypesAndThis(Type t)
            {
                while (t is object)
                {
                    yield return t;

                    t = t.BaseType;
                }
            }

            IEnumerable<FieldInfo> GetDeclaredFields(Type t)
            {
                if (t is TypeInfo ti)
                {
                    return ti.DeclaredFields;
                }

                return t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        /// <summary>
        /// Tries to create an instance of a given type.
        /// </summary>
        /// <remarks>
        /// There is a limit of what types can be instantiated.
        /// The following types are not supported by this function:
        /// * Open generic types like <code>typeof(List&lt;&gt;)</code>
        /// * Abstract types
        /// </remarks>
        public static (object? result, bool success) TryCreateInstanceSafe(Type t)
        {
            if (!CanCreateInstance(t))
            {
                return (result: null, success: false);
            }

            // Value types are handled separately
            if (t.IsValueType)
            {
                var nullableType = Nullable.GetUnderlyingType(t);
                if (nullableType is object)
                {
                    return Success(GetUninitializedObject(t));
                }

                return Success(Activator.CreateInstance(t));
            }

            // String is handled separately as well due to security restrictions
            if (t == typeof(string))
            {
                return Success(string.Empty);
            }

            // It is actually possible that GetUnitializedObject will return null.
            // I've got null for some security related types.
            return Success(GetUninitializedObject(t));

            static (object? result, bool success) Success(object? o) => (o, o is object);
        }

        private static object? GetUninitializedObject(Type t)
        {
            try
            {
                return FormatterServices.GetUninitializedObject(t);
            }
            catch (TypeInitializationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the instance of type <paramref name="t"/> can be instantiated.
        /// </summary>
        public static bool CanCreateInstance(this Type t)
        {
            // Abstract types and generics are not supported
            if (t.IsAbstract || IsOpenGenericType(t) || t.IsCOMObject)
            {
                return false;
            }

            // TODO: check where ArgIterator is located
            if (// t == typeof(ArgIterator) || 
                t == typeof(RuntimeArgumentHandle) || t == typeof(TypedReference) || t.Name == "Void"
                || t == typeof(IsVolatile) || t == typeof(RuntimeFieldHandle) || t == typeof(RuntimeMethodHandle) ||
                t == typeof(RuntimeTypeHandle))
            {
                // This is a special type
                return false;
            }

            if (t.BaseType == typeof(ContextBoundObject))
            {
                return false;
            }

            return true;

            static bool IsOpenGenericType(Type type)
            {
                return type.IsGenericTypeDefinition && !type.IsConstructedGenericType;
            }
        }

        /// <summary>
        /// Returns true if a given type is unsafe.
        /// </summary>
        public static bool IsUnsafeValueType(this Type t) => t.GetCustomAttribute<UnsafeValueTypeAttribute>() is object;

#if NET8_0_OR_GREATER
        /// <summary>
        /// Checks if the type is inline array, returns the <see cref="InlineArrayAttribute.Length"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns><see cref="InlineArrayAttribute.Length"/></returns>
        public static int InlineArrayLength(this Type t) => t.GetCustomAttribute<InlineArrayAttribute>()?.Length ?? 0;
#endif
    }
}