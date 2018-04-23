using System;
using System.Collections.Generic;

namespace ObjectLayoutInspector
{
    internal static class EnumerableExtensions
    {
        public static T MaxBy<T>(this IEnumerable<T> sequence, Func<T, int> selector)
        {
            bool firstElement = false;
            T maxValue = default(T);
            foreach (var e in sequence)
            {
                if (!firstElement)
                {
                    firstElement = true;
                    maxValue = e;
                }
                else
                {
                    var currentMax = selector(maxValue);
                    var maxCandidate = selector(e);

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