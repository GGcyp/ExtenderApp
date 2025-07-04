using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件操作接口
    /// </summary>
    public interface IFileOperate : IConcurrentOperate
    {
        /// <summary>
        /// 获取文件的本地信息
        /// </summary>
        LocalFileInfo Info { get; }

        /// <summary>
        /// 获取最后一次操作的时间。
        /// </summary>
        /// <returns>最后一次操作的时间。</returns>
        DateTime LastOperateTime { get; }

        /// <summary>
        /// 获取或设置是否托管。
        /// </summary>
        /// <value>
        /// 如果托管，则为 true；否则为 false。
        /// </value>
        bool IsHosted { get; set; }

        #region Write

        /// <summary>
        /// 将字节数组写入文件
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        void Write(byte[] bytes);

        /// <summary>
        /// 将字节数组从指定位置写入文件
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="filePosition">要写入的起始位置</param>
        void Write(byte[] bytes, long filePosition);

        /// <summary>
        /// 将字节数组从指定位置写入文件，并指定要写入的字节长度
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="filePosition">要写入的起始位置</param>
        /// <param name="bytesPosition">要写入字节数组的起始位置</param>
        /// <param name="bytesLength">要写入的字节长度</param>
        void Write(byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        /// <summary>
        /// 将数据写入到指定位置。
        /// </summary>
        /// <param name="writer">用于写入数据的<see cref="ExtenderBinaryWriter"/>对象。</param>
        /// <param name="filePosition">写入数据的起始位置。</param>
        void Write(ExtenderBinaryWriter writer, long filePosition);

        #endregion

        #region WriteAsync

        /// <summary>
        /// 异步将字节数组写入文件
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组</param>
        void WriteAsync(byte[] bytes, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步将字节数组从指定位置写入文件
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="filePosition">要写入的起始位置</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组</param>
        void WriteAsync(byte[] bytes, long filePosition, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步将字节数组从指定位置写入文件，并指定要写入的字节长度
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="filePosition">要写入的起始位置</param>
        /// <param name="bytesPosition">要写入字节数组的起始位置</param>
        /// <param name="bytesLength">要写入的字节长度</param>
        /// <param name="callback">写入完成后的回调函数，参数为写入的字节数组</param>
        void WriteAsync(byte[] bytes, long filePosition, int bytesPosition, int bytesLength, Action<byte[]>? callback = null);

        /// <summary>
        /// 异步写入数据到文件中。
        /// </summary>
        /// <param name="writer">用于写入数据的二进制写入器。</param>
        /// <param name="filePosition">文件写入的位置。</param>
        /// <param name="callback">写入完成后的回调函数。</param>
        void WriteAsync(ExtenderBinaryWriter writer, long filePosition, Action callback);


        #endregion

        #region Read

        /// <summary>
        /// 读取数据并返回字节数组。
        /// </summary>
        /// <returns>读取的数据，返回字节数组。</returns>
        byte[] Read();

        /// <summary>
        /// 从指定位置读取指定长度的字节数组
        /// </summary>
        /// <param name="filePosition">读取的起始位置</param>
        /// <param name="length">要读取的字节长度</param>
        /// <returns>读取到的字节数组</returns>
        byte[]? Read(long filePosition, int length);

        /// <summary>
        /// 从指定位置读取指定长度的字节数组到目标字节数组中
        /// </summary>
        /// <param name="filePosition">读取的起始位置</param>
        /// <param name="length">要读取的字节长度</param>
        /// <param name="bytes">目标字节数组</param>
        /// <param name="bytesStart">目标字节数组的起始位置</param>
        /// <returns>是否读取成功</returns>
        void Read(long filePosition, int length, byte[] bytes, int bytesStart = 0);

        #endregion

        #region ReadAsync

        /// <summary>
        /// 异步从指定位置读取指定长度的字节数组
        /// </summary>
        /// <param name="filePosition">读取的起始位置</param>
        /// <param name="length">要读取的字节长度</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取到的字节数组</param>
        void ReadAsync(long filePosition, int length, Action<byte[]> callback);

        /// <summary>
        /// 异步从指定位置读取指定长度的字节数组到目标字节数组中
        /// </summary>
        /// <param name="filePosition">读取的起始位置</param>
        /// <param name="length">要读取的字节长度</param>
        /// <param name="bytes">目标字节数组</param>
        /// <param name="callback">读取完成后的回调函数，参数为读取到的字节数组</param>
        /// <param name="bytesStart">目标字节数组的起始位置</param>
        void ReadAsync(long filePosition, int length, byte[] bytes, Action<byte[]> callback, int bytesStart = 0);

        /// <summary>
        /// 从指定文件位置读取指定长度的字节数据到数组池中。
        /// </summary>
        /// <param name="filePosition">文件位置。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        byte[] ReadForArrayPool(long filePosition, int length);

        #endregion

        /// <summary>
        /// 扩展容量到指定大小。
        /// </summary>
        /// <param name="newCapacity">新的容量大小。</param>
        void ExpandCapacity(long newCapacity);
    }
}
