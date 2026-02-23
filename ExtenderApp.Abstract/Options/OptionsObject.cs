using System.Collections.Concurrent;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract.Options
{
    /// <summary>
    /// 通用选项管理基类，支持选项注册、获取、设置和资源释放。
    /// </summary>
    public class OptionsObject : DisposableObject, IOptions
    {
        /// <summary>
        /// 存储所有已注册的选项。
        /// </summary>
        private readonly ConcurrentDictionary<OptionIdentifier, OptionValue> _options;

        ///<inheritdoc/>
        public event EventHandler<(OptionIdentifier, OptionValue)>? RegisterOptionEvent;

        ///<inheritdoc/>
        public event EventHandler<(OptionIdentifier, OptionValue)>? UnRegisterOptionEvent;

        ///<inheritdoc/>
        public IEnumerable<(OptionIdentifier, OptionValue)> RegisteredOptionsIdentifier => _options.Where(kv => kv.Key.GetVisibility == OptionVisibility.Public).Select(kv => (kv.Key, kv.Value));

        public OptionsObject(IOptions options) : this()
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            RegisterOptions(options, true);
        }

        public OptionsObject()
        {
            _options = new();
        }

        #region RegisterOption

        ///<inheritdoc/>
        public void RegisterOption<T>(OptionIdentifier<T> identifier)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            OptionValue<T> optionValue = identifier.GetDefaultOptionValue();

            RegisterOption(identifier, optionValue);
        }

        ///<inheritdoc/>
        public void RegisterOption<T>(OptionIdentifier<T> identifier, T value)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            OptionValue<T> optionValue = new(identifier, value);

            RegisterOption(identifier, optionValue);
        }

        ///<inheritdoc/>
        public void RegisterOption<T>(OptionIdentifier<T> identifier, T value, EventHandler<(OptionIdentifier, T)> valueChangeHandler)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            OptionValue<T> optionValue = new(identifier, value);
            optionValue.ChangedHandler += valueChangeHandler;

            RegisterOption(identifier, optionValue);
        }

        /// <summary>
        /// 注册选项对象到当前选项管理器中，并触发注册事件。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="optionValue">选项值。</param>
        /// <exception cref="InvalidOperationException">当选项标识符已存在时抛出。</exception>
        private void RegisterOption(OptionIdentifier identifier, OptionValue optionValue)
        {
            if (!_options.ContainsKey(identifier))
            {
                _options.TryAdd(identifier, optionValue);
                OnRegisterOption(identifier, optionValue);
                RegisterOptionEvent?.Invoke(this, (identifier, optionValue));
            }
            else
                throw new InvalidOperationException($"当前选项标识符已存在，无法重复注册。{identifier}");
        }

        /// <summary>
        /// 将另一个选项对象中的选项注册到当前对象中。
        /// </summary>
        /// <param name="options">要注册的选项对象。</param>
        /// <param name="needRegisterChange">指示是否需要注册值变化事件。</param>
        /// <exception cref="ArgumentException">当 options 不是 OptionsObject 类型时抛出。</exception>
        /// <exception cref="InvalidOperationException">当选项标识符已存在时抛出。</exception>
        protected void RegisterOptions(IOptions options, bool needRegisterChange)
        {
            foreach (var (identifier, optionValue) in options.RegisteredOptionsIdentifier)
            {
                if (!_options.ContainsKey(identifier))
                    _options.TryAdd(identifier, optionValue!.Clone(needRegisterChange));
                else
                    throw new InvalidOperationException($"当前选项标识符已存在，无法重复注册。{identifier}");
            }
        }

        /// <summary>
        /// 当注册选项时触发的虚方法，允许子类在选项注册后执行额外的逻辑。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="optionValue">选项值。</param>
        protected virtual void OnRegisterOption(OptionIdentifier identifier, OptionValue optionValue)
        {
        }

        #endregion RegisterOption

        #region UnRegisterOption

        /// <summary>
        /// 注销指定标识符的选项。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        public void UnRegisterOption(OptionIdentifier identifier)
        {
            UnRegisterOptionByVisibility(identifier, OptionVisibility.Public);
        }

        /// <summary>
        /// 尝试注销指定标识符的选项。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <returns>注销成功返回 true，否则返回 false。</returns>
        public bool TryUnRegisterOption(OptionIdentifier identifier)
        {
            return TryUnRegisterOptionByVisibility(identifier, OptionVisibility.Public);
        }

        /// <summary>
        /// 注销指定标识符的内部选项。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        protected internal void UnRegisterOptionInternal(OptionIdentifier identifier)
        {
            UnRegisterOptionByVisibility(identifier, OptionVisibility.Internal);
        }

        /// <summary>
        /// 尝试注销指定标识符的内部选项。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <returns>注销成功返回 true，否则返回 false。</returns>
        protected internal bool TryUnRegisterOptionInternal(OptionIdentifier identifier)
        {
            return TryUnRegisterOptionByVisibility(identifier, OptionVisibility.Internal);
        }

        /// <summary>
        /// 注销指定标识符的受保护选项。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        protected void UnRegisterOptionProtected(OptionIdentifier identifier)
        {
            UnRegisterOptionByVisibility(identifier, OptionVisibility.Protected);
        }

        /// <summary>
        /// 尝试注销指定标识符的受保护选项。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <returns>注销成功返回 true，否则返回 false。</returns>
        protected bool TryUnRegisterOptionProtected(OptionIdentifier identifier)
        {
            return TryUnRegisterOptionByVisibility(identifier, OptionVisibility.Protected);
        }

        /// <summary>
        /// 根据可见性注销指定标识符的选项。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="visibility">可见性。</param>
        private void UnRegisterOptionByVisibility(OptionIdentifier identifier, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));

            if (!CheckOptionVisibility(identifier.SetVisibility | identifier.GetVisibility, visibility))
                throw new InvalidOperationException($"指定的选项标识符不可见，无法删除选项。{identifier}");

            if (_options.TryRemove(identifier, out var value))
            {
                value.Dispose();
                OnUnRegisterOption(identifier, value);
                UnRegisterOptionEvent?.Invoke(this, (identifier, value));
            }
        }

        /// <summary>
        /// 根据可见性尝试注销指定标识符的选项。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="visibility">可见性。</param>
        /// <returns>注销成功返回 true，否则返回 false。</returns>
        private bool TryUnRegisterOptionByVisibility(OptionIdentifier identifier, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            if (!CheckOptionVisibility(identifier.SetVisibility | identifier.GetVisibility, visibility))
                return false;

            if (_options.TryRemove(identifier, out var value))
            {
                value.Dispose();
                OnUnRegisterOption(identifier, value);
                UnRegisterOptionEvent?.Invoke(this, (identifier, value));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 当注销选项时触发的虚方法，允许子类在选项注销后执行额外的逻辑。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="optionValue">选项值。</param>
        protected virtual void OnUnRegisterOption(OptionIdentifier identifier, OptionValue optionValue)
        {
        }

        #endregion UnRegisterOption

        #region RegisterOptionChange

        ///<inheritdoc/>
        public void RegisterOptionChange<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler)
        {
            RegisterOptionChangeByVisibility(identifier, handler, OptionVisibility.Public);
        }

        ///<inheritdoc/>
        public void UnregisterOptionChange<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler)
        {
            UnregisterOptionChangeByVisibility(identifier, handler);
        }

        /// <summary>
        /// 注册内部选项值变更事件。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="handler">变更事件处理器。</param>
        /// <exception cref="InvalidOperationException">未找到指定的选项标识符。</exception>
        protected internal void RegisterOptionChangeInternal<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler)
        {
            RegisterOptionChangeByVisibility(identifier, handler, OptionVisibility.Internal);
        }

        /// <summary>
        /// 注册受保护选项值变更事件。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="handler">变更事件处理器。</param>
        /// <exception cref="InvalidOperationException">未找到指定的选项标识符。</exception>
        protected void RegisterOptionChangeProtected<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler)
        {
            RegisterOptionChangeByVisibility(identifier, handler, OptionVisibility.Protected);
        }

        /// <summary>
        /// 根据可见性注册选项值变更事件。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="handler">变更事件处理器。</param>
        /// <param name="visibility">可见性。</param>
        /// <exception cref="InvalidOperationException">指定的选项标识符不可见或未找到。</exception>
        private void RegisterOptionChangeByVisibility<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            if (CheckOptionVisibility(identifier.GetVisibility, visibility))
                throw new InvalidOperationException($"指定的选项标识符不可见，无法注册选项值变更事件。{identifier}");

            if (!TryGetOptionValue(identifier, out OptionValue<T> optionValue))
                throw new InvalidOperationException($"未找到指定的选项标识符，无法注册选项值变更事件。{identifier}");

            optionValue.ChangedHandler += handler;
        }

        /// <summary>
        /// 根据可见性注销选项值变更事件。
        /// </summary>
        /// <typeparam name="T">选项值的类型。</typeparam>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="handler">变更事件处理器。</param>
        /// <exception cref="InvalidOperationException">未找到指定的选项标识符。</exception>
        private void UnregisterOptionChangeByVisibility<T>(OptionIdentifier<T> identifier, EventHandler<(OptionIdentifier, T)> handler)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));

            if (!TryGetOptionValue(identifier, out OptionValue<T> optionValue))
                throw new InvalidOperationException($"未找到指定的选项标识符，无法注销选项值变更事件。{identifier}");

            optionValue.ChangedHandler -= handler;
        }

        #endregion RegisterOptionChange

        #region GetOption

        ///<inheritdoc/>
        public T GetOptionValue<T>(OptionIdentifier<T> identifier)
        {
            return GetOptionByVisibility(identifier, OptionVisibility.Public);
        }

        ///<inheritdoc/>
        public bool TryGetOptionValue<T>(OptionIdentifier<T> identifier, out T value)
        {
            return TryGetOptionByVisibility(identifier, out value, OptionVisibility.Public);
        }

        /// <summary>
        /// 获取指定标识符的内部选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <returns>选项值。</returns>
        protected internal T GetOptionInternalValue<T>(OptionIdentifier<T> identifier)
        {
            return GetOptionByVisibility(identifier, OptionVisibility.Internal);
        }

        /// <summary>
        /// 尝试获取指定标识符的内部选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        protected internal bool TryGetOptionInternalValue<T>(OptionIdentifier<T> identifier, out T value)
        {
            return TryGetOptionByVisibility(identifier, out value, OptionVisibility.Internal);
        }

        /// <summary>
        /// 获取指定标识符的私有选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <returns>选项值。</returns>
        protected T GetOptionProtectedValue<T>(OptionIdentifier<T> identifier)
        {
            return GetOptionByVisibility(identifier, OptionVisibility.Protected);
        }

        /// <summary>
        /// 尝试获取指定标识符的私有选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        protected bool TryGetOptionProtected<T>(OptionIdentifier<T> identifier, out T value)
        {
            return TryGetOptionByVisibility(identifier, out value, OptionVisibility.Protected);
        }

        /// <summary>
        /// 根据可见性获取指定标识符的选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="visibility">可见性。</param>
        /// <returns>选项值对象。</returns>
        private T GetOptionByVisibility<T>(OptionIdentifier<T> identifier, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            if (!CheckOptionVisibility(identifier.GetVisibility, visibility))
                throw new InvalidOperationException($"指定的选项标识符不可见，无法获取选项值。{identifier}");

            if (TryGetOptionValue(identifier, out OptionValue<T>? optionValue))
            {
                return optionValue.Value;
            }
            throw new KeyNotFoundException($"未找到指定的选项标识符，无法获取选项值。{identifier}");
        }

        /// <summary>
        /// 根据可见性尝试获取指定标识符的选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <param name="visibility">可见性。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        private bool TryGetOptionByVisibility<T>(OptionIdentifier<T> identifier, out T value, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            value = default!;
            if (!CheckOptionVisibility(identifier.GetVisibility, visibility))
                throw new InvalidOperationException($"指定的选项标识符不可见，无法获取选项值。{identifier}");

            if (TryGetOptionValue(identifier, out OptionValue<T> optionValue))
            {
                value = optionValue.Value;
                return true;
            }
            return false;
        }

        #endregion GetOption

        #region SetOption

        ///<inheritdoc/>
        public void SetOptionValue<T>(OptionIdentifier<T> identifier, T value)
        {
            SetOptionByVisibility(identifier, value, OptionVisibility.Public);
        }

        ///<inheritdoc/>
        public bool TrySetOptionValue<T>(OptionIdentifier<T> identifier, T value)
        {
            return TrySetOptionByVisibility(identifier, value, OptionVisibility.Public);
        }

        /// <summary>
        /// 设置指定标识符的内部选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        protected internal void SetOptionInternal<T>(OptionIdentifier<T> identifier, T value)
        {
            SetOptionByVisibility(identifier, value, OptionVisibility.Internal);
        }

        /// <summary>
        /// 尝试设置指定标识符的内部选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <returns>设置成功返回 true，否则返回 false。</returns>
        protected internal bool TrySetOptionInternal<T>(OptionIdentifier<T> identifier, T value)
        {
            return TrySetOptionByVisibility(identifier, value, OptionVisibility.Internal);
        }

        /// <summary>
        /// 设置指定标识符的私有选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        protected void SetOptionProtected<T>(OptionIdentifier<T> identifier, T value)
        {
            SetOptionByVisibility(identifier, value, OptionVisibility.Public);
        }

        /// <summary>
        /// 尝试设置指定标识符的私有选项值。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项值。</param>
        /// <returns>设置成功返回 true，否则返回 false。</returns>
        protected bool TrySetOptionProtected<T>(OptionIdentifier<T> identifier, T value)
        {
            return TrySetOptionByVisibility(identifier, value, OptionVisibility.Public);
        }

        /// <summary>
        /// 根据可见性设置指定标识符的选项对象。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项对象。</param>
        /// <param name="visibility">可见性。</param>
        private void SetOptionByVisibility<T>(OptionIdentifier<T> identifier, T value, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            if (!CheckOptionVisibility(identifier.SetVisibility, visibility))
                throw new InvalidOperationException($"指定的选项标识符不可见，无法设置选项值。{identifier}");

            if (!TryGetOptionValue(identifier, out OptionValue<T> optionValue))
                throw new InvalidOperationException($"未找到指定的选项标识符，无法设置选项值。{identifier}");

            optionValue.UpdateValue(this, (identifier, value));
            OnOptionValueChanged(identifier, optionValue);
        }

        /// <summary>
        /// 根据可见性尝试设置指定标识符的选项对象。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项对象。</param>
        /// <param name="visibility">可见性。</param>
        /// <returns>设置成功返回 true，否则返回 false。</returns>
        private bool TrySetOptionByVisibility<T>(OptionIdentifier<T> identifier, T value, OptionVisibility visibility)
        {
            ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
            if (!CheckOptionVisibility(identifier.SetVisibility, visibility))
                return false;

            if (TryGetOptionValue(identifier, out OptionValue<T> optionValue))
            {
                optionValue.UpdateValue(this, (identifier, value));
                OnOptionValueChanged(identifier, optionValue);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 当选项值发生变化时触发的虚方法，允许子类在选项值变化后执行额外的逻辑。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="optionValue">选项值对象。</param>
        protected virtual void OnOptionValueChanged(OptionIdentifier identifier, OptionValue optionValue)
        {
        }

        #endregion SetOption

        #region GetOptionValue

        /// <summary>
        /// 根据可见性尝试获取指定标识符的选项对象。
        /// </summary>
        /// <param name="identifier">选项标识符。</param>
        /// <param name="value">选项对象。</param>
        /// <param name="visibility">可见性。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        private bool TryGetOptionValue<T>(OptionIdentifier<T> identifier, out OptionValue<T> value)
        {
            value = default!;

            if (_options.TryGetValue(identifier, out var optionValue) &&
                optionValue is OptionValue<T> typedValue)
            {
                value = typedValue;
                return true;
            }
            return false;
        }

        #endregion GetOptionValue

        /// <summary>
        /// 检查指定可见性是否包含目标可见性。
        /// </summary>
        /// <param name="otheVisibility">选项标识符的可见性。</param>
        /// <param name="visibility">目标可见性。</param>
        /// <returns>包含返回 true，否则返回 false。</returns>
        private static bool CheckOptionVisibility(OptionVisibility otheVisibility, OptionVisibility visibility)
        {
            return (otheVisibility & visibility) != 0;
        }

        /// <summary>
        /// 释放所有已注册选项的资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            foreach (var option in _options.Values)
            {
                option.Dispose();
            }
        }
    }
}