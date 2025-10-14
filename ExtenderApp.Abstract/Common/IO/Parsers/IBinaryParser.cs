using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制文件解析器接口，继承自文件解析器接口。
    /// </summary>
    /// <remarks>
    /// 定义对象 ⇄ 二进制 的序列化/反序列化契约，以及基于 LZ4 的压缩/解压能力。
    /// 具体异常与边界行为以实现为准。
    /// </remarks>
    public interface IBinaryParser : IFileParser
    {
        #region Deserialize

        /// <summary>
        /// 将内存中的字节反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="span">包含序列化数据的只读内存。</param>
        /// <returns>反序列化后的对象；失败或内容为空可返回 null（或实现可能抛出异常）。</returns>
        T? Deserialize<T>(ReadOnlyMemory<byte> span);

        /// <summary>
        /// 从流中反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="stream">包含序列化数据的流。</param>
        /// <returns>反序列化后的对象；失败或格式不匹配时返回 null（或实现可能抛出异常）。</returns>
        T? Deserialize<T>(Stream stream);

        /// <summary>
        /// 从顺序缓冲中反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="buffer">顺序缓冲（从其可读区读取）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        T? Deserialize<T>(ref ByteBuffer buffer);

        /// <summary>
        /// 从缓冲块中反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="block">缓冲块（读取其未读区）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        T? Deserialize<T>(ref ByteBlock block);

        /// <summary>
        /// 异步地从流中反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="stream">包含序列化数据的流。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken token = default);

        /// <summary>
        /// 异步地从只读内存反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="span">包含序列化数据的只读内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> span, CancellationToken token = default);

        /// <summary>
        /// 异步地从顺序缓冲反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="buffer">顺序缓冲（从其可读区读取）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DeserializeAsync<T>(ref ByteBuffer buffer);

        /// <summary>
        /// 异步地从缓冲块反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="block">缓冲块（读取其未读区）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DeserializeAsync<T>(ref ByteBlock block);

        #endregion Deserialize

        #region Serialize

        /// <summary>
        /// 将对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>包含序列化结果的字节数组。</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// 将对象序列化到调用方提供的数组中。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="bytes">目标数组（容量需足够，容量不足应抛出异常）。</param>
        void Serialize<T>(T value, byte[] bytes);

        /// <summary>
        /// 将对象序列化到顺序缓冲中（追加到其写入端）。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="buffer">目标顺序缓冲。</param>
        /// <param name="value">要序列化的对象。</param>
        void Serialize<T>(T value, ref ByteBuffer buffer);

        /// <summary>
        /// 将对象序列化到缓冲块中（追加到其写入端）。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="block">目标缓冲块。</param>
        /// <param name="value">要序列化的对象。</param>
        void Serialize<T>(T value, ref ByteBlock block);

        /// <summary>
        /// 将对象序列化为二进制并写入到流。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="stream">目标流。</param>
        /// <param name="value">要序列化的对象。</param>
        void Serialize<T>(T value, Stream stream);

        /// <summary>
        /// 异步地将对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>包含序列化结果的字节数组。</returns>
        Task<byte[]> SerializeAsync<T>(T value, CancellationToken token = default);

        /// <summary>
        /// 异步地将对象序列化为字节并写入到流。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="stream">目标流。</param>
        /// <param name="token">取消令牌。</param>
        Task SerializeAsync<T>(T value, Stream stream, CancellationToken token = default);

        /// <summary>
        /// 异步地将对象序列化到顺序缓冲。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="buffer">目标顺序缓冲。</param>
        /// <param name="token">取消令牌。</param>
        Task SerializeAsync<T>(T value, ref ByteBuffer buffer, CancellationToken token = default);

        /// <summary>
        /// 异步地将对象序列化到缓冲块。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="block">目标缓冲块。</param>
        /// <param name="token">取消令牌。</param>
        Task SerializeAsync<T>(T value, ref ByteBlock block, CancellationToken token = default);

        #endregion Serialize

        #region Compression

        /// <summary>
        /// 将对象序列化后按指定 LZ4 压缩格式写入到文件（通过 FileOperateInfo 打开）。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="value">要写入的对象。</param>
        /// <param name="compression">压缩类型（None 表示直写，不压缩）。</param>
        void Write<T>(FileOperateInfo info, T value, CompressionType compression);

        /// <summary>
        /// 将对象序列化后按指定 LZ4 压缩格式写入到文件操作接口。
        /// </summary>

        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="value">要写入的对象。</param>
        /// <param name="compression">压缩类型（None 表示直写，不压缩）。</param>
        void Write<T>(IFileOperate fileOperate, T value, CompressionType compression);

        /// <summary>
        /// 将顺序缓冲中的字节作为载荷，按指定 LZ4 压缩格式写入到文件（通过 ExpectLocalFileInfo 打开）。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="buffer">输入顺序缓冲（从其可读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        void Write(ExpectLocalFileInfo info, ref ByteBuffer buffer, CompressionType compression);

        /// <summary>
        /// 将顺序缓冲中的字节作为载荷，按指定 LZ4 压缩格式写入到文件（通过 FileOperateInfo 打开）。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="buffer">输入顺序缓冲（从其可读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        void Write(FileOperateInfo info, ref ByteBuffer buffer, CompressionType compression);

        /// <summary>
        /// 将顺序缓冲中的字节作为载荷，按指定 LZ4 压缩格式写入到文件操作接口。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="buffer">输入顺序缓冲（从其可读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        void Write(IFileOperate fileOperate, ref ByteBuffer buffer, CompressionType compression);

        /// <summary>
        /// 将缓冲块中的字节作为载荷，按指定 LZ4 压缩格式写入（ExpectLocalFileInfo）。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="block">输入缓冲块（从其未读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        void Write(ExpectLocalFileInfo info, ref ByteBlock block, CompressionType compression);

        /// <summary>
        /// 将缓冲块中的字节作为载荷，按指定 LZ4 压缩格式写入（FileOperateInfo）。
        /// </summary>

        /// <param name="info">文件操作信息。</param>
        /// <param name="block">输入缓冲块（从其未读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        void Write(FileOperateInfo info, ref ByteBlock block, CompressionType compression);

        /// <summary>
        /// 将缓冲块中的字节作为载荷，按指定 LZ4 压缩格式写入（IFileOperate）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="block">输入缓冲块（从其未读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        void Write(IFileOperate fileOperate, ref ByteBlock block, CompressionType compression);

        /// <summary>
        /// 异步：将对象序列化后按指定 LZ4 压缩格式写入（ExpectLocalFileInfo）。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的对象。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(ExpectLocalFileInfo info, T value, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将对象序列化后按指定 LZ4 压缩格式写入（FileOperateInfo）。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="info">文件操作信息。</param>
        /// <param name="value">要写入的对象。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(FileOperateInfo info, T value, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将对象序列化后按指定 LZ4 压缩格式写入（IFileOperate）。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="value">要写入的对象。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(IFileOperate fileOperate, T value, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将顺序缓冲中的字节压缩后写入（ExpectLocalFileInfo）。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="buffer">输入顺序缓冲（从其可读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync(ExpectLocalFileInfo info, ref ByteBuffer buffer, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将顺序缓冲中的字节压缩后写入（FileOperateInfo）。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="buffer">输入顺序缓冲（从其可读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync(FileOperateInfo info, ref ByteBuffer buffer, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将顺序缓冲中的字节压缩后写入（IFileOperate）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="buffer">输入顺序缓冲（从其可读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync(IFileOperate fileOperate, ref ByteBuffer buffer, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将缓冲块中的字节压缩后写入（ExpectLocalFileInfo）。
        /// </summary>

        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="block">输入缓冲块（从其未读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(ExpectLocalFileInfo info, ref ByteBlock block, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将缓冲块中的字节压缩后写入（FileOperateInfo）。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="block">输入缓冲块（从其未读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync(FileOperateInfo info, ref ByteBlock block, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 异步：将缓冲块中的字节压缩后写入（IFileOperate）。
        /// </summary>
        /// <param name="fileOperate">文件操作接口。</param>
        /// <param name="block">输入缓冲块（从其未读区读取并压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync(IFileOperate fileOperate, ref ByteBlock block, CompressionType compression, CancellationToken token = default);

        /// <summary>
        /// 将对象序列化并按指定压缩类型压缩为字节数组。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要压缩的对象（先序列化后压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <returns>压缩后的字节数组。</returns>
        byte[] Compression<T>(T value, CompressionType compression);

        /// <summary>
        /// 将对象序列化并压缩到新的顺序缓冲中。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要压缩的对象（先序列化后压缩）。</param>
        /// <param name="buffer">输出：承载压缩结果的新顺序缓冲。</param>
        /// <param name="compression">压缩类型。</param>
        void Compression<T>(T value, out ByteBuffer buffer, CompressionType compression);

        /// <summary>
        /// 将对象序列化并压缩到新的缓冲块中。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要压缩的对象（先序列化后压缩）。</param>
        /// <param name="block">输出：承载压缩结果的新缓冲块。</param>
        /// <param name="compression">压缩类型。</param>
        void Compression<T>(T value, out ByteBlock block, CompressionType compression);

        /// <summary>
        /// 将输入顺序缓冲的可读区按指定压缩类型压缩到新的顺序缓冲。
        /// </summary>
        /// <param name="inputBuffer">输入顺序缓冲（读取其可读区）。</param>
        /// <param name="outBuffer">输出：压缩结果缓冲。</param>
        /// <param name="compression">压缩类型。</param>
        void Compression(ref ByteBuffer inputBuffer, out ByteBuffer outBuffer, CompressionType compression);

        /// <summary>
        /// 将输入缓冲块的未读区按指定压缩类型压缩到新的缓冲块。
        /// </summary>
        /// <param name="inputBlock">输入缓冲块（读取其未读区）。</param>
        /// <param name="outBlock">输出：压缩结果缓冲块。</param>
        /// <param name="compression">压缩类型。</param>
        void Compression(ref ByteBlock inputBlock, out ByteBlock outBlock, CompressionType compression);

        /// <summary>
        /// 异步：将对象序列化并压缩为字节数组。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要压缩的对象（先序列化后压缩）。</param>
        /// <param name="compression">压缩类型。</param>
        /// <returns>压缩后的字节数组。</returns>
        Task<byte[]> CompressionAsync<T>(T value, CompressionType compression);

        /// <summary>
        /// 异步：将对象序列化并压缩到新的顺序缓冲。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要压缩的对象（先序列化后压缩）。</param>
        /// <param name="buffer">输出：压缩结果顺序缓冲。</param>
        /// <param name="compression">压缩类型。</param>
        Task CompressionAsync<T>(T value, out ByteBuffer buffer, CompressionType compression);

        /// <summary>
        /// 异步：将对象序列化并压缩到新的缓冲块。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="value">要压缩的对象（先序列化后压缩）。</param>
        /// <param name="block">输出：压缩结果缓冲块。</param>
        /// <param name="compression">压缩类型。</param>
        Task CompressionAsync<T>(T value, out ByteBlock block, CompressionType compression);

        /// <summary>
        /// 异步：压缩输入顺序缓冲到新的顺序缓冲。
        /// </summary>
        /// <param name="inputBuffer">输入顺序缓冲（读取其可读区）。</param>
        /// <param name="outBuffer">输出：压缩结果顺序缓冲。</param>
        /// <param name="compression">压缩类型。</param>
        Task CompressionAsync(ref ByteBuffer inputBuffer, out ByteBuffer outBuffer, CompressionType compression);

        /// <summary>
        /// 异步：压缩输入缓冲块到新的缓冲块。
        /// </summary>
        /// <param name="inputBlock">输入缓冲块（读取其未读区）。</param>
        /// <param name="outBlock">输出：压缩结果缓冲块。</param>
        /// <param name="compression">压缩类型。</param>
        Task CompressionAsync(ref ByteBlock inputBlock, out ByteBlock outBlock, CompressionType compression);

        #endregion Compression

        #region Decompression

        /// <summary>
        /// 将压缩字节解压并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="bytes">包含压缩载荷的字节数组。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        T? Decompression<T>(byte[] bytes);

        /// <summary>
        /// 从顺序缓冲中读取压缩数据，解压并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="buffer">输入顺序缓冲（读取其可读区）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        T? Decompression<T>(ref ByteBuffer buffer);

        /// <summary>
        /// 从缓冲块中读取压缩数据，解压并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="block">输入缓冲块（读取其未读区）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        T? Decompression<T>(ref ByteBlock block);

        /// <summary>
        /// 将输入缓冲块的压缩数据解压到新的缓冲块。
        /// </summary>
        /// <param name="inputBlock">输入缓冲块（包含压缩载荷，读取其未读区）。</param>
        /// <param name="outBlock">输出：解压后的数据缓冲块。</param>
        /// <param name="compression">压缩类型（应与写入时一致；None 表示直通）。</param>
        void Decompression(ref ByteBlock inputBlock, out ByteBlock outBlock, CompressionType compression);

        /// <summary>
        /// 异步：解压字节并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="bytes">包含压缩载荷的字节数组。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DecompressionAsync<T>(byte[] bytes);

        /// <summary>
        /// 异步：从顺序缓冲解压并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="buffer">输入顺序缓冲（读取其可读区）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DecompressionAsync<T>(ref ByteBuffer buffer);

        /// <summary>
        /// 异步：从缓冲块解压并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="block">输入缓冲块（读取其未读区）。</param>
        /// <returns>反序列化后的对象；失败时返回 null。</returns>
        Task<T?> DecompressionAsync<T>(ref ByteBlock block);

        /// <summary>
        /// 异步：将输入顺序缓冲的压缩数据解压到新的顺序缓冲。
        /// </summary>
        /// <param name="inputBuffer">输入顺序缓冲（包含压缩载荷，读取其可读区）。</param>
        /// <param name="outBuffer">输出：解压后的数据顺序缓冲。</param>
        /// <param name="compression">压缩类型（应与写入时一致；None 表示直通）。</param>
        Task DecompressionAsync(ref ByteBuffer inputBuffer, out ByteBuffer outBuffer, CompressionType compression);

        /// <summary>
        /// 异步：将输入缓冲块的压缩数据解压到新的缓冲块。
        /// </summary>
        /// <param name="inputBlock">输入缓冲块（包含压缩载荷，读取其未读区）。</param>
        /// <param name="outBlock">输出：解压后的数据缓冲块。</param>
        /// <param name="compression">压缩类型（应与写入时一致；None 表示直通）。</param>
        Task DecompressionAsync(ref ByteBlock inputBlock, out ByteBlock outBlock, CompressionType compression);

        #endregion Decompression

        /// <summary>
        /// 获取指定对象在默认二进制格式下的序列化长度（可能为估算值）。
        /// </summary>
        long GetLength<T>(T value);

        /// <summary>
        /// 获取指定类型的默认序列化长度（若适用）。
        /// </summary>
        long GetDefaulLength<T>();

        /// <summary>
        /// 获取指定类型的二进制格式化器。
        /// </summary>
        IBinaryFormatter<T> GetFormatter<T>();
    }
}