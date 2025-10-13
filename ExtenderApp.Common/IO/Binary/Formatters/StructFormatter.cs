using System.Runtime.CompilerServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 面向值类型 <typeparamref name="T"/> 的序列化/反序列化抽象基类。
    /// 默认长度使用 <see cref="Unsafe.SizeOf{T}"/> 基于托管布局的大小进行预估，适合按字节写入的场景。
    /// </summary>
    /// <typeparam name="T">值类型约束的目标类型。</typeparam>
    public abstract class StructFormatter<T> : BinaryFormatter<T>
        where T : struct
    {
        /// <summary>
        /// 序列化的默认预估长度（字节数），基于 <see cref="Unsafe.SizeOf{T}"/>。
        /// </summary>
        public override int DefaultLength { get; }

        /// <summary>
        /// 使用指定写适配器与二进制选项初始化结构体格式化器。
        /// </summary>
        /// <param name="blockConvert">字节块写入适配器集合。</param>
        /// <param name="options">二进制选项。</param>
        protected StructFormatter(ByteBufferConvert blockConvert, BinaryOptions options) : base(blockConvert, options)
        {
            DefaultLength = Unsafe.SizeOf<T>() + 1;
        }

        public override long GetLength(T value)
        {
            return DefaultLength;
        }
    }
}
