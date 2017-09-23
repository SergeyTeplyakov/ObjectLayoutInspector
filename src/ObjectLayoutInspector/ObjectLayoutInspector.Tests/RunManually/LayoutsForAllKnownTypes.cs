using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests.RunManually
{
    [TestFixture]
    public class LayoutsForAllKnownTypes
    {
        struct SizeComputer<T>
        {
#pragma warning disable 0649 // Unassigned field
#pragma warning disable 169 // Unused field
            public T field1;
            public T field2;
#pragma warning restore 169 // Unused field
#pragma warning restore 0649 // Unassigned field
        }

        [Test]
        public void GetLayoutsForAllTypes()
        {
            //var xx = new SizeComputer<RSAOAEPKeyExchangeDeformatter>();
            var cache = TypeLayoutCache.Create();
            var layouts = GetAllLoadedTypes()
                //.Where(t => t.Assembly.FullName.Contains("mscorlib"))
                .Select(t => TypeLayout.TryGetLayout(t, cache))
                .Where(t => t != null)
                .Select(t => t.Value)
                .ToList();

            var top10BiggestInstances = layouts.OrderByDescending(l => l.Size).Take(20);

            var top10WithBiggestPaddings = layouts.OrderByDescending(l => l.Paddings).Take(20);

            Console.WriteLine("Top 10 biggest types:");
            foreach (var t in top10BiggestInstances)
            {
                t.PrintLayout();
            }

            Console.WriteLine();
            Console.WriteLine("Top 10 types with biggest paddings:");
            foreach (var t in top10WithBiggestPaddings)
            {
                t.PrintLayout();
            }
        }

        private IEnumerable<Type> GetAllLoadedTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
        }
    }
}