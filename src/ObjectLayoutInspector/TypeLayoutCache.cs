using System;
using System.Collections.Concurrent;

namespace ObjectLayoutInspector
{
    /// <summary>
    /// Thread-safe cache for type layouts.
    /// </summary>
    public sealed class TypeLayoutCache
    {
        internal readonly ConcurrentDictionary<Type, TypeLayout> LayoutCache = new ConcurrentDictionary<Type, TypeLayout>();

        private TypeLayoutCache()
        {
        }

        internal static TypeLayoutCache Create()
        {
            return new TypeLayoutCache();
        }
    }
}