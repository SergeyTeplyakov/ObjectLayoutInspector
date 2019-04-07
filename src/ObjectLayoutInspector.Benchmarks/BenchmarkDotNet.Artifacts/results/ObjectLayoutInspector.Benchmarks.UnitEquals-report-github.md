``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview-009812
  [Host] : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  Core   : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|            Method |              Mean |          Error |         StdDev |            Median | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------ |------------------:|---------------:|---------------:|------------------:|------------:|------------:|------------:|--------------------:|
| TypeLayoutComplex | 5,337,636.6629 ns | 93,278.0142 ns | 82,688.5299 ns | 5,333,125.3906 ns |     46.8750 |     23.4375 |           - |            152703 B |
|         op_equals |         0.0000 ns |      0.0000 ns |      0.0000 ns |         0.0000 ns |           - |           - |           - |                   - |
|              Unit |         0.0012 ns |      0.0040 ns |      0.0045 ns |         0.0000 ns |           - |           - |           - |                   - |
