using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BenchRunner
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class BinarySerializationBenchmarks
    {
        private BenchRunnerServiceProvider? _provider;
        private IBinarySerialization? _serialization;
        private MessagePackSerializerOptions _mpOptions = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);
        private bool _initialized = false;

        private CustomTestModel _sampleObject = default!;
        private byte[] _sampleBytes = default!;
        private string _sampleString = string.Empty;

        // buffer created for the deserialize benchmark per iteration
        private AbstractBuffer<byte>? _deserializeBuffer;

        private byte[]? _mpBytes;
        private byte[]? _binaryBytes;

        [GlobalSetup]
        public void GlobalSetup()
        {
            try
            {
                _provider = new BenchRunnerServiceProvider();
                _serialization = _provider.ServiceProvider.GetService<IBinarySerialization>();
                if (_serialization == null)
                    throw new InvalidOperationException("IBinarySerialization is not available from service provider.");

                _sampleObject = new CustomTestModel
                {
                    Id = 123,
                    Name = "Benchmark-自定义类型-测试",
                    Token = Guid.NewGuid(),
                    Duration = TimeSpan.FromMinutes(90)
                };

                _sampleBytes = new byte[1024];
                Random.Shared.NextBytes(_sampleBytes);

                _sampleString = new string('测', 1024);

                // Pre-warm/initialize formatters and JIT for both serialization implementations
                // Warm-up for custom binary serialization
                _serialization.Serialize(_sampleObject, out var warmBuf);
                try
                {
                    var _ = _serialization.Deserialize<CustomTestModel>(warmBuf);
                }
                finally
                {
                    try { warmBuf.TryRelease(); } catch { }
                }

                // Warm-up for MessagePack (use contractless resolver)
                _mpBytes = MessagePackSerializer.Serialize(_sampleObject, _mpOptions);
                var tmp = MessagePackSerializer.Deserialize<CustomTestModel>(_mpBytes, _mpOptions);

                // Prepare reusable buffers for Deserialize benchmarks to avoid per-iteration setup cost
                _serialization.Serialize(_sampleObject, out _deserializeBuffer);
                // prepare contiguous binary bytes for SpanReader-based deserialize benchmark
                _binaryBytes = _serialization.Serialize(_sampleObject);
                // prepare MessagePack bytes (already set to _mpBytes)

                _initialized = true;
            }
            catch (Exception ex)
            {
                _initialized = false;
                Console.WriteLine("GlobalSetup exception in BinarySerializationBenchmarks:");
                Console.WriteLine(ex.ToString());
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            try { _provider?.Dispose(); } catch { }
            try { _deserializeBuffer?.TryRelease(); } catch { }
        }

        [Benchmark(Description = "Serialize<Object> -> AbstractBuffer")]
        public void Serialize_Object_To_AbstractBuffer()
        {
            var serialization = _serialization!;
            serialization.Serialize(_sampleObject, out AbstractBuffer<byte> buffer);
            // release buffer to avoid memory leaks
            try { buffer.TryRelease(); } catch { }
        }

        [Benchmark(Description = "Serialize<byte[]> -> AbstractBuffer")]
        public void Serialize_ByteArray_To_AbstractBuffer()
        {
            var serialization = _serialization!;
            serialization.Serialize(_sampleBytes, out AbstractBuffer<byte> buffer);
            try { buffer.TryRelease(); } catch { }
        }

        [Benchmark(Description = "Serialize<string> -> AbstractBuffer")]
        public void Serialize_String_To_AbstractBuffer()
        {
            var serialization = _serialization!;
            serialization.Serialize(_sampleString, out AbstractBuffer<byte> buffer);
            try { buffer.TryRelease(); } catch { }
        }

        [Benchmark(Description = "Deserialize<AbstractBuffer> -> Object")]
        public CustomTestModel? Deserialize_AbstractBuffer_To_Object()
        {
            try
            {
                var serialization = _serialization!;
                // _deserializeBuffer is prepared in IterationSetup
                var buf = _deserializeBuffer!;
                var obj = serialization.Deserialize<CustomTestModel>(buf);
                return obj;
            }
            catch (Exception ex)
            {
                // Log exception so BenchmarkDotNet output doesn't silently mark this as NA without details
                Console.WriteLine($"Deserialize benchmark exception: {ex}");
                return null;
            }
        }

        [Benchmark(Description = "Deserialize<byte[]> -> Object (SpanReader)")]
        public CustomTestModel? Deserialize_Bytes_SpanReader_To_Object()
        {
            try
            {
                var serialization = _serialization!;
                var bytes = _binaryBytes!; // prepared in GlobalSetup
                var reader = new SpanReader<byte>(bytes);
                return serialization.Deserialize<CustomTestModel>(ref reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SpanReader deserialize exception: {ex}");
                return null;
            }
        }

        // MessagePack benchmarks
        [Benchmark(Description = "MessagePack Serialize<Object> -> byte[]")]
        public void MessagePack_Serialize_Object()
        {
            var bytes = MessagePackSerializer.Serialize(_sampleObject, _mpOptions);
            // keep bytes alive briefly
            if (bytes == null) throw new InvalidOperationException();
        }

        [Benchmark(Description = "MessagePack Deserialize<byte[]> -> Object")]
        public CustomTestModel? MessagePack_Deserialize_Bytes_To_Object()
        {
            try
            {
                var bytes = _mpBytes!; // prepared in IterationSetup
                return MessagePackSerializer.Deserialize<CustomTestModel>(bytes, _mpOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MessagePack deserialize exception: {ex}");
                return null;
            }
        }

        [Benchmark(Description = "Serialize<Object> -> SpanWriter")]
        public void Serialize_Object_To_SpanWriter()
        {
            var serialization = _serialization!;
            int len = (int)serialization.GetLength(_sampleObject);
            var block = MemoryBlock<byte>.GetBuffer(len);
            Span<byte> span = block.GetSpan(len);
            var writer = new SpanWriter<byte>(span);
            serialization.Serialize(_sampleObject, ref writer);
            block.TryRelease(); // release buffer after use
        }

        [Benchmark(Description = "Serialize<byte[]> -> SpanWriter")]
        public void Serialize_ByteArray_To_SpanWriter()
        {
            var serialization = _serialization!;
            int len = (int)serialization.GetLength(_sampleBytes);
            var block = MemoryBlock<byte>.GetBuffer(len);
            Span<byte> span = block.GetSpan(len);
            var writer = new SpanWriter<byte>(span);
            serialization.Serialize(_sampleObject, ref writer);
            block.TryRelease(); // release buffer after use
        }

        [Benchmark(Description = "Serialize<string> -> SpanWriter")]
        public void Serialize_String_To_SpanWriter()
        {
            var serialization = _serialization!;
            int len = (int)serialization.GetLength(_sampleString);
            var block = MemoryBlock<byte>.GetBuffer(len);
            Span<byte> span = block.GetSpan(len);
            var writer = new SpanWriter<byte>(span);
            serialization.Serialize(_sampleObject, ref writer);
            block.TryRelease(); // release buffer after use
        }

        public sealed class CustomTestModel : IEquatable<CustomTestModel>
        {
            public int Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public Guid Token { get; init; }
            public TimeSpan Duration { get; init; }

            public bool Equals(CustomTestModel? other)
            {
                if (other is null) return false;
                return Id == other.Id && Name == other.Name && Token == other.Token && Duration == other.Duration;
            }
        }
    }
}