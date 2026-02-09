using System.Buffers;
using System.Net.NetworkInformation;

namespace ExtenderApp.Contracts
{
    ///// <summary>
    ///// 基于 <see cref="ArrayPool{T}"/> 租借缓冲的“物理地址”值类型包装器。
    ///// 适用于短期持有 MAC 或任意链路层地址字节序列，避免频繁分配。
    ///// </summary>
    ///// <remarks>
    ///// - 生命周期：使用完毕后必须调用 <see cref="Dispose"/> 将内部缓冲归还池；
    /////   “默认实例”不持有缓冲，且不应调用 <see cref="Dispose"/>。<br/>
    ///// - 有效区间：仅 <see cref="Length"/> 指示的前 N 字节有效；
    /////   底层缓冲容量可能大于 N。<br/>
    ///// - 线程安全：本类型非线程安全；从 <see cref="Span"/> 获取的 <see cref="Span{T}"/> 在 <see cref="Dispose"/> 后即刻失效。<br/>
    ///// - 性能：拷贝一次输入 <see cref="ReadOnlySpan{T}"/>，内部存储位于共享池，适合高频、短生命周期场景。
    ///// </remarks>
    //public readonly struct ValuePhysicalAddress : IEquatable<ValuePhysicalAddress>, IDisposable
    //{
    //    public static readonly ValuePhysicalAddress None = new ValuePhysicalAddress();

    //    /// <summary>
    //    /// 底层租借的字节数组（可能比 <see cref="Length"/> 更长）。
    //    /// </summary>
    //    private readonly byte[] _addressBytes;

    //    /// <summary>
    //    /// 返回底层缓冲的可写视图。
    //    /// 注意：仅前 <see cref="Length"/> 字节为有效数据；
    //    /// 请勿在 <see cref="Dispose"/> 之后保留或使用该 <see cref="Span{T}"/>。
    //    /// </summary>
    //    public Span<byte> Span => _addressBytes;

    //    /// <summary>
    //    /// 有效地址字节长度。
    //    /// </summary>
    //    public int Length { get; }

    //    /// <summary>
    //    /// 指示是否为“空”实例（未租借缓冲或有效长度为 0）。
    //    /// </summary>
    //    public bool IsEmpty => _addressBytes == null || Length == 0;

    //    public ValuePhysicalAddress(PhysicalAddress address)
    //        : this(address.GetAddressBytes())
    //    {
    //    }

    //    /// <summary>
    //    /// 以指定字节序列构造实例；内部会从共享池租借足够的缓冲并复制数据。
    //    /// </summary>
    //    /// <param name="addressBytes">物理地址字节序列。</param>
    //    public ValuePhysicalAddress(ReadOnlySpan<byte> addressBytes)
    //    {
    //        Length = addressBytes.Length;
    //        _addressBytes = ArrayPool<byte>.Shared.Rent(addressBytes.Length);
    //        addressBytes.CopyTo(_addressBytes);
    //    }

    //    public ValuePhysicalAddress()
    //    {
    //        _addressBytes = null!;
    //        Length = 0;
    //    }

    //    /// <summary>
    //    /// 将硬件地址字节序列写入到 <paramref name="block"/> 末尾。
    //    /// </summary>
    //    /// <param name="block">指定字节块</param>
    //    public void CopyTo(ref ByteBlock block)
    //    {
    //        if (IsEmpty)
    //            return;

    //        block.Write(Span);
    //    }

    //    /// <summary>
    //    /// 将内部缓冲归还共享池。
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        if (_addressBytes != null)
    //        {
    //            ArrayPool<byte>.Shared.Return(_addressBytes);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        if (IsEmpty) return string.Empty;

    //        var memory = _addressBytes.AsMemory(0, Length);
    //        int len = memory.Length * 3 - 1; // 两位十六进制 + 分隔符
    //        return string.Create(len, memory, static (dst, src) =>
    //        {
    //            int di = 0;
    //            ReadOnlySpan<byte> span = src.Span;
    //            for (int i = 0; i < src.Length; i++)
    //            {
    //                byte b = span[i];
    //                int hi = b >> 4;
    //                int lo = b & 0xF;

    //                dst[di++] = (char)(hi < 10 ? '0' + hi : 'A' + (hi - 10));
    //                dst[di++] = (char)(lo < 10 ? '0' + lo : 'A' + (lo - 10));

    //                if (i < src.Length - 1)
    //                    dst[di++] = '-';
    //            }
    //        });
    //    }

    //    /// <summary>
    //    /// 比较两个 <see cref="ValuePhysicalAddress"/> 是否相等。
    //    /// </summary>
    //    /// <param name="other">另一个实例。</param>
    //    /// <returns>若长度一致且字节序列相同则为 true。</returns>
    //    public bool Equals(ValuePhysicalAddress other)
    //    {
    //        if (IsEmpty && other.IsEmpty)
    //            return true;
    //        if (IsEmpty || other.IsEmpty)
    //            return false;
    //        if (Length != other.Length)
    //            return false;
    //        return Span.SequenceEqual(other.Span);
    //    }

    //    public static implicit operator ValuePhysicalAddress(PhysicalAddress address)
    //        => new ValuePhysicalAddress(address.GetAddressBytes());

    //    public static implicit operator PhysicalAddress(ValuePhysicalAddress valueAddress)
    //        => new PhysicalAddress(valueAddress._addressBytes.AsSpan(0, valueAddress.Length).ToArray());
    //}
}