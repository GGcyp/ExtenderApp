namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 表示一次键盘弹起事件，包括主键和修饰键。 用于记录或传递用户释放的具体按键及其组合状态。
    /// </summary>
    public readonly struct KeyUpEvent
    {
        /// <summary>
        /// 触发事件的主键。
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// 事件发生时按下的修饰键（如 Ctrl、Shift、Alt 等）。
        /// </summary>
        public ModifierKeys ModifierKeys { get; }

        /// <summary>
        /// 是否为重复按键事件（此属性在弹起事件中通常为 false）。
        /// </summary>
        public bool IsRepeat { get; }

        /// <summary>
        /// 获取此键盘事件是否为全局事件。
        /// </summary>
        public bool IsGlobal { get; }

        /// <summary>
        /// 构造键盘弹起事件。
        /// </summary>
        /// <param name="key">主键</param>
        /// <param name="modifierKeys">修饰键</param>
        /// <param name="isRepeat">是否为重复按键</param>
        /// <param name="isGlobal">是否为全局事件</param>
        public KeyUpEvent(Key key, ModifierKeys modifierKeys, bool isRepeat, bool isGlobal)
        {
            Key = key;
            ModifierKeys = modifierKeys;
            IsRepeat = isRepeat;
            IsGlobal = isGlobal;
        }
    }
}