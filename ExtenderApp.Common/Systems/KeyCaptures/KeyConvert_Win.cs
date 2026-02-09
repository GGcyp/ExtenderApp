using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Systems.KeyCaptures
{
    /// <summary>
    /// 提供 Key 与系统虚拟键码（int）之间的转换方法。
    /// </summary>
    public class KeyConvert_Win
    {
        /// <summary>
        /// 支持的虚拟键码最大值（含）。Windows VK 通常位于 0x00 ~ 0xFF。
        /// </summary>
        public const byte MaxVkCode = 0xFF;

        /// <summary>
        /// 映射表存储容器。
        /// - 下半区用于 Key→VK 的查找（由构造与初始化过程写入）。
        /// - 上半区用于 VK→Key 的查找（由 <see cref="AddKey(int, Key)"/> 写入）。
        /// </summary>
        private byte[] keys;

        /// <summary>
        /// 构造转换器并初始化常见按键的映射关系。
        /// </summary>
        public KeyConvert_Win()
        {
            byte[] ints = (byte[])Enum.GetValues(typeof(Key));
            keys = new byte[MaxVkCode * 2];
            Array.Copy(ints, 0, keys, 0, ints.Length);
            InitVKCode();
        }

        /// <summary>
        /// 初始化常见 VK ↔ Key 的映射表。 仅设置 VK→Key 的映射条目。
        /// </summary>
        private void InitVKCode()
        {
            AddKey(0x41, Key.A);
            AddKey(0x42, Key.B);
            AddKey(0x43, Key.C);
            AddKey(0x44, Key.D);
            AddKey(0x45, Key.E);
            AddKey(0x46, Key.F);
            AddKey(0x47, Key.G);
            AddKey(0x48, Key.H);
            AddKey(0x49, Key.I);
            AddKey(0x4A, Key.J);
            AddKey(0x4B, Key.K);
            AddKey(0x4C, Key.L);
            AddKey(0x4D, Key.M);
            AddKey(0x4E, Key.N);
            AddKey(0x4F, Key.O);
            AddKey(0x50, Key.P);
            AddKey(0x51, Key.Q);
            AddKey(0x52, Key.R);
            AddKey(0x53, Key.S);
            AddKey(0x54, Key.T);
            AddKey(0x55, Key.U);
            AddKey(0x56, Key.V);
            AddKey(0x57, Key.W);
            AddKey(0x58, Key.X);
            AddKey(0x59, Key.Y);
            AddKey(0x5A, Key.Z);
            AddKey(0x30, Key.D0);
            AddKey(0x31, Key.D1);
            AddKey(0x32, Key.D2);
            AddKey(0x33, Key.D3);
            AddKey(0x34, Key.D4);
            AddKey(0x35, Key.D5);
            AddKey(0x36, Key.D6);
            AddKey(0x37, Key.D7);
            AddKey(0x38, Key.D8);
            AddKey(0x39, Key.D9);
            AddKey(0x0D, Key.Enter);
            AddKey(0x1B, Key.Escape);
            AddKey(0x09, Key.Tab);
            AddKey(0x20, Key.Space);
            AddKey(0x25, Key.Left);
            AddKey(0x26, Key.Up);
            AddKey(0x27, Key.Right);
            AddKey(0x28, Key.Down);
            AddKey(0x70, Key.F1);
            AddKey(0x71, Key.F2);
            AddKey(0x72, Key.F3);
            AddKey(0x73, Key.F4);
            AddKey(0x74, Key.F5);
            AddKey(0x75, Key.F6);
            AddKey(0x76, Key.F7);
            AddKey(0x77, Key.F8);
            AddKey(0x78, Key.F9);
            AddKey(0x79, Key.F10);
            AddKey(0x7A, Key.F11);
            AddKey(0x7B, Key.F12);
            AddKey(0xA0, Key.LeftShift);
            AddKey(0xA1, Key.RightShift);
            AddKey(0xA2, Key.LeftCtrl);
            AddKey(0xA3, Key.RightCtrl);
            AddKey(0xA4, Key.LeftAlt);
            AddKey(0xA5, Key.RightAlt);
            AddKey(0x5B, Key.LWin);
            AddKey(0x5C, Key.RWin);
            AddKey(0x2E, Key.Delete);
            AddKey(0x2D, Key.Insert);
            AddKey(0x24, Key.Home);
            AddKey(0x23, Key.End);
            AddKey(0x21, Key.PageUp);
            AddKey(0x22, Key.PageDown);
            AddKey(0x2C, Key.PrintScreen);
            AddKey(0x90, Key.NumLock);
            //AddKey(0x91, Key.ScrollLock);
            AddKey(0x14, Key.CapsLock);
            AddKey(0x6A, Key.Multiply);
            AddKey(0x6B, Key.Add);
            AddKey(0x6D, Key.Subtract);
            AddKey(0x6E, Key.Decimal);
            AddKey(0x6F, Key.Divide);
            AddKey(0x60, Key.Num0);
            AddKey(0x61, Key.Num1);
            AddKey(0x62, Key.Num2);
            AddKey(0x63, Key.Num3);
            AddKey(0x64, Key.Num4);
            AddKey(0x65, Key.Num5);
            AddKey(0x66, Key.Num6);
            AddKey(0x67, Key.Num7);
            AddKey(0x68, Key.Num8);
            AddKey(0x69, Key.Num9);
            AddKey(0xBA, Key.Oem1);
            AddKey(0xBB, Key.OemPlus);
            AddKey(0xBC, Key.OemComma);
            AddKey(0xBD, Key.OemMinus);
            AddKey(0xBE, Key.OemPeriod);
            AddKey(0xBF, Key.Oem2);
            AddKey(0xC0, Key.Oem3);
            AddKey(0xDB, Key.Oem4);
            AddKey(0xDC, Key.Oem5);
            AddKey(0xDD, Key.Oem6);
            AddKey(0xDE, Key.Oem7);
            AddKey(0xDF, Key.Oem8);
            AddKey(0xE2, Key.Oem102);
            AddKey(0x5D, Key.OemBackslash);
        }

        /// <summary>
        /// 记录一条 VK→Key 的映射。
        /// </summary>
        /// <param name="vkCode">Windows 虚拟键码（0~255）。</param>
        /// <param name="key">对应的 <see cref="Key"/>。</param>
        private void AddKey(int vkCode, Key key)
        {
            keys[MaxVkCode + vkCode] = (byte)key;
        }

        /// <summary>
        /// 将系统虚拟键码（int）转换为 <see cref="Key"/>。
        /// </summary>
        /// <param name="vkCode">系统虚拟键码，建议范围 0~255。</param>
        /// <returns>对应的 <see cref="Key"/> 枚举值；若未映射则返回 <see cref="Key.None"/>。</returns>
        public Key Convert(int vkCode)
        {
            return (Key)keys[vkCode + MaxVkCode];
        }

        /// <summary>
        /// 将 Key 枚举转换为系统虚拟键码（int）。
        /// </summary>
        /// <param name="key">Key 枚举值</param>
        /// <returns>对应的系统虚拟键码</returns>
        public byte Convert(Key key)
        {
            return keys[(int)key];
        }
    }
}