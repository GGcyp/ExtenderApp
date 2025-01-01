using BinaryWriter = ExtenderApp.Data.BinaryWriter;
using BinaryReader = ExtenderApp.Data.BinaryReader;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 二进制格式化器接口。
    /// </summary>
    public interface IBinaryFormatter
    {

    }

    /// <summary>
    /// 泛型二进制格式化器接口，继承自 <see cref="IBinaryFormatter"/>。
    /// </summary>
    /// <typeparam name="T">需要序列化和反序列化的类型。</typeparam>
    public interface IBinaryFormatter<T> : IBinaryFormatter
    {
        /// <summary>
        /// 将指定对象序列化为二进制数据并写入到 <see cref="Data.BinaryWriter"/> 中。
        /// </summary>
        /// <param name="writer"><see cref="Data.BinaryWriter"/> 实例，用于写入二进制数据。</param>
        /// <param name="value">需要序列化的对象。</param>
        void Serialize(ref BinaryWriter writer, T value);

        /// <summary>
        /// 从 <see cref="Data.BinaryReader"/> 中读取二进制数据并反序列化为指定类型的对象。
        /// </summary>
        /// <param name="reader"><see cref="Data.BinaryReader"/> 实例，用于读取二进制数据。</param>
        /// <returns>反序列化得到的对象。</returns>
        T Deserialize(ref BinaryReader reader);
    }
}
