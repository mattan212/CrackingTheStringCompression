``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.677 (1803/April2018Update/Redstone4)
Intel Core i7-4720HQ CPU 2.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=2533209 Hz, Resolution=394.7562 ns, Timer=TSC
.NET Core SDK=3.0.100-preview3-010431
  [Host]     : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT


```
|                   Method |       Mean |     Error |    StdDev |
|------------------------- |-----------:|----------:|----------:|
|            BruteCompress | 130.862 ms | 0.7493 ms | 0.6257 ms |
|          BuilderCompress |   2.044 ms | 0.0409 ms | 0.0401 ms |
|         ThreadedCompress |   1.260 ms | 0.0130 ms | 0.0116 ms |
|         ParallelCompress |   1.328 ms | 0.0156 ms | 0.0146 ms |
| ParallelThreadedCompress |   1.665 ms | 0.0188 ms | 0.0167 ms |
