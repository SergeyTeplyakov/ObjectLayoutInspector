using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using ObjectLayoutInspector;
using ObjectLayoutInspector.Tests;

namespace ObjectLayoutInspector.Benchmarks
{
    [CoreJob]
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