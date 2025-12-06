namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一次键盘事件，包括主键和修饰键。 用于记录或传递用户按下的具体按键及其组合状态。
    /// </summary>
    public readonly struct KeyEvent
    {
        /// <summary>
        /// 触发事件的主键。
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// 当前按下的修饰键（如 Ctrl、Shift、Alt 等）。
        /// </summary>
        public ModifierKeys ModifierKeys { get; }

        /// <summary>
        /// 是否为重复按键事件（按住不放时会连续触发）。
        /// </summary>
        public bool IsRepeat { get; }

        /// <summary>
        /// 构造键盘事件。
        /// </summary>
        /// <param name="key">主键</param>
        /// <param name="modifierKeys">修饰键</param>
        /// <param name="isRepeat">是否为重复按键</param>
        public KeyEvent(Key key, ModifierKeys modifierKeys, bool isRepeat)
        {
            Key = key;
            ModifierKeys = modifierKeys;
            IsRepeat = isRepeat;
        }
    }
}