using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 抽象基类 Linker，继承自 ConcurrentOperate 类，实现了 ILinker 接口。
    /// </summary>
    /// <typeparam name="TPolicy">操作策略类型，需要继承自 LinkOperatePolicy 并约束 TData 类型。</typeparam>
    /// <typeparam name="TData">数据类型，需要继承自 LinkerData。</typeparam>
    public abstract class Linker : DisposableObject, ILinker
    {

    }
}
