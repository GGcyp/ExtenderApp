using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 非托管结构体的二进制格式化器基类，支持高效读写基础数值类型及结构体本体。
    /// </summary>
    /// <typeparam name="T">目标结构体类型，必须为 unmanaged。</typeparam>
    internal class UnManagedFornatter<T> : BinaryFormatter<T> where T : unmanaged
    {
        public override int DefaultLength { get; }

        /// <summary>
        /// 初始化格式化器并指定二进制选项。
        /// </summary>
        /// <param name="options">二进制编码选项。</param>
        protected UnManagedFornatter(BinaryOptions options) : base(options)
        {
            DefaultLength = GetLength<T>();
        }

        public override T Deserialize(ref ByteBuffer buffer)
        {
            return Read(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, T value)
        {
            Write(ref buffer, Unsafe.As<T, long>(ref value));
        }

        public override long GetLength(T value)
        {
            return GetLength<T>();
        }

        /// <summary>
        /// 从缓冲区读取结构体（按类型标记和紧凑数值长度推断），返回结构体实例。
        /// </summary>
        /// <param name="buffer">字节缓冲区。</param>
        /// <returns>读取到的结构体实例。</returns>
        /// <exception cref="InvalidDataException">遇到未知类型标记时抛出。</exception>
        protected T Read(ref ByteBuffer buffer)
        {
            byte mark = buffer.Read();
            int length = 0;

            if (mark == Options.Int8) length = GetSize<SByte>();
            else if (mark == Options.UInt8) length = GetSize<Byte>();
            else if (mark == Options.Int16) length = GetSize<Int16>();
            else if (mark == Options.UInt16) length = GetSize<UInt16>();
            else if (mark == Options.Int32) length = GetSize<Int32>();
            else if (mark == Options.UInt32) length = GetSize<UInt32>();
            else if (mark == Options.Int64) length = GetSize<Int64>();
            else if (mark == Options.UInt64) length = GetSize<UInt64>();
            else throw new InvalidDataException($"未知标识: {mark}");

            Span<byte> temp = stackalloc byte[length - 1];
            buffer.Read(temp);
            return Read(temp);
        }

        /// <summary>
        /// 从字节序列读取结构体实例（未做字节序转换）。
        /// </summary>
        /// <param name="span">结构体字节序列。</param>
        /// <returns>结构体实例。</returns>
        private T Read(ReadOnlySpan<byte> span)
        {
            T value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
            return value;
        }

        /// <summary>
        /// 将数值按最紧凑类型编码写入缓冲区，并写入类型标记。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="value">待写入的数值。</param>
        protected void Write(ref ByteBuffer buffer, long value)
        {
            int length = GetLength(value, out var mark);
            buffer.Write(mark);

            if (mark == Options.Int8 || mark == Options.UInt8) buffer.Write((byte)value);
            else if (mark == Options.Int16) buffer.Write((short)value);
            else if (mark == Options.UInt16) buffer.Write((ushort)value);
            else if (mark == Options.Int32) buffer.Write((int)value);
            else if (mark == Options.UInt64) buffer.Write((ulong)value);
            else if (mark == Options.Int64) buffer.Write(value);
            else buffer.Write((ulong)value);
        }

        /// <summary>
        /// 获取 long 类型按最紧凑编码的长度及类型标记。
        /// </summary>
        /// <param name="value">待编码的数值。</param>
        /// <param name="mark">输出：类型标记。</param>
        /// <returns>编码所需字节数（含类型标记）。</returns>
        protected int GetLength(long value, out byte mark)
        {
            if (value > 0)
            {
                return GetLength((ulong)value, out mark);
            }
            else if (value > SByte.MinValue)
            {
                mark = Options.Int8;
                return GetLength<SByte>();
            }
            else if (value > Int16.MinValue)
            {
                mark = Options.Int16;
                return GetLength<Int16>();
            }
            else if (value > Int32.MinValue)
            {
                mark = Options.Int32;
                return GetLength<Int32>();
            }
            else
            {
                mark = Options.Int64;
                return GetLength<Int64>();
            }
        }

        /// <summary>
        /// 获取 ulong 类型按最紧凑编码的长度及类型标记。
        /// </summary>
        /// <param name="value">待编码的数值。</param>
        /// <param name="mark">输出：类型标记。</param>
        /// <returns>编码所需字节数（含类型标记）。</returns>
        protected int GetLength(ulong value, out byte mark)
        {
            if (value <= Byte.MaxValue)
            {
                mark = Options.UInt8;
                return GetLength<Byte>();
            }
            else if (value <= UInt16.MaxValue)
            {
                mark = Options.UInt16;
                return GetLength<UInt16>();
            }
            else if (value <= UInt32.MaxValue)
            {
                mark = Options.UInt32;
                return GetLength<UInt32>();
            }
            else
            {
                mark = Options.UInt64;
                return GetLength<UInt64>();
            }
        }

        /// <summary>
        /// 获取指定类型编码所需总字节数（含类型标记）。
        /// </summary>
        /// <typeparam name="TValue">数值类型。</typeparam>
        /// <returns>编码总字节数。</returns>
        protected int GetLength<TValue>()
        {
            return GetSize<TValue>() + 1;
        }

        /// <summary>
        /// 获取指定类型的字节大小。
        /// </summary>
        /// <typeparam name="TValue">数值类型。</typeparam>
        /// <returns>类型字节大小。</returns>
        protected int GetSize<TValue>()
        {
            return Unsafe.SizeOf<TValue>();
        }
    }
}