using System.Reflection;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制格式化器核心方法的反射信息聚合。
    /// </summary>
    /// <remarks>
    /// 用于缓存并复用与某个具体格式化器相关的反射调用入口，典型包含：
    /// 1) Serialize(ref ByteBuffer, T)；
    /// 2) Deserialize(ref ByteBuffer)；
    /// 3) GetLength(T)。
    /// 这些 <see cref="MethodInfo"/> 通常来自已闭包的泛型格式化器实例方法。允许为 null，表示对应能力不支持或未提供。
    /// </remarks>
    public struct BinaryFormatterMethodInfoDetails
    {
        /// <summary>
        /// 写入方法的反射信息，形如：void Serialize(ref ByteBuffer Block, T value)。
        /// </summary>
        public MethodInfo Serialize { get; }

        /// <summary>
        /// 读取方法的反射信息，形如：T Deserialize(ref ByteBuffer Block)。
        /// </summary>
        public MethodInfo Deserialize { get; }

        /// <summary>
        /// 估算长度方法的反射信息，形如：long GetLength(T value)。
        /// </summary>
        public MethodInfo GetLength { get; }

        /// <summary>
        /// 获取一个值，指示是否所有方法信息均为 null。
        /// </summary>
        public bool IsEmpty => Serialize is null && Deserialize is null && GetLength is null;

        public BinaryFormatterMethodInfoDetails(MethodInfo serialize, MethodInfo deserialize, MethodInfo getLength)
        {
            Serialize = serialize;
            Deserialize = deserialize;
            GetLength = getLength;
        }
    }
}
