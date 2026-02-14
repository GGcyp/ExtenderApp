using System.Diagnostics;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.MemoryBlocks;

namespace ExtenderApp.Test.Tests
{
    /// <summary>
    /// 一组用于验证二进制序列化以及缓冲区回收/冻结行为的简单测试用例。 这些测试是自包含的，直接使用传入的 <see cref="IBinarySerialization"/> 执行序列化/反序列化并检查 <see cref="AbstractBuffer{T}"/> 的
    /// TryRelease 行为。
    /// </summary>
    internal static class SerializationTests
    {
        public static void RunAll(IBinarySerialization binarySerialization)
        {
            if (binarySerialization == null) throw new ArgumentNullException(nameof(binarySerialization));

            Debug.Print("-- 序列化测试开始 --");
            TestPrimitiveLong(binarySerialization);
            TestStringRoundTrip(binarySerialization);
            TestLongStringRoundTrip(binarySerialization);
            TestGuidRoundTrip(binarySerialization);
            TestTimeSpanRoundTrip(binarySerialization);
            TestStringFreezeBehavior(binarySerialization);
            TestWriteFrozenBehavior(binarySerialization);
            TestBufferReuse(binarySerialization);
            RunAutomationTests(binarySerialization);
            TestCustomClassRoundTrip(binarySerialization);
            TestSliceBehavior();
            Debug.Print("-- 序列化测试结束 --");
        }

