using ExtenderBinaryWriter = ExtenderApp.Data.ExtenderBinaryWriter;
using ExtenderBinaryReader = ExtenderApp.Data.ExtenderBinaryReader;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制格式化器接口。
    /// </summary>
    public interface IBinaryFormatter
    {
        /// <summary>
        /// 获取当前类型默认字节数量，用于序列化和反序列化。
        /// 如果是Collection类型，返回的头元素中字节的数量。
        /// </summary>
        /// <returns>字节数量</returns>
        int DefaultLength { get; }
    }

    /// <summary>
    /// 泛型二进制格式化器接口，继承自 <see cref="IBinaryFormatter"/>。
    /// </summary>
    /// <typeparam name="T">需要序列化和反序列化的类型。</typeparam>
    public interface IBinaryFormatter<T> : IBinaryFormatter
    {
        /// <summary>
        /// 将指定值序列化到指定的二进制写入器中。
        /// </summary>
        /// <param name="writer">要写入数据的二进制写入器。</param>
        /// <param name="value">要序列化的值。</param>
        /// <returns>序列化后的字节数。</returns>
        void Serialize(ref ExtenderBinaryWriter writer, T value);

        /// <summary>
        /// 从 <see cref="Data.ExtenderBinaryReader"/> 中读取二进制数据并反序列化为指定类型的对象。
        /// </summary>
        /// <param name="reader"><see cref="Data.ExtenderBinaryReader"/> 实例，用于读取二进制数据。</param>
        /// <returns>反序列化得到的对象。</returns>
        T Deserialize(ref ExtenderBinaryReader reader);

        /// <summary>
        /// 获取默认值。
        /// </summary>
        /// <value>返回类型T的默认值。</value>
        T Default { get; }

        /// <summary>
        /// 获取需要转换类型需要的字节数量。
        /// 如果引用类型为null，返回1。
        /// </summary>
        /// <param name="value">要获取字节数量的值。</param>
        /// <returns>返回所需字节数。</returns>
        long GetLength(T value);
    }
}
