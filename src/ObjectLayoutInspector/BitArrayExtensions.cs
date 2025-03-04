using System.Collections;

namespace ObjectLayoutInspector
{
    internal static class BitArrayExtensions
    {
        internal static BitArray SetRange(this BitArray bitArray, FieldLayoutBase fieldLayout, int additionalOffset = 0)
        {
            for (int i = fieldLayout.Offset; i < fieldLayout.Offset + fieldLayout.Size; i++)
            {
                bitArray.Set(i + additionalOffset, true);
            }
            return bitArray;
        }

        internal static int GetRange(this BitArray bitArray, int offset)
        {
            int size = 0;

            while (offset < bitArray.Length)
            {
                if (bitArray.Get(offset))
                    break;
                bitArray.Set(offset, true);
                offset++;
                size++;
            }

            return size;
        }

        internal static int GetSetCount(this BitArray bitArray)
        {
            int count = 0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray.Get(i))
                    count++;
            }
            return count;
        }
    }
}
