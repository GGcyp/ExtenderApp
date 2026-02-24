// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using BenchRunner;

Console.WriteLine("Hello, BenchRunner!");

BenchmarkRunner.Run<BinarySerializationBenchmarks>();

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();