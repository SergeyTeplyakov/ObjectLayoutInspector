using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ObjectLayoutInspector.Tests;

namespace ObjectLayoutInspector.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    public class TypeVsRecursiveVsFlat
    {
        [Benchmark]
        public void TypeLayoutStruct()
        {
            TypeLayout.GetLayout<VeryVeryComplexStruct>();
        }

        [Benchmark]
        public void TypeLayoutPaddingsStruct()
        {
            TypeLayout.GetLayout<VeryVeryComplexStruct>(includePaddings: true);
        }

        [Benchmark]
        public void UnsafeLayoutStruct()
        {
            UnsafeLayout.GetFieldsLayout<VeryVeryComplexStruct>(recursive: false);
        }

        [Benchmark]
        public void UnsafeLayoutPaddingsStruct()
        {
            UnsafeLayout.GetLayout<VeryVeryComplexStruct>(recursive: false);
        }

        [Benchmark]
        public void UnsafeLayoutPaddingsRecursiveStruct()
        {
            UnsafeLayout.GetLayout<VeryVeryComplexStruct>(recursive: true);
        }
    }
}