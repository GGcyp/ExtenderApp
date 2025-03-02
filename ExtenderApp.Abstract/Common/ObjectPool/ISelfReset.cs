

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个可重置的接口，支持设置重置行为。
    /// </summary>
    /// <typeparam name="T">重置行为所接受的参数类型。</typeparam>
    public interface ISelfReset : IResettable
    {
        /// <summary>
        /// 设置重置行为。
        /// </summary>
        /// <param name="action">当重置时执行的动作，接受一个类型为T的参数。</param>
        public void SetReset(Action<object> action);
    }
}
