using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks.Send
{
    /// <summary>
    /// 泛型类 SendDataFormatter，用于格式化 <see cref="SendData{T}"/> 类型的对象。
    /// </summary>
    /// <typeparam name="T">SendData 对象的泛型参数类型。</typeparam>
    internal class NetworkPacketFormatter : ResolverFormatter<NetworkPacket>
    {
        /// <summary>
        /// 整数类型的二进制格式化器。
        /// </summary>
        private readonly IBinaryFormatter<int> _int;

        /// <summary>
        /// 泛型参数 T 类型的二进制格式化器。
        /// </summary>
        private readonly IBinaryFormatter<byte[]> _array;

        /// <summary>
        /// 重写 Count 属性，返回格式化器中包含的字节数。
        /// </summary>
        public override int Length => _int.Length + _array.Length;

        /// <summary>
        /// 初始化 SendDataFormatter 实例。
        /// </summary>
        /// <param name="resolver">二进制格式化器解析器。</param>
        public NetworkPacketFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
            _array = GetFormatter<byte[]>();
        }

        /// <summary>
        /// 反序列化一个 <see cref="SendData{T}"/> 对象。
        /// </summary>
        /// <param name="reader">扩展的二进制读取器。</param>
        /// <returns>反序列化后的 <see cref="SendData{T}"/> 对象。</returns>
        public override NetworkPacket Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = new NetworkPacket();
            result.TypeCode = _int.Deserialize(ref reader);
            result.Bytes = _array.Deserialize(ref reader);
            return result;
        }

        /// <summary>
        /// 序列化一个 <see cref="SendData{T}"/> 对象。
        /// </summary>
        /// <param name="writer">扩展的二进制写入器。</param>
        /// <param name="value">要序列化的 <see cref="SendData{T}"/> 对象。</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, NetworkPacket value)
        {
            _int.Serialize(ref writer, value.TypeCode);
            _array.Serialize(ref writer, value.Bytes);
        }

        /// <summary>
        /// 获取序列化后的 <see cref="SendData{T}"/> 对象的长度。
        /// </summary>
        /// <param name="value">要序列化的 <see cref="SendData{T}"/> 对象。</param>
        /// <returns>序列化后的长度。</returns>
        public override long GetLength(NetworkPacket value)
        {
            return _int.Length + _array.GetLength(value.Bytes);
        }
    }
}
