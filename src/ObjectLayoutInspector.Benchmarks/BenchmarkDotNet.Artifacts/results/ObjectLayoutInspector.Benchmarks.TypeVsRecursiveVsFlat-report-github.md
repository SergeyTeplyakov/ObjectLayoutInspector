``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview-009812
  [Host] : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  Core   : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|                              Method |     Mean |     Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------------------ |---------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|                    TypeLayoutStruct | 5.031 ms | 0.1232 ms | 0.1152 ms |     46.8750 |     23.4375 |           - |           149.12 KB |
|            TypeLayoutPaddingsStruct | 5.008 ms | 0.0681 ms | 0.0569 ms |     46.8750 |     23.4375 |           - |           149.12 KB |
|                  UnsafeLayoutStruct | 1.067 ms | 0.0065 ms | 0.0058 ms |    148.4375 |           - |           - |           459.23 KB |
|          UnsafeLayoutPaddingsStruct | 1.092 ms | 0.0067 ms | 0.0063 ms |    148.4375 |           - |           - |           459.59 KB |
| UnsafeLayoutPaddingsRecursiveStruct | 1.110 ms | 0.0221 ms | 0.0410 ms |    171.8750 |           - |           - |           530.68 KB |
