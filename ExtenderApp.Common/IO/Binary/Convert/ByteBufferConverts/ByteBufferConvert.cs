

using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    /// <summary>
    /// 基于 <see cref="ByteBuffer"/> 的高性能二进制写入适配器集合。
    /// 负责将常见原始类型、字符串、集合头与扩展头按照内部编码规则写入到可增长缓冲区，
    /// 具体编码由内部的 <c>_binaryConvert</c> 实现。
    /// </summary>
    public partial class ByteBufferConvert
    {
        /// <summary>
        /// 二进制转换器实例，负责具体的编码实现。
        /// </summary>
        private readonly BinaryConvert _binaryConvert;

        private BinaryCode BinaryCode => _binaryConvert.BinaryCode;

        public ByteBufferConvert(BinaryConvert binaryConvert)
        {
            _binaryConvert = binaryConvert;
        }
    }
}