        private static void TestPrimitiveLong(IBinarySerialization binarySerialization)
        {
            const long expected = 1111111111L;
            binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
            try
            {
                var actual = binarySerialization.Deserialize<long>(buffer);
                if (actual != expected)
                    Debug.Print($"[失败] TestPrimitiveLong：期望 {expected}，实际 {actual}");
                else
                    Debug.Print("[通过] TestPrimitiveLong");

                var released = buffer.TryRelease();
                Debug.Print($"[回收] TestPrimitiveLong：{released}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                }
            }
        }

        private static void TestStringFreezeBehavior(IBinarySerialization binarySerialization)
        {
            const string test = "hello-serialization";
            binarySerialization.Serialize(test, out AbstractBuffer<byte> buffer);
            try
            {
                var value = binarySerialization.Deserialize<string>(buffer);
                Debug.Print(value == test ? "[通过] TestStringFreezeBehavior 反序列化" : "[失败] TestStringFreezeBehavior 反序列化");

                buffer.Freeze();
                buffer.Freeze();
                var releasedWhileFrozen = buffer.TryRelease();
                Debug.Print($"[冻结] TestStringFreezeBehavior TryRelease：{releasedWhileFrozen}（期望：False）");

                buffer.Unfreeze();
                var releasedAfterUnfreeze = buffer.TryRelease();
                Debug.Print($"[解冻] TestStringFreezeBehavior TryRelease：{releasedAfterUnfreeze}（期望：True）");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private static void TestWriteFrozenBehavior(IBinarySerialization binarySerialization)
        {
            var sample = new byte[] { 1, 2, 3, 4, 5 };
            binarySerialization.Serialize(sample, out AbstractBuffer<byte> buffer);
            try
            {
                buffer.FreezeWrite();
                var releasedWhileWriteFrozen = buffer.TryRelease();
                Debug.Print($"[写冻结] TestWriteFrozenBehavior TryRelease：{releasedWhileWriteFrozen}（期望：False）");

                buffer.UnfreezeWrite();
                var releasedAfterUnfreezeWrite = buffer.TryRelease();
                Debug.Print($"[解写冻结] TestWriteFrozenBehavior TryRelease：{releasedAfterUnfreezeWrite}（期望：True）");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private static void TestStringRoundTrip(IBinarySerialization binarySerialization)
        {
            const string expected = "序列化-反序列化-字符串";
            binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
            try
            {
                var actual = binarySerialization.Deserialize<string>(buffer);
                if (actual != expected)
                    Debug.Print($"[失败] TestStringRoundTrip：期望 {expected}，实际 {actual}");
                else
                    Debug.Print("[通过] TestStringRoundTrip");

                var released = buffer.TryRelease();
                Debug.Print($"[回收] TestStringRoundTrip：{released}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                }
            }
        }

        private static void TestLongStringRoundTrip(IBinarySerialization binarySerialization)
        {
            string expected = new string('测', 10000) + "-超长字符串-" + new string('试', 10000);
            binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
            try
            {
                var actual = binarySerialization.Deserialize<string>(buffer);
                if (actual != expected)
                    Debug.Print("[失败] TestLongStringRoundTrip：内容不一致");
                else
                    Debug.Print("[通过] TestLongStringRoundTrip");

                var released = buffer.TryRelease();
                var sprovider = DefaultSequenceBufferProvider<byte>.Shared;
                var mprovider = MemoryBlockProvider<byte>.Shared;
                Debug.Print($"[回收] TestLongStringRoundTrip：{released}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                }
            }
        }

        private static void TestGuidRoundTrip(IBinarySerialization binarySerialization)
        {
            Guid expected = Guid.NewGuid();
            binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
            try
            {
                var actual = binarySerialization.Deserialize<Guid>(buffer);
                if (actual != expected)
                    Debug.Print($"[失败] TestGuidRoundTrip：期望 {expected}，实际 {actual}");
                else
                    Debug.Print("[通过] TestGuidRoundTrip");

                var released = buffer.TryRelease();
                Debug.Print($"[回收] TestGuidRoundTrip：{released}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                }
            }
        }

        private static void TestTimeSpanRoundTrip(IBinarySerialization binarySerialization)
        {
            TimeSpan expected = TimeSpan.FromDays(1.5) + TimeSpan.FromMilliseconds(1234);
            binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
            try
            {
                var actual = binarySerialization.Deserialize<TimeSpan>(buffer);
                if (actual != expected)
                    Debug.Print($"[失败] TestTimeSpanRoundTrip：期望 {expected}，实际 {actual}");
                else
                    Debug.Print("[通过] TestTimeSpanRoundTrip");

                var released = buffer.TryRelease();
                Debug.Print($"[回收] TestTimeSpanRoundTrip：{released}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                }
            }
        }

        private static void RunAutomationTests(IBinarySerialization binarySerialization)
        {
            const int iterations = 50;
            int failed = 0;
            for (int i = 0; i < iterations; i++)
            {
                long expected = i * 1024L + 7;
                binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
                try
                {
                    var actual = binarySerialization.Deserialize<long>(buffer);
                    if (actual != expected)
                        failed++;
                }
                finally
                {
                    buffer.TryRelease();
                }
            }

            Debug.Print($"[自动化] RunAutomationTests 完成：总次数 {iterations}，失败 {failed}");
        }

        private static void TestBufferReuse(IBinarySerialization binarySerialization)
        {
            binarySerialization.Serialize(12345, out AbstractBuffer<byte> firstBuffer);
            var firstInstance = firstBuffer;
            var firstReleased = firstBuffer.TryRelease();
            Debug.Print($"[回收] TestBufferReuse 首次回收：{firstReleased}");

            binarySerialization.Serialize(67890, out AbstractBuffer<byte> secondBuffer);
            var reused = ReferenceEquals(firstInstance, secondBuffer);
            Debug.Print($"[复用] TestBufferReuse 是否复用同一实例：{reused}");
            if (!secondBuffer.TryRelease())
            {
                try { secondBuffer.UnfreezeWrite(); } catch { }
                try { secondBuffer.Unfreeze(); } catch { }
                secondBuffer.TryRelease();
            }
        }

        private static void TestCustomClassRoundTrip(IBinarySerialization binarySerialization)
        {
            var expected = new CustomTestModel
            {
                Id = 42,
                Name = "自定义类型",
                Token = Guid.NewGuid(),
                Duration = TimeSpan.FromMinutes(90)
            };

            binarySerialization.Serialize(expected, out AbstractBuffer<byte> buffer);
            try
            {
                var actual = binarySerialization.Deserialize<CustomTestModel>(buffer);
                bool same = actual != null && actual.Equals(expected);
                if (!same)
                    Debug.Print("[失败] TestCustomClassRoundTrip：内容不一致");
                else
                    Debug.Print("[通过] TestCustomClassRoundTrip");

                var released = buffer.TryRelease();
                Debug.Print($"[回收] TestCustomClassRoundTrip：{released}");
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private static void TestSliceBehavior()
        {
            var source = Enumerable.Range(0, 100).Select(value => (byte)value).ToArray();
            var buffer = DefaultSequenceBufferProvider<byte>.Shared.GetBuffer();
            try
            {
                int start = Random.Shared.Next(0, 100);
                int length = Random.Shared.Next(1, 100 - start);
                buffer.Write(source);
                Debug.Print($"[信息] TestSliceBehavior 创建切片：Start={start}, Length={length}");
                var slice = buffer.Slice(start, length);
                try
                {
                    bool same = slice.CommittedSequence.FirstSpan.SequenceEqual(source.AsSpan().Slice(start, length));
                    Debug.Print(same ? "[通过] TestSliceBehavior" : "[失败] TestSliceBehavior：切片内容不一致");
                }
                finally
                {
                    if (!slice.TryRelease())
                    {
                        try { slice.UnfreezeWrite(); } catch { }
                        try { slice.Unfreeze(); } catch { }
                        slice.TryRelease();
                    }
                }
            }
            finally
            {
                if (!buffer.TryRelease())
                {
                    try { buffer.UnfreezeWrite(); } catch { }
                    try { buffer.Unfreeze(); } catch { }
                    buffer.TryRelease();
                }
            }
        }

        private sealed class CustomTestModel : IEquatable<CustomTestModel>
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

            public override bool Equals(object? obj) => obj is CustomTestModel other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Id, Name, Token, Duration);
        }
    }
}