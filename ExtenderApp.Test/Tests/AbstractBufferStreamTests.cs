using System.Diagnostics;
using ExtenderApp.Buffer;

namespace ExtenderApp.Test.Tests
{
    /// <summary>
    /// Tests for `AbstractBufferStream` covering basic read/write, position and disposal behavior.
    /// These are lightweight checks that throw on failure and print results via Debug.
    /// </summary>
    public static class AbstractBufferStreamTests
    {
        /// <summary>
        /// Run all tests in this class.
        /// </summary>
        public static void RunAll()
        {
            try
            {
                TestWriteAndRead();
                TestPositionAfterWrite();
                TestReadBeyondEnd();
                TestDisposeBehavior();

                TestMultipleWritesAndReads();
                TestOverwriteMiddle();
                TestSeekAndRead();
                TestBufferVisibility();

                Debug.Print("AbstractBufferStreamTests: All tests passed.");
            }
            catch (Exception ex)
            {
                Debug.Print($"AbstractBufferStreamTests: Exception: {ex}");
                throw;
            }
        }

        private static void TestWriteAndRead()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(128);
            try
            {
                var writer = new AbstractBufferStream(buffer);
                var data = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();
                writer.Write(data, 0, data.Length);

                // create a new stream to read from the beginning
                var reader = new AbstractBufferStream(buffer);
                var outBuf = new byte[data.Length];
                int read = reader.Read(outBuf, 0, outBuf.Length);
                if (read != data.Length)
                    throw new InvalidOperationException($"TestWriteAndRead: expected {data.Length} bytes, got {read}");

                for (int i = 0; i < data.Length; i++)
                {
                    if (outBuf[i] != data[i])
                        throw new InvalidOperationException($"TestWriteAndRead: data mismatch at index {i}, expected {data[i]}, got {outBuf[i]}");
                }

                // cleanup
                reader.Dispose();
                writer.Dispose();
            }
            finally
            {
                // try to release buffer if possible
                buffer.TryRelease();
            }

            Debug.Print("TestWriteAndRead: OK");
        }

        private static void TestPositionAfterWrite()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(64);
            try
            {
                var stream = new AbstractBufferStream(buffer);
                var data = new byte[] { 10, 20, 30, 40 };
                stream.Write(data, 0, data.Length);

                if (stream.Position != buffer.Committed)
                    throw new InvalidOperationException($"TestPositionAfterWrite: Position ({stream.Position}) != Committed ({buffer.Committed})");

                stream.Dispose();
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestPositionAfterWrite: OK");
        }

        private static void TestReadBeyondEnd()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(32);
            try
            {
                var writer = new AbstractBufferStream(buffer);
                var bytes = new byte[] { 1, 2, 3 };
                writer.Write(bytes, 0, bytes.Length);

                var reader = new AbstractBufferStream(buffer);
                var dest = new byte[10];
                int read = reader.Read(dest, 0, dest.Length);

                if (read != bytes.Length)
                    throw new InvalidOperationException($"TestReadBeyondEnd: expected read {bytes.Length}, got {read}");

                // verify the bytes that were read match the source
                for (int i = 0; i < read; i++)
                {
                    if (dest[i] != bytes[i])
                        throw new InvalidOperationException($"TestReadBeyondEnd: data mismatch at {i}, expected {bytes[i]}, got {dest[i]}");
                }

                reader.Dispose();
                writer.Dispose();
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestReadBeyondEnd: OK");
        }

        private static void TestDisposeBehavior()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(16);
            var stream = new AbstractBufferStream(buffer);
            stream.Dispose();

            try
            {
                // operations after dispose should throw ObjectDisposedException
                var tmp = new byte[4];
                try
                {
                    stream.Read(tmp, 0, tmp.Length);
                    throw new InvalidOperationException("TestDisposeBehavior: Read did not throw after dispose.");
                }
                catch (ObjectDisposedException)
                {
                    // expected
                }

                try
                {
                    stream.Write(tmp, 0, tmp.Length);
                    throw new InvalidOperationException("TestDisposeBehavior: Write did not throw after dispose.");
                }
                catch (ObjectDisposedException)
                {
                    // expected
                }
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestDisposeBehavior: OK");
        }

        // New tests below

