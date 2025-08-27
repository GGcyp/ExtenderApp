using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义版本化数据格式化器管理器的接口，用于管理多个版本化的数据格式化器。
    /// </summary>
    /// <remarks>
    /// 该接口主要用于在需要处理多个不同版本数据格式化器的场景中，
    /// 提供统一的格式化器添加和管理功能。
    /// </remarks>
    public interface IVersionDataFormatterMananger : IBinaryFormatter
    {
        /// <summary>
        /// 添加一个版本化数据格式化器到管理器中。
        /// </summary>
        /// <param name="formatter">
        /// 要添加的版本化数据格式化器实例，必须实现 <see cref="IVersionDataFormatter"/> 接口。
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// 当传入的格式化器为 null 时抛出此异常。
        /// </exception>
        void AddFormatter(object formatter);
    }


    /// <summary>
    /// 提供版本化数据序列化、反序列化及长度计算功能的接口。
    /// 实现了 <see cref="IBinaryFormatter{T}"/> 和 <see cref="IVersionDataFormatterMananger"/> 接口。
    /// </summary>
    /// <typeparam name="T">需要处理的数据类型。</typeparam>
    public interface IVersionDataFormatterMananger<T> : IBinaryFormatter<T>, IVersionDataFormatterMananger
    {
        /// <summary>
        /// 将指定值序列化到指定的二进制写入器中。
        /// </summary>
        /// <param name="writer">要写入数据的二进制写入器，通过引用传递。</param>
        /// <param name="value">要序列化的值。</param>
        /// <param name="version">指定的版本信息，用于控制序列化过程。</param>
        /// <returns>序列化后的字节数。</returns>
        void Serialize(ref ExtenderBinaryWriter writer, T value, Version version);

        /// <summary>
        /// 从二进制读取器中读取数据并反序列化为指定类型的对象。
        /// </summary>
        /// <param name="reader">用于读取二进制数据的 <see cref="Data.ExtenderBinaryReader"/> 实例，通过引用传递。</param>
        /// <param name="version">指定的版本信息，用于控制反序列化过程。</param>
        /// <returns>反序列化得到的对象。</returns>
        T Deserialize(ref ExtenderBinaryReader reader, Version version);

        /// <summary>
        /// 计算指定值在指定版本下序列化后的字节长度。
        /// </summary>
        /// <param name="value">要计算长度的值。</param>
        /// <param name="version">指定的版本信息。</param>
        /// <returns>序列化后的字节长度。</returns>
        long GetLength(T value, Version version);
    }
}
