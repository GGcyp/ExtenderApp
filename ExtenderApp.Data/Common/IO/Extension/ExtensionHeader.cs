

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示扩展头信息的结构体。
    /// </summary>
    public struct ExtensionHeader : IEquatable<ExtensionHeader>
    {
        /// <summary>
        /// 扩展头的固定大小为5字节（1字节类型代码 + 4字节长度）
        /// </summary>
        public const int LengthOfUncompressedDataSizeHeader = 5; 

        /// LZ4块压缩的标识常量
        /// </summary>
        public const sbyte Lz4Block = 99;

        /// <summary>
        /// LZ4块数组压缩的标识常量
        /// </summary>
        public const sbyte Lz4bufferArray = 98;

        public ExtensionHeader Empty => new ExtensionHeader(0, 0);

        /// <summary>
        /// 获取扩展头的类型代码。
        /// </summary>
        public sbyte TypeCode { get; }

        /// <summary>
        /// 获取扩展头的长度。
        /// </summary>
        public uint Length { get; }

        public bool IsEmpty => this.TypeCode == 0 && this.Length == 0;

        /// <summary>
        /// 使用指定的类型代码和长度初始化 ExtensionHeader 实例。
        /// </summary>
        /// <param name="typeCode">类型代码。</param>
        /// <param name="length">未压缩数据的长度。</param>
        public ExtensionHeader(sbyte typeCode, uint length)
        {
            this.TypeCode = typeCode;
            this.Length = length + LengthOfUncompressedDataSizeHeader;
        }

        /// <summary>
        /// 使用指定的类型代码和长度初始化 ExtensionHeader 实例。
        /// </summary>
        /// <param name="typeCode">类型代码。</param>
        /// <param name="length">未压缩数据的长度。</param>
        public ExtensionHeader(sbyte typeCode, int length)
        {
            this.TypeCode = typeCode;
            this.Length = (uint)length + LengthOfUncompressedDataSizeHeader;
        }

        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <param name="other">与当前对象进行比较的对象。</param>
        /// <returns>如果当前对象等于 other 参数，则为 true；否则为 false。</returns>
        public bool Equals(ExtensionHeader other)
            => this.TypeCode == other.TypeCode && this.Length == other.Length;
    }
}
