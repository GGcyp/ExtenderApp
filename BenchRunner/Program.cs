// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using BenchRunner;
using Microsoft.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using System.Reflection;

Console.WriteLine("Hello, BenchRunner!");

if (args.Length > 0 && args[0].Equals("diagnose", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Running diagnostic for Serialize via SpanWriter...");
    try
    {
        using var provider = new BenchRunnerServiceProvider();
        var serviceProvider = provider.ServiceProvider;
        var serialization = serviceProvider.GetService<IBinarySerialization>();
        if (serialization == null)
        {
            Console.WriteLine("IBinarySerialization not available from service provider.");
            return;
        }

        // Try to locate the nested CustomTestModel type used by the benchmarks
        var asm = Assembly.GetExecutingAssembly();
        var modelType = asm.GetType("BenchRunner.BinarySerializationBenchmarks+CustomTestModel");
        if (modelType == null)
        {
            Console.WriteLine("Could not find CustomTestModel type in assembly.");
            return;
        }

        // Create an instance and populate properties via reflection
        var instance = Activator.CreateInstance(modelType)!;
        void SetProp(string name, object? value)
        {
            var pi = modelType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null && pi.CanWrite) pi.SetValue(instance, value);
        }
        SetProp("Id", 123);
        SetProp("Name", "Diagnostic-自定义类型-测试");
        SetProp("Token", Guid.NewGuid());
        SetProp("Duration", TimeSpan.FromMinutes(90));

        // Invoke GetLength<T>(T) via reflection
        var getLengthMethod = typeof(IBinarySerialization).GetMethod("GetLength")!.MakeGenericMethod(modelType);
        var lenObj = getLengthMethod.Invoke(serialization, new object[] { instance });
        Console.WriteLine($"GetLength returned: {lenObj}");

        // As SpanWriter is a ref struct (cannot be boxed), invoke the SequenceBuffer-based Serialize(T, out SequenceBuffer<byte>)
        try
        {
            var serializeMethods = serialization.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == "Serialize" && m.IsGenericMethodDefinition);

            MethodInfo? target = null;
            foreach (var m in serializeMethods)
            {
                var ps = m.GetParameters();
                if (ps.Length == 2 && ps[1].ParameterType.IsByRef)
                {
                    var elem = ps[1].ParameterType.GetElementType();
                    if (elem != null && elem.IsGenericType && elem.GetGenericTypeDefinition().Name.Contains("SequenceBuffer"))
                    {
                        target = m;
                        break;
                    }
                }
            }

            if (target == null)
            {
                Console.WriteLine("Could not locate Serialize(T, out SequenceBuffer<byte>) method on serialization instance.");
                return;
            }

            var genericSerialize = target.MakeGenericMethod(modelType);
            object? bufferPlaceholder = null;
            object[] parameters = new object[] { instance, bufferPlaceholder };
            try
            {
                genericSerialize.Invoke(serialization, parameters);
                var buffer = parameters[1];
                Console.WriteLine(buffer == null ? "Serialize returned null buffer" : $"Serialize returned buffer of type: {buffer.GetType().FullName}");
            }
            catch (TargetInvocationException tie)
            {
                Console.WriteLine("Serialize threw TargetInvocationException:");
                Console.WriteLine(tie.InnerException?.ToString() ?? tie.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Serialize diagnostic failed:");
            Console.WriteLine(ex.ToString());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Diagnostic failed:");
        Console.WriteLine(ex.ToString());
    }

    return;
}

BenchmarkRunner.Run<BinarySerializationBenchmarks>();

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
