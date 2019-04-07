using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ObjectLayoutInspector
{
    internal static class Detectors
    {
        public static bool IsFixed(FieldInfo fieldInfo, out int fixedBuffer)
        {
            var fixedCheck = fieldInfo
                             .CustomAttributes.Where(x => x.AttributeType.Equals(typeof(FixedBufferAttribute)))
                             .Select(x => x.ConstructorArguments)
                             .FirstOrDefault();
            fixedBuffer = fixedCheck != null ? (int)fixedCheck[1].Value : 0;
            return fixedBuffer != 0;
        }

        public static bool IsPrimitive(FieldInfo fieldInfo) =>
            fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType.IsEnum || fieldInfo.FieldType == typeof(decimal);

        public static bool IsNullable(FieldInfo fieldInfo) => IsNullable(fieldInfo.FieldType);
        public static bool IsNullable(Type type) =>
              type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);              
    }
}