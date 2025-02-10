namespace ExtenderApp.Common.File.Splitter
{
    /// <summary>
    /// 写操作类，继承自SplitterOperation类
    /// </summary>
    internal class WriteOperation : SplitterOperation
    {
        /// <summary>
        /// 待写入的数据
        /// </summary>
        private byte[] bytes;

        /// <summary>
        /// 数据偏移量
        /// </summary>
        private long offset;

        /// <summary>
        /// 设置要写入的数据和区块索引
        /// </summary>
        /// <param name="bytes">要写入的字节数组</param>
        /// <param name="chunkIndex">区块索引</param>
        /// <exception cref="InvalidOperationException">当字节长度超过区块最大长度或写入数据超出文件长度时抛出</exception>
        public void Set(byte[] bytes, uint chunkIndex)
        {
            if (bytes.Length > SplitterInfo.MaxChunkSize)
            {
                throw new InvalidOperationException("当前字节长度超过了区块最大长度");
            }

            offset = chunkIndex * SplitterInfo.MaxChunkSize;
            if (offset + bytes.Length > SplitterInfo.Length)
            {
                throw new InvalidOperationException("写入数据超出文件长度");
            }

            this.bytes = bytes;
        }

        protected override void ExecuteProtected(FileStream stream)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected override bool Reset()
        {
            bytes = null;
            offset = 0;

            return true;
        }
    }
}
