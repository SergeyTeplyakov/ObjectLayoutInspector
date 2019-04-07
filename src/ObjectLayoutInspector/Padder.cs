using System.Collections.Generic;

namespace ObjectLayoutInspector
{
    internal static class Padder
    {
        public static void AddPaddings(bool includePaddings, int size, FieldLayout[] fieldsOffsets, List<FieldLayoutBase> layouts)
        {
            if (includePaddings && fieldsOffsets.Length != 0 && fieldsOffsets[0].Offset != 0)
            {
                layouts.Add(new Padding(0, fieldsOffsets[0].Offset));
            }

            for (var index = 0; index < fieldsOffsets.Length; index++)
            {
                var fieldOffset = fieldsOffsets[index];
                layouts.Add(fieldOffset);

                if (includePaddings)
                {
                    int nextOffsetOrSize = size;
                    if (index != fieldsOffsets.Length - 1)
                    {
                        // This is not a last field.
                        nextOffsetOrSize = fieldsOffsets[index + 1].Offset;
                    }

                    var nextSectionOffsetCandidate = fieldOffset.Offset + fieldOffset.Size;
                    if (nextSectionOffsetCandidate < nextOffsetOrSize)
                    {
                        // we have padding
                        layouts.Add(new Padding(nextSectionOffsetCandidate, nextOffsetOrSize - nextSectionOffsetCandidate));
                    }
                }
            }
        }
    }
}