        private static void TestMultipleWritesAndReads()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(256);
            try
            {
                var writer = new AbstractBufferStream(buffer);
                var a = Enumerable.Range(1, 50).Select(i => (byte)i).ToArray();
                var b = Enumerable.Range(51, 50).Select(i => (byte)i).ToArray();

                writer.Write(a, 0, a.Length);
                writer.Write(b, 0, b.Length);

                // read back all
                var reader = new AbstractBufferStream(buffer);
                var outBuf = new byte[a.Length + b.Length];
                int read = reader.Read(outBuf, 0, outBuf.Length);
                if (read != outBuf.Length)
                    throw new InvalidOperationException($"TestMultipleWritesAndReads: expected {outBuf.Length}, got {read}");

                for (int i = 0; i < a.Length; i++)
                {
                    if (outBuf[i] != a[i])
                        throw new InvalidOperationException("TestMultipleWritesAndReads: mismatch in first segment");
                }
                for (int i = 0; i < b.Length; i++)
                {
                    if (outBuf[a.Length + i] != b[i])
                        throw new InvalidOperationException("TestMultipleWritesAndReads: mismatch in second segment");
                }

                reader.Dispose();
                writer.Dispose();
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestMultipleWritesAndReads: OK");
        }

        private static void TestOverwriteMiddle()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(64);
            try
            {
                var stream = new AbstractBufferStream(buffer);
                var initial = new byte[] { 1, 2, 3, 4, 5 };
                stream.Write(initial, 0, initial.Length);

                // seek to offset 2 and overwrite two bytes
                stream.Seek(2, System.IO.SeekOrigin.Begin);
                var over = new byte[] { 9, 9 };
                stream.Write(over, 0, over.Length);

                // read back
                var reader = new AbstractBufferStream(buffer);
                var outBuf = new byte[initial.Length];
                int read = reader.Read(outBuf, 0, outBuf.Length);
                if (read != initial.Length)
                    throw new InvalidOperationException($"TestOverwriteMiddle: expected {initial.Length}, got {read}");

                var expected = new byte[] { 1, 2, 9, 9, 5 };
                for (int i = 0; i < expected.Length; i++)
                {
                    if (outBuf[i] != expected[i])
                        throw new InvalidOperationException($"TestOverwriteMiddle: at {i} expected {expected[i]} got {outBuf[i]}");
                }

                reader.Dispose();
                stream.Dispose();
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestOverwriteMiddle: OK");
        }

        private static void TestSeekAndRead()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(128);
            try
            {
                var writer = new AbstractBufferStream(buffer);
                var data = Enumerable.Range(1, 20).Select(i => (byte)i).ToArray();
                writer.Write(data, 0, data.Length);

                var reader = new AbstractBufferStream(buffer);
                reader.Seek(4, System.IO.SeekOrigin.Begin); // move to 5th byte (value 5)
                var outBuf = new byte[3];
                int read = reader.Read(outBuf, 0, outBuf.Length);
                if (read != 3)
                    throw new InvalidOperationException($"TestSeekAndRead: expected 3 bytes, got {read}");
                if (outBuf[0] != 5 || outBuf[1] != 6 || outBuf[2] != 7)
                    throw new InvalidOperationException("TestSeekAndRead: values mismatch after seek");

                reader.Dispose();
                writer.Dispose();
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestSeekAndRead: OK");
        }

        private static void TestBufferVisibility()
        {
            var buffer = AbstractBuffer.GetBlock<byte>(64);
            try
            {
                var stream = new AbstractBufferStream(buffer);
                var data = new byte[] { 11, 22, 33 };
                stream.Write(data, 0, data.Length);

                // underlying buffer should reflect committed bytes
                if (buffer.Committed != data.Length)
                    throw new InvalidOperationException($"TestBufferVisibility: expected committed {data.Length}, got {buffer.Committed}");

                var arr = buffer.ToArray();
                for (int i = 0; i < data.Length; i++)
                {
                    if (arr[i] != data[i])
                        throw new InvalidOperationException($"TestBufferVisibility: data mismatch at {i}");
                }

                stream.Dispose();
            }
            finally
            {
                buffer.TryRelease();
            }

            Debug.Print("TestBufferVisibility: OK");
        }
    }
}