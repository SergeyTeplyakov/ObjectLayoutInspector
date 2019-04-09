using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests.RunManually
{
    [TestFixture]
    public class LayoutsForAllKnownTypes
    {
        struct SizeComputer<T>
        {
            public T field1;
            public T field2;

            public SizeComputer(T field1, T field2) => (this.field1, this.field2) = (field1, field2);
        }

        struct Test1
        {
            public byte b; public double d; public byte b2; public double d2; public byte b3;

            public Test1(byte b, double d, byte b2, double d2, byte b3) => (this.b, this.d, this.b2, this.d2, this.b3) = (b, d, b2, d2, b3);
        }

        struct Test2
        {
            public double d; public double d2; public byte b; public byte b2; public byte b3;
            public Test2(byte b, double d, byte b2, double d2, byte b3) => (this.d, this.d2, this.b, this.b2, this.b3) = (d, d2, b, b2, b3);
        }

        [Test]
        public void GetLayoutForTwoSpecificTypes()
        {
            TypeLayout.PrintLayout(typeof(Test1));
            TypeLayout.PrintLayout(typeof(Test2));
        }

        //[Test]
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
                Console.WriteLine(t);
            }

            Console.WriteLine();
            Console.WriteLine("Top 10 types with biggest paddings:");
            foreach (var t in top10WithBiggestPaddings)
            {
                Console.WriteLine(t);
            }
        }

        private IEnumerable<Type> GetAllLoadedTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
        }
    }
}