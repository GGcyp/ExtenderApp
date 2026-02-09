using System.Net;
using System.Buffers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Contracts
{
    ///// <summary>
    ///// 使用 ArrayPool 管理底层字节数组的轻量 IP 地址类型。 可表示 IPv4
    ///// (4 bytes) 或 IPv6 (16 bytes)，并支持与 <see
    ///// cref="System.Net.IPAddress"/> 相互转换。
    ///// 使用完毕应调用 <see cref="Dispose"/> 归还租用缓冲。
    ///// </summary>
    //public struct ValueIPAddress : IEquatable<ValueIPAddress>, IDisposable
    //{
    //    /// <summary>
    //    /// IPv4 地址字节长度。
    //    /// </summary>
    //    public const int IPv4Length = 4;

    //    /// <summary>
    //    /// IPv6 地址字节长度。
    //    /// </summary>
    //    public const int IPv6Length = 16;

    //    /// <summary>
    //    /// 代表 IPv4 任意地址（0.0.0.0）的 <see
    //    /// cref="ValueIPAddress"/> 实例。
    //    /// </summary>
    //    public static ValueIPAddress Any => FromIPAddress(IPAddress.Any);

    //    /// <summary>
    //    /// 代表 IPv4 回环地址（127.0.0.1）的 <see
    //    /// cref="ValueIPAddress"/> 实例。
    //    /// </summary>
    //    public static ValueIPAddress Loopback => FromIPAddress(IPAddress.Loopback);

    //    /// <summary>
    //    /// 代表 IPv4 广播地址（255.255.255.255）的 <see
    //    /// cref="ValueIPAddress"/> 实例。
    //    /// </summary>
    //    public static ValueIPAddress Broadcast => FromIPAddress(IPAddress.Broadcast);

    //    /// <summary>
    //    /// 表示空或未指定的地址，等同于 <see cref="Any"/>。
    //    /// </summary>
    //    public static ValueIPAddress None => Any;

    //    /// <summary>
    //    /// 代表 IPv6 任意地址（::）的 <see
    //    /// cref="ValueIPAddress"/> 实例。
    //    /// </summary>
    //    public static ValueIPAddress IPv6Any => FromIPAddress(IPAddress.IPv6Any);

    //    /// <summary>
    //    /// 代表 IPv6 回环地址（::1）的 <see
    //    /// cref="ValueIPAddress"/> 实例。
    //    /// </summary>
    //    public static ValueIPAddress IPv6Loopback => FromIPAddress(IPAddress.IPv6Loopback);

    //    /// <summary>
    //    /// 表示 IPv6 未指定地址，等同于 <see cref="IPv6Any"/>。
    //    /// </summary>
    //    public static ValueIPAddress IPv6None => IPv6Any;

    //    private byte[]? _buffer; // 租用数组（可能比实际长度更大）
    //    private long _scopeId;   // IPv6 scope id（IPv4 时为 0）

    //    public int Length { get; private set; }

    //    /// <summary>
    //    /// 检查是否为空地址（未初始化或已释放）。
    //    /// </summary>
    //    public bool IsEmpty => _buffer is null || Length == 0;

    //    /// <summary>
    //    /// 地址族。
    //    /// </summary>
    //    public AddressFamily AddressFamily => Length == 16 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

    //    /// <summary>
    //    /// IPv6 时有效的 ScopeId；IPv4 时返回 0。
    //    /// </summary>
    //    public long ScopeId
    //    {
    //        get => Length == 16 ? _scopeId : 0;
    //    }

    //    /// <summary>
    //    /// 构造：从字节数组及 scopeId 创建（会将字节复制到租用缓冲）。
    //    /// </summary>
    //    /// <param name="address">IPv4（4）或 IPv6（16）字节数组</param>
    //    /// <param name="scopeid">
    //    /// IPv6 scope id；IPv4 时忽略
    //    /// </param>
    //    public ValueIPAddress(byte[] address, long scopeid = 0) : this((ReadOnlySpan<byte>)address, scopeid) { }

    //    /// <summary>
    //    /// 构造：从只读跨度及 scopeId 创建（会将字节复制到租用缓冲）。
    //    /// </summary>
    //    public ValueIPAddress(ReadOnlySpan<byte> address, long scopeid = 0)
    //    {
    //        if (address.Length != IPv4Length && address.Length != IPv6Length)
    //            throw new ArgumentException("address must be 4 (IPv4) or 16 (IPv6) bytes", nameof(address));

    //        Length = address.Length;
    //        _scopeId = scopeid;

    //        // 租用最少长度为 16 可统一管理，也可按实际长度租用
    //        int rentSize = Length;
    //        _buffer = ArrayPool<byte>.Shared.Rent(rentSize);
    //        address.CopyTo(_buffer.AsSpan(0, Length));
    //    }

    //    /// <summary>
    //    /// 通过 32 位无符号整数创建 IPv4 地址（按网络字节序 Big-Endian 解析）。
    //    /// </summary>
    //    /// <param name="ipv4Address"> 32 位无符号整数地址</param>
    //    public ValueIPAddress(uint ipv4Address)
    //    {
    //        Length = IPv4Length;
    //        _scopeId = 0;
    //        _buffer = ArrayPool<byte>.Shared.Rent(IPv4Length);
    //        _buffer[0] = (byte)((ipv4Address >> 24) & 0xFF);
    //        _buffer[1] = (byte)((ipv4Address >> 16) & 0xFF);
    //        _buffer[2] = (byte)((ipv4Address >> 8) & 0xFF);
    //        _buffer[3] = (byte)(ipv4Address & 0xFF);
    //    }

    //    /// <summary>
    //    /// 从 <see cref="IPAddress"/> 创建 ValueIPAddress（复制字节）。
    //    /// </summary>
    //    public static ValueIPAddress FromIPAddress(IPAddress ip)
    //    {
    //        if (ip is null) throw new ArgumentNullException(nameof(ip));

    //        Span<byte> tempSpan = stackalloc byte[16];
    //        var bytes = ip.TryWriteBytes(tempSpan, out int written);

    //        if (written == IPv4Length)
    //        {
    //            return new ValueIPAddress(tempSpan.Slice(0, IPv4Length));
    //        }
    //        else if (written == IPv6Length)
    //        {
    //            return new ValueIPAddress(tempSpan.Slice(0, IPv6Length), ip.ScopeId);
    //        }
    //        else
    //        {
    //            throw new ArgumentException("当前地址既不是IPV4或IPV6", nameof(ip));
    //        }
    //    }

    //    /// <summary>
    //    /// 转换为 <see cref="IPAddress"/>（复制或使用跨度构造）。
    //    /// </summary>
    //    public IPAddress ToIPAddress()
    //    {
    //        if (_buffer == null || Length == 0)
    //            throw new ObjectDisposedException(nameof(ValueIPAddress));

    //        // IPAddress 有 Accept
    //        // ReadOnlySpan<byte> 构造
    //        if (Length == IPv4Length)
    //        {
    //            // IPv4 constructor accepts ReadOnlySpan<byte>
    //            return new IPAddress(new ReadOnlySpan<byte>(_buffer, 0, IPv4Length));
    //        }
    //        else
    //        {
    //            return new IPAddress(new ReadOnlySpan<byte>(_buffer, 0, IPv6Length), _scopeId);
    //        }
    //    }

    //    /// <summary>
    //    /// 返回一份字节副本（调用者负责释放）。
    //    /// </summary>
    //    public byte[] ToArray()
    //    {
    //        if (_buffer == null || Length == 0)
    //            throw new ObjectDisposedException(nameof(ValueIPAddress));

    //        var result = new byte[Length];
    //        Array.Copy(_buffer, 0, result, 0, Length);
    //        return result;
    //    }

    //    /// <summary>
    //    /// 获取有效字节作为 Span。
    //    /// </summary>
    //    public Span<byte> AsSpan()
    //    {
    //        if (_buffer == null || Length == 0)
    //            throw new ObjectDisposedException(nameof(ValueIPAddress));
    //        return new Span<byte>(_buffer, 0, Length);
    //    }

    //    /// <summary>
    //    /// 将当前 IP 地址的有效字节复制到目标缓冲区。
    //    /// </summary>
    //    /// <param name="destination">
    //    /// 目标写入的字节跨度，长度必须大于等于 <see cref="Length"/>。
    //    /// </param>
    //    /// <param name="written">
    //    /// 实际写入的字节数；复制成功时等于 <see cref="Length"/>。
    //    /// </param>
    //    /// <exception cref="ObjectDisposedException">
    //    /// 当实例未初始化或已释放（内部缓冲为 null 或 <see cref="Length"/> 为 0）时抛出。
    //    /// </exception>
    //    /// <exception cref="ArgumentException">
    //    /// 当 <paramref name="destination"/> 的长度小于 <see cref="Length"/> 时抛出。
    //    /// </exception>
    //    public void TryWriteBytes(Span<byte> destination, out int written)
    //    {
    //        if (_buffer == null || Length == 0)
    //            throw new ObjectDisposedException(nameof(ValueIPAddress));
    //        if (destination.Length < Length)
    //            throw new ArgumentException("Destination span is too small", nameof(destination));
    //        written = Length;
    //        new Span<byte>(_buffer, 0, Length).CopyTo(destination);
    //    }

    //    /// <summary>
    //    /// 将当前 IP 地址的有效字节写入到 <see cref="ByteBlock"/> 末尾。
    //    /// </summary>
    //    /// <param name="block">指定字节块</param>
    //    public void CopyTo(ref ByteBlock block)
    //    {
    //        if (IsEmpty)
    //            return;

    //        block.Write(AsSpan());
    //    }

    //    /// <summary>
    //    /// 将当前 IPv4 地址转换为 32 位无符号整数（按网络字节序 Big-Endian 组合）。
    //    /// </summary>
    //    /// <returns>
    //    /// 与 IPv4 四段十进制等价的 32 位无符号整数值；高位为地址第一个字节。
    //    /// </returns>
    //    /// <exception cref="ArgumentNullException">
    //    /// 当当前实例为空（未初始化或已释放）时抛出。
    //    /// </exception>
    //    /// <exception cref="ArgumentException">
    //    /// 当当前地址不是 IPv4（长度非 4 字节）时抛出。
    //    /// </exception>
    //    /// <remarks>
    //    /// 返回值按网络序（Big-Endian）计算，即 (b0 &lt;&lt; 24) | (b1 &lt;&lt; 16) | (b2 &lt;&lt; 8) | b3。
    //    /// 若需主机字节序表示，可依据 <see cref="BitConverter.IsLittleEndian"/> 进行转换。
    //    /// </remarks>
    //    public uint ToUInt32()
    //    {
    //        if (IsEmpty)
    //            throw new ArgumentNullException(nameof(ValueIPAddress));
    //        if (AddressFamily != AddressFamily.InterNetwork)
    //            throw new ArgumentException("仅支持 IPv4 地址转换为 UInt32");

    //        ReadOnlySpan<byte> span = AsSpan();
    //        return (uint)(span[0] << 24 | span[1] << 16 | span[2] << 8 | span[3]);
    //    }

    //    /// <summary>
    //    /// 释放并将租用缓冲归还到 ArrayPool
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        if (_buffer != null)
    //        {
    //            ArrayPool<byte>.Shared.Return(_buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<byte>());
    //            _buffer = null;
    //        }
    //        Length = 0;
    //        _scopeId = 0;
    //    }

    //    /// <summary>
    //    /// 格式化为字符串（委托给 System.Net.IPAddress）。
    //    /// </summary>
    //    public override string ToString()
    //    {
    //        var ip = ToIPAddress();
    //        return ip.ToString();
    //    }

    //    public bool Equals(ValueIPAddress other)
    //    {
    //        if (Length != other.Length) return false;
    //        if (Length == 16 && _scopeId != other._scopeId) return false;
    //        return AsSpan().SequenceEqual(other.AsSpan());
    //    }

    //    public override bool Equals(object? obj)
    //        => obj is ValueIPAddress v && Equals(v);

    //    public override int GetHashCode()
    //    {
    //        if (_buffer == null || Length == 0) return 0;
    //        // 简单 hash：对前几字节与长度/ScopeId 做组合
    //        int h = Length;
    //        ReadOnlySpan<byte> s = AsSpan();
    //        for (int i = 0; i < Math.Min(8, s.Length); i++)
    //        {
    //            h = (h * 31) ^ s[i];
    //        }
    //        if (Length == 16)
    //            h = HashCode.Combine(h, _scopeId);
    //        return h;
    //    }

    //    public static bool operator ==(ValueIPAddress left, ValueIPAddress right) => left.Equals(right);

    //    public static bool operator !=(ValueIPAddress left, ValueIPAddress right) => !left.Equals(right);

    //    public static implicit operator IPAddress(ValueIPAddress v) => v.ToIPAddress();

    //    public static implicit operator ValueIPAddress(IPAddress ip) => FromIPAddress(ip);
    //}
}