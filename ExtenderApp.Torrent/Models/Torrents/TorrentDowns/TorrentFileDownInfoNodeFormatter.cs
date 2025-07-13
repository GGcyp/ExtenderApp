using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// TorrentFileDownInfoNode 的格式化器类
    /// </summary>
    internal class TorrentFileDownInfoNodeFormatter : FileNodeFormatter<TorrentFileDownInfoNode>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resolver">二进制格式化解析器</param>
        /// <param name="binaryWriterConvert">二进制写入转换器</param>
        /// <param name="binaryReaderConvert">二进制读取转换器</param>
        /// <param name="options">二进制选项</param>
        public TorrentFileDownInfoNodeFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>反序列化后的 TorrentFileDownInfoNode 对象</returns>
        protected override TorrentFileDownInfoNode ProtectedDeserialize(ref ExtenderBinaryReader reader)
        {
            var result = base.ProtectedDeserialize(ref reader);
            result.IsDownload = _bool.Deserialize(ref reader);
            result.Downloaded = _long.Deserialize(ref reader);
            result.Uploaded = _long.Deserialize(ref reader);
            return result;
        }

        /// <summary>
        /// 序列化方法
        /// </summary>
        /// <param name="writer">二进制写入器</param>
        /// <param name="value">待序列化的 TorrentFileDownInfoNode 对象</param>
        protected override void ProtectedSerialize(ref ExtenderBinaryWriter writer, TorrentFileDownInfoNode value)
        {
            base.ProtectedSerialize(ref writer, value);
            _bool.Serialize(ref writer, value.IsDownload);
            _long.Serialize(ref writer, value.Downloaded);
            _long.Serialize(ref writer, value.Uploaded);
        }

        /// <summary>
        /// 获取序列化后的数据长度
        /// </summary>
        /// <param name="value">待序列化的 TorrentFileDownInfoNode 对象</param>
        /// <param name="dataBuffer">数据缓冲区</param>
        protected override void ProtectedGetLength(TorrentFileDownInfoNode value, DataBuffer<long> dataBuffer)
        {
            base.ProtectedGetLength(value, dataBuffer);
            dataBuffer.Item1 += _bool.Length;
            dataBuffer.Item1 += _long.Length;
            dataBuffer.Item1 += _long.Length;
        }
    }
}
