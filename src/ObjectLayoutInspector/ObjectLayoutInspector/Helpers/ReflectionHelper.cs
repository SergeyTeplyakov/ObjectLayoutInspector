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
        public static FieldInfo[] GetInstanceFields(this Type type)
        {
            return GetBaseTypesAndThis(type).SelectMany(t => GetDeclaredFields(t)).Where(fi => !fi.IsStatic).ToArray();

            IEnumerable<Type> GetBaseTypesAndThis(Type t)
            {
                while (t != null)
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

                return t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                      BindingFlags.NonPublic);
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
        public static (object result, bool success) TryCreateInstanceSafe(Type t)
        {
            if (!CanCreateInstance(t))
            {
                return (result: null, success: false);
            }

            // Value types are handled separately
            if (t.IsValueType)
            {
                return Success(Activator.CreateInstance(t));
            }

            // String is handled separately as well due to security restrictions
            if (t == typeof(string))
            {
                return Success(string.Empty);
            }

            return Success(FormatterServices.GetUninitializedObject(t));

            (object result, bool success) Success(object o) => (o, true);
        }

        /// <summary>
        /// Returns true if the instance of type <paramref name="t"/> can be instantiated.
        /// </summary>
        public static bool CanCreateInstance(Type t)
        {
            // Abstract types and generics are not supported
            if (t.IsAbstract || IsOpenGenericType(t))
            {
                return false;
            }

            return true;
            bool IsOpenGenericType(Type type)
            {
                return type.IsGenericTypeDefinition && !type.IsConstructedGenericType;
            }
        }

        /// <summary>
        /// Returns true if a given type is unsafe.
        /// </summary>
        public static bool IsUnsafeValueType(this Type t)
        {
            return t.GetCustomAttribute(typeof(UnsafeValueTypeAttribute)) != null;
        }
    }
}