using ExtenderApp.Abstract.Options;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义通用选项管理的接口，支持选项的注册、获取、设置和尝试操作。
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// 获取所有已注册的公开选项标识符和对应的选项值。
        /// </summary>
        IEnumerable<(OptionIdentifier, OptionValue)> RegisteredOptionsIdentifier { get; }

        /// <summary>
        /// 当注册或注销选项时触发的事件，提供选项标识符和对应的选项值信息。
        /// </summary>
        event EventHandler<(OptionIdentifier, OptionValue)>? RegisterOptionEvent;

        /// <summary>
        /// 当注销选项时触发的事件，提供选项标识符和对应的选项值信息。
        /// </summary>
        event EventHandler<(OptionIdentifier, OptionValue)>? UnRegisterOptionEvent;

        #region RegisterOption

        /// <summary>
        /// 注册指定标识符的选项，使用默认值。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        void RegisterOption<T>(OptionIdentifier<T> identifier);

        /// <summary>
        /// 注册指定标识符的选项，使用给定值。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        void RegisterOption<T>(OptionIdentifier<T> identifier, T value);

        /// <summary>
        /// 注册指定标识符的选项，使用给定值和变更事件处理器。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <param name="valueChangeHandler">选项值变更事件处理器。</param>
        void RegisterOption<T>(OptionIdentifier<T> identifier, T value, EventHandler<(OptionIdentifier, T)> valueChangeHandler);

        /// <summary>
        /// 注销指定标识符的选项。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        void UnRegisterOption(OptionIdentifier identifier);

        /// <summary>
        /// 尝试注销指定标识符的选项。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <returns>注销成功返回 true，否则返回 false。</returns>
        bool TryUnRegisterOption(OptionIdentifier identifier);

        #endregion RegisterOption

        #region RegisterOptionChange

        /// <summary>
        /// 注册选项值变更事件。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="handler">变更事件处理器。</param>
        /// <exception cref="InvalidOperationException">未找到指定的选项标识符。</exception>
        void RegisterOptionChange<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler);

        /// <summary>
        /// 注销选项值变更事件。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="handler">变更事件处理器。</param>
        /// <exception cref="InvalidOperationException">未找到指定的选项标识符。</exception>
        void UnregisterOptionChange<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler);

        #endregion RegisterOptionChange

        #region GetOption

        /// <summary>
        /// 获取指定类型标识的选项值。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">类型安全的选项标识符。</param>
        /// <returns>类型安全的选项值。</returns>
        T GetOptionValue<T>(OptionIdentifier<T> identifier);

        /// <summary>
        /// 尝试获取指定类型标识的选项对象。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">类型安全的选项标识符。</param>
        /// <param name="value">输出参数，返回类型安全的选项对象。</param>
        /// <returns>若获取成功则为 true，否则为 false。</returns>
        bool TryGetOptionValue<T>(OptionIdentifier<T> identifier, out T value);

        #endregion GetOption

        #region SetOption

        /// <summary>
        /// 设置指定类型标识的选项值。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">类型安全的选项标识符。</param>
        /// <param name="value">选项值。</param>
        void SetOptionValue<T>(OptionIdentifier<T> identifier, T value);

        /// <summary>
        /// 尝试设置指定类型标识的选项值。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">类型安全的选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <returns>若设置成功则为 true，否则为 false。</returns>
        bool TrySetOptionValue<T>(OptionIdentifier<T> identifier, T value);

        #endregion SetOption
    }
}