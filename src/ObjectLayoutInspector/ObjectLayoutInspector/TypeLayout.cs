using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectLayoutInspector
{
    public struct TypeLayout
    {
        public int Size { get; }
        public int Paddings { get; }
        public FieldLayoutBase[] Fields { get; }

        public TypeLayout(int size, FieldLayoutBase[] fields)
        {
            Size = size;
            Fields = fields;
            Paddings = fields.OfType<Padding>().Sum(p => p.Size);
        }

        public static void PrintLayout<T>(bool recursively = true)
        {
            InspectorHelper.Print<T>(recursively);
        }

        public static TypeLayout GetLayout<T>(bool includePaddings = true)
        {
            return GetLayout(typeof(T), includePaddings);
        }

        public static TypeLayout GetLayout(Type type, bool includePaddings = true)
        {
            var (size, overhead) = InspectorHelper.GetSize(type);

            var fieldsOffsets = InspectorHelper.GetFieldOffsets(type);
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
    }
}