``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview-009812
  [Host] : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  Core   : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|                              Method |       Mean |      Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------------------ |-----------:|-----------:|----------:|------------:|------------:|------------:|--------------------:|
|                    TypeLayoutStruct | 6,535.6 us |  98.409 us | 92.052 us |     46.8750 |     23.4375 |           - |           149.12 KB |
|            TypeLayoutPaddingsStruct | 6,614.8 us | 112.103 us | 99.376 us |     46.8750 |     23.4375 |           - |           149.12 KB |
|                  UnsafeLayoutStruct |   889.1 us |   7.178 us |  6.363 us |    172.8516 |           - |           - |           531.18 KB |
|          UnsafeLayoutPaddingsStruct |   884.5 us |   6.987 us |  6.536 us |    172.8516 |           - |           - |            531.5 KB |
| UnsafeLayoutPaddingsRecursiveStruct |   917.8 us |   8.702 us |  8.140 us |    172.8516 |           - |           - |           532.48 KB |
