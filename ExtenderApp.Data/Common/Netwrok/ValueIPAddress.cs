using System.Net;
using System.Buffers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 使用 ArrayPool 管理底层字节数组的轻量 IP 地址类型。 可表示 IPv4
    /// (4 bytes) 或 IPv6 (16 bytes)，并支持与 <see
    /// cref="System.Net.IPAddress"/> 相互转换。
    /// 使用完毕应调用 <see cref="Dispose"/> 归还租用缓冲。
    /// </summary>
    public struct ValueIPAddress : IEquatable<ValueIPAddress>, IDisposable
    {
        /// <summary>
        /// IPv4 地址字节长度。
        /// </summary>
        private const int IPv4Length = 4;

        /// <summary>
        /// IPv6 地址字节长度。
        /// </summary>
        private const int IPv6Length = 16;

        /// <summary>
        /// 代表 IPv4 任意地址（0.0.0.0）的 <see
        /// cref="ValueIPAddress"/> 实例。
        /// </summary>
        public static ValueIPAddress Any => FromIPAddress(IPAddress.Any);

        /// <summary>
        /// 代表 IPv4 回环地址（127.0.0.1）的 <see
        /// cref="ValueIPAddress"/> 实例。
        /// </summary>
        public static ValueIPAddress Loopback => FromIPAddress(IPAddress.Loopback);

        /// <summary>
        /// 代表 IPv4 广播地址（255.255.255.255）的 <see
        /// cref="ValueIPAddress"/> 实例。
        /// </summary>
        public static ValueIPAddress Broadcast => FromIPAddress(IPAddress.Broadcast);

        /// <summary>
        /// 表示空或未指定的地址，等同于 <see cref="Any"/>。
        /// </summary>
        public static ValueIPAddress None => Any;

        /// <summary>
        /// 代表 IPv6 任意地址（::）的 <see
        /// cref="ValueIPAddress"/> 实例。
        /// </summary>
        public static ValueIPAddress IPv6Any => FromIPAddress(IPAddress.IPv6Any);

        /// <summary>
        /// 代表 IPv6 回环地址（::1）的 <see
        /// cref="ValueIPAddress"/> 实例。
        /// </summary>
        public static ValueIPAddress IPv6Loopback => FromIPAddress(IPAddress.IPv6Loopback);

        /// <summary>
        /// 表示 IPv6 未指定地址，等同于 <see cref="IPv6Any"/>。
        /// </summary>
        public static ValueIPAddress IPv6None => IPv6Any;

        private byte[]? _buffer; // 租用数组（可能比实际长度更大）
        private int _length;     // 有效字节长度：4 或 16
        private long _scopeId;   // IPv6 scope id（IPv4 时为 0）
        private bool _returned;  // 是否已归还

        /// <summary>
        /// 构造：从字节数组及 scopeId 创建（会将字节复制到租用缓冲）。
        /// </summary>
        /// <param name="address">IPv4（4）或 IPv6（16）字节数组</param>
        /// <param name="scopeid">
        /// IPv6 scope id；IPv4 时忽略
        /// </param>
        public ValueIPAddress(byte[] address, long scopeid = 0) : this((ReadOnlySpan<byte>)address, scopeid) { }

        /// <summary>
        /// 构造：从只读跨度及 scopeId 创建（会将字节复制到租用缓冲）。
        /// </summary>
        public ValueIPAddress(ReadOnlySpan<byte> address, long scopeid = 0)
        {
            if (address.Length != IPv4Length && address.Length != IPv6Length)
                throw new ArgumentException("address must be 4 (IPv4) or 16 (IPv6) bytes", nameof(address));

            _length = address.Length;
            _scopeId = scopeid;
            _returned = false;

            // 租用最少长度为 16 可统一管理，也可按实际长度租用
            int rentSize = _length;
            _buffer = ArrayPool<byte>.Shared.Rent(rentSize);
            address.CopyTo(_buffer.AsSpan(0, _length));
        }

        /// <summary>
        /// 从 <see cref="IPAddress"/> 创建 ValueIPAddress（复制字节）。
        /// </summary>
        public static ValueIPAddress FromIPAddress(IPAddress ip)
        {
            if (ip is null) throw new ArgumentNullException(nameof(ip));

            Span<byte> tempSpan = stackalloc byte[16];
            var bytes = ip.TryWriteBytes(tempSpan, out int written);

            if (written == IPv4Length)
            {
                return new ValueIPAddress(tempSpan.Slice(0, IPv4Length));
            }
            else if (written == IPv6Length)
            {
                return new ValueIPAddress(tempSpan.Slice(0, IPv6Length), ip.ScopeId);
            }
            else
            {
                throw new ArgumentException("当前地址既不是IPV4或IPV6", nameof(ip));
            }
        }

        /// <summary>
        /// 转换为 <see cref="IPAddress"/>（复制或使用跨度构造）。
        /// </summary>
        public IPAddress ToIPAddress()
        {
            if (_buffer == null || _length == 0)
                throw new ObjectDisposedException(nameof(ValueIPAddress));

            // IPAddress 有 Accept
            // ReadOnlySpan<byte> 构造
            if (_length == IPv4Length)
            {
                // IPv4 constructor accepts ReadOnlySpan<byte>
                return new IPAddress(new ReadOnlySpan<byte>(_buffer, 0, IPv4Length));
            }
            else
            {
                return new IPAddress(new ReadOnlySpan<byte>(_buffer, 0, IPv6Length), _scopeId);
            }
        }

        /// <summary>
        /// 返回一份字节副本（调用者负责释放）。
        /// </summary>
        public byte[] ToArray()
        {
            if (_buffer == null || _length == 0)
                throw new ObjectDisposedException(nameof(ValueIPAddress));

            var result = new byte[_length];
            Array.Copy(_buffer, 0, result, 0, _length);
            return result;
        }

        /// <summary>
        /// 获取有效字节作为 ReadOnlySpan。
        /// </summary>
        public Span<byte> AsSpan()
        {
            if (_buffer == null || _length == 0)
                throw new ObjectDisposedException(nameof(ValueIPAddress));
            return new Span<byte>(_buffer, 0, _length);
        }

        /// <summary>
        /// 地址族。
        /// </summary>
        public AddressFamily AddressFamily => _length == 16 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

        /// <summary>
        /// IPv6 时有效的 ScopeId；IPv4 时返回 0。
        /// </summary>
        public long ScopeId
        {
            get => _length == 16 ? _scopeId : 0;
        }

        /// <summary>
        /// 释放并将租用缓冲归还到 ArrayPool（可重复调用但只归还一次）。
        /// </summary>
        public void Dispose()
        {
            if (_returned) return;
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<byte>());
                _buffer = null;
            }
            _length = 0;
            _scopeId = 0;
            _returned = true;
        }

        /// <summary>
        /// 格式化为字符串（委托给 System.Net.IPAddress）。
        /// </summary>
        public override string ToString()
        {
            var ip = ToIPAddress();
            return ip.ToString();
        }

        public bool Equals(ValueIPAddress other)
        {
            if (_length != other._length) return false;
            if (_length == 16 && _scopeId != other._scopeId) return false;
            return AsSpan().SequenceEqual(other.AsSpan());
        }

        public override bool Equals(object? obj)
            => obj is ValueIPAddress v && Equals(v);

        public override int GetHashCode()
        {
            if (_buffer == null || _length == 0) return 0;
            // 简单 hash：对前几字节与长度/ScopeId 做组合
            int h = _length;
            ReadOnlySpan<byte> s = AsSpan();
            for (int i = 0; i < Math.Min(8, s.Length); i++)
            {
                h = (h * 31) ^ s[i];
            }
            if (_length == 16)
                h = HashCode.Combine(h, _scopeId);
            return h;
        }

        public static bool operator ==(ValueIPAddress left, ValueIPAddress right) => left.Equals(right);

        public static bool operator !=(ValueIPAddress left, ValueIPAddress right) => !left.Equals(right);

        public static implicit operator IPAddress(ValueIPAddress v) => v.ToIPAddress();

        public static implicit operator ValueIPAddress(IPAddress ip) => FromIPAddress(ip);
    }
}