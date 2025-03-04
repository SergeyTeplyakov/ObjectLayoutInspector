using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectLayoutInspector
{
    internal static class Padder
    {
        public static void AddPaddings(bool includePaddings, int size, FieldLayout[] fieldsOffsets, List<FieldLayoutBase> layouts, Type type)
        {
            if (includePaddings)
            {
                var allBits = new BitArray(size);
                var dict = new Dictionary<Type, (List<FieldLayout> fields, BitArray usedBytes)>();

                foreach (var fieldOffset in fieldsOffsets)
                {
                    if (dict.TryGetValue(fieldOffset.DeclaringType, out var range))
                    {
                        range.fields.Add(fieldOffset);
                        range = (range.fields, range.usedBytes.SetRange(fieldOffset));
                    }
                    else
                    {
                        range = (new List<FieldLayout>() { fieldOffset }, new BitArray(size).SetRange(fieldOffset));
                    }

                    dict[fieldOffset.DeclaringType] = range;
                }

                foreach (var item in dict)
                {
                    var startPaddingSize = item.Value.usedBytes.GetRange(0);
                    if (startPaddingSize > 0)
                        layouts.Add(new Padding(0, startPaddingSize, item.Key));

                    foreach (var field in item.Value.fields)
                    {
                        layouts.Add(field);
                        var paddingSize = item.Value.usedBytes.GetRange(field.Offset + field.Size);
                        if (paddingSize > 0)
                            layouts.Add(new Padding(field.Offset + field.Size, paddingSize, item.Key));
                    }

                    allBits.Or(item.Value.usedBytes);
                }

                int start = -1, i = 0;
                for (; i < size; i++)
                {
                    var notSet = !allBits.Get(i);
                    if(notSet && start is -1)
                        start=i;
                    else if (!notSet && !(start is -1))
                    {
                        layouts.Add(new Padding(start, i - start, type));
                        start = -1;
                    }
                }
                if (!(start is -1))
                    layouts.Add(new Padding(start, size - start, type));
            }
            else
            {
                layouts.AddRange(fieldsOffsets);
            }
        }
    }
}