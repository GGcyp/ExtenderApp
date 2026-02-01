using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 针对类型 <typeparamref name="T"/> 的二进制序列化/反序列化抽象基类（中间层）。 封装共享依赖（ <see cref="ByteBufferConvert"/> 与 <see cref="BinaryOptions"/>）， 具体读写逻辑由更具体的派生类实现。
    /// </summary>
    /// <typeparam name="T">要序列化/反序列化的目标类型。</typeparam>
    public abstract class BinaryFormatter<T> : BinaryFormatterBase<T>
    {
        /// <summary>
        /// 二进制选项（类型码、范围、长度、时间等运行时配置）。
        /// </summary>
        protected readonly BinaryOptions Options;

        /// <summary>
        /// 使用指定的写适配器与二进制选项初始化格式化器。
        /// </summary>
        /// <param name="blockConvert">字节块写入适配器集合。</param>
        /// <param name="options">二进制选项。</param>
        protected BinaryFormatter(BinaryOptions options)
        {
            Options = options;
        }
    }
}