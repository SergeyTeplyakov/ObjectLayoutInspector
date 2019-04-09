using System;
using System.Collections.Generic;

// It is not clear how to write MaxBy with in non-nullable context. Technically, this method is null-invariant.
#nullable disable

namespace ObjectLayoutInspector
{
    internal static class EnumerableExtensions
    {
        public static T MaxBy<T>(this IEnumerable<T> sequence, Func<T, int> selector)
        {
            bool firstElement = false;
            T maxValue = default(T);
            foreach (T e in sequence)
            {
                if (!firstElement)
                {
                    firstElement = true;
                    maxValue = e;
                }
                else
                {
                    int currentMax = selector(maxValue);
                    int maxCandidate = selector(e);

                    if (Math.Max(currentMax, maxCandidate) == maxCandidate)
                    {
                        maxValue = e;
                    }
                }
            }

            return maxValue;
        }
    }
}