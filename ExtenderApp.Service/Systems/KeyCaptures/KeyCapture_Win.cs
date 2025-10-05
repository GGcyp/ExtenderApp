using System.Runtime.InteropServices;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Common;
using ExtenderApp.Common.Systems;

namespace ExtenderApp.Services.Systems.KeyCaptures
{
    /// <summary>
    /// Windows 全局低级键盘钩子实现。 负责安装/卸载 WH_KEYBOARD_LL
    /// 钩子，将系统键盘消息转换为 Key/ModifierKeys，
    /// 并分发给每个按键对应的 KeyConsume 实例。
    /// </summary>
    internal class KeyCapture_Win : DisposableObject, IKeyCapture
    {
        #region Windows API 导入

        /// <summary>
        /// 安装系统级钩子。
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// 卸载已安装的系统级钩子。
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// 传递消息给下一个钩子。
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 获取指定模块的句柄。
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary> 获取虚拟键的当前状态（<0 表示按下）。 </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern short GetKeyState(int vKey);

        /// <summary>
        /// 低级键盘钩子回调委托签名。
        /// </summary>
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // 常量定义
        private const int WH_KEYBOARD_LL = 13;     // 低级键盘钩子

        private const int WM_KEYDOWN = 0x0100;     // 普通按键按下
        private const int WM_KEYUP = 0x0101;       // 普通按键抬起
        private const int WM_SYSKEYDOWN = 0x0104;  // 系统按键按下（含 Alt）
        private const int WM_SYSKEYUP = 0x0105;    // 系统按键抬起

        #endregion Windows API 导入

        /// <summary>
        /// Key 与 VK 的转换器。
        /// </summary>
        private readonly KeyConvert_Win _convert;

        /// <summary>
        /// 以 VK 为索引的按键消费器表（懒加载）。
        /// </summary>
        private readonly KeyConsume[] _keyConsumes;

        /// <summary>
        /// 钩子回调委托的持有，防止被 GC 回收。
        /// </summary>
        private LowLevelKeyboardProc _proc;

        /// <summary>
        /// 当前安装的钩子句柄。
        /// </summary>
        private IntPtr _hookID = IntPtr.Zero;

        public bool IsStart { get; private set; }

        /// <summary>
        /// 初始化并安装键盘钩子。
        /// </summary>
        public KeyCapture_Win()
        {
            _convert = new();
            _keyConsumes = new KeyConsume[KeyConvert_Win.MaxVkCode];
            _proc = HookCallback;
            IsStart = false;
        }

        public void Start()
        {
            lock (_keyConsumes)
            {
                if (IsStart)
                    return;

                Hook();
                IsStart = true;
            }
        }

        public void Stop()
        {
            lock (_keyConsumes)
            {
                if (!IsStart)
                    return;

                Unhook();
                IsStart = false;
            }
        }

        /// <summary>
        /// 安装低级键盘钩子。
        /// </summary>
        public void Hook()
        {
            _hookID = SetHook(_proc);
        }

        /// <summary>
        /// 卸载低级键盘钩子。
        /// </summary>
        public void Unhook()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 调用 Win32 API 安装钩子并返回钩子句柄。
        /// </summary>
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var module = System.Diagnostics.Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(module.ModuleName), 0);
            }
        }

        /// <summary>
        /// 钩子回调（系统线程执行）。 解析
        /// VK、计算修饰键与按下/抬起状态，派发给对应的 KeyConsume。
        /// 注意：应保持逻辑简短快速，并始终调用 CallNextHookEx 以不阻断其他钩子。
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // 仅处理有效消息
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam); // 虚拟键码
                Key key = _convert.Convert(vkCode);

                // 提取按键状态（按下/抬起）和修饰键
                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                ModifierKeys modifiers = GetModifiers();

                var consume = GetKeyConsume(key);
                if (consume != null)
                {
                    consume.Notify(isKeyDown, modifiers);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 读取当前修饰键（Ctrl/Shift/Alt）的状态。
        /// </summary>
        private ModifierKeys GetModifiers()
        {
            ModifierKeys mods = ModifierKeys.None;
            if (GetKeyState((int)VirtualKeyStates.VK_CONTROL) < 0) mods |= ModifierKeys.Control;
            if (GetKeyState((int)VirtualKeyStates.VK_SHIFT) < 0) mods |= ModifierKeys.Shift;
            if (GetKeyState((int)VirtualKeyStates.VK_MENU) < 0) mods |= ModifierKeys.Alt;
            return mods;
        }

        /// <summary>
        /// 为指定键注册订阅者的处理器。 同一对象与修饰键组合会被合并处理（参见 KeyConsume.Add）。
        /// </summary>
        /// <param name="key">要监听的主键。</param>
        /// <param name="obj">订阅者目标对象。</param>
        /// <param name="keyDownHandler">按下事件处理器（可选）。</param>
        /// <param name="keyupHandler">抬起事件处理器（可选）。</param>
        /// <param name="modifierKeys">匹配所需的修饰键组合。</param>
        public void Register(Key key, object obj,
            EventHandler<KeyEvent>? keyDownHandler,
            EventHandler<KeyEvent>? keyupHandler,
            ModifierKeys modifierKeys = ModifierKeys.None)
        {
            var consume = GetKeyConsume(key) ?? new(key);
            consume.Add(obj, modifierKeys, keyDownHandler, keyupHandler);
        }


        public void Register(object obj,
            EventHandler<KeyEvent>? keyDownHandler,
            EventHandler<KeyEvent>? keyupHandler,
            ModifierKeys modifierKeys = ModifierKeys.None)
        {
            for (int i = 0; i < _keyConsumes.Length; i++)
            {
                var consume = _keyConsumes[i];
                if (consume == null)
                {
                    Key key = _convert.Convert(i);
                    if (key == Key.None)
                        continue;
                    _keyConsumes[i] = consume = new(key);
                }
                consume.Add(obj, modifierKeys, keyDownHandler, keyupHandler);
            }
        }

        /// <summary>
        /// 取消指定对象在某键上的所有订阅。
        /// </summary>
        /// <param name="key">目标主键。</param>
        /// <param name="obj">订阅者目标对象。</param>
        public void UnRegister(Key key, object obj)
        {
            var consume = GetKeyConsume(key);
            if (consume == null)
                return;

            consume.Remove(obj);
        }

        public void UnRegister(object obj)
        {
            for (int i = 0; i < _keyConsumes.Length; i++)
            {
                var consume = _keyConsumes[i];
                if (consume != null)
                {
                    consume.Remove(obj);
                }
            }
        }

        /// <summary>
        /// 获取（并在必要时创建）某个 Key 对应的 KeyConsume。 通过
        /// VK 作为索引定位。
        /// </summary>
        private KeyConsume? GetKeyConsume(Key key)
        {
            int vkCode = _convert.Convert(key);
            if (vkCode < 0 || vkCode >= _keyConsumes.Length)
                throw new ArgumentOutOfRangeException(nameof(key), "按键超过范围");
            return _keyConsumes[vkCode] ??= new(key);
        }

        /// <summary>
        /// 虚拟键状态枚举（用于 <see
        /// cref="GetKeyState(int)"/> 判断）。
        /// </summary>
        private enum VirtualKeyStates : int
        {
            VK_CONTROL = 0x11,
            VK_SHIFT = 0x10,
            VK_MENU = 0x12
        }

        /// <summary>
        /// 释放钩子与相关资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            Unhook();
        }
    }
}