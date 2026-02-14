
namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 序列缓冲区提供者的抽象基类，定义了创建和回收 <see cref="SequenceBuffer{T}"/> 实例的基本接口和流程。 具体的缓冲区池实现（如 <see cref="DefaultSequenceBufferProvider{T}"/>）应继承此类并实现相关方法以管理缓冲区的生命周期。 该设计允许灵活替换不同的缓冲区池实现，以适应不同的性能需求和使用场景。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SequenceBufferProvider<T> : AbstractBufferProvider<T, SequenceBuffer<T>>
    {
        public static SequenceBufferProvider<T> Shared => DefaultSequenceBufferProvider<T>.Default;
    }
}
