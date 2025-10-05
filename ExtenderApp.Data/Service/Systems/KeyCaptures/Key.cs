namespace ExtenderApp.Data
{
    /// <summary>
    /// Key 枚举定义了常用键盘按键的标识符。
    /// 用于表示各种标准键盘按键，包括功能键、字母、数字、控制键、媒体键等。
    /// 部分枚举值存在别名（如 Enter/Return），数值相同。
    /// </summary>
    public enum Key : byte
    {
        /// <summary>
        /// 无按键。
        /// </summary>
        None = 0,

        /// <summary>
        /// Cancel 键。
        /// </summary>
        Cancel = 1,

        /// <summary>
        /// Backspace 键。
        /// </summary>
        Back = 2,

        /// <summary>
        /// Tab 键。
        /// </summary>
        Tab = 3,

        /// <summary>
        /// Linefeed 键。
        /// </summary>
        LineFeed = 4,

        /// <summary>
        /// Clear 键。
        /// </summary>
        Clear = 5,

        /// <summary>
        /// Enter 键。
        /// </summary>
        Enter = 6,

        /// <summary>
        /// Return 键（与 Enter 相同）。
        /// </summary>
        Return = 6,

        /// <summary>
        /// Pause 键。
        /// </summary>
        Pause = 7,

        /// <summary>
        /// Caps Lock 键。
        /// </summary>
        Capital = 8,

        /// <summary>
        /// Caps Lock 键（别名）。
        /// </summary>
        CapsLock = 8,

        /// <summary>
        /// 韩文输入法模式键。
        /// </summary>
        HangulMode = 9,

        /// <summary>
        /// 日文假名输入法模式键（别名）。
        /// </summary>
        KanaMode = 9,

        /// <summary>
        /// Junja 输入法模式键。
        /// </summary>
        JunjaMode = 10,

        /// <summary>
        /// Final 输入法模式键。
        /// </summary>
        FinalMode = 11,

        /// <summary>
        /// 韩文汉字输入法模式键。
        /// </summary>
        HanjaMode = 12,

        /// <summary>
        /// 日文汉字输入法模式键（别名）。
        /// </summary>
        KanjiMode = 12,

        /// <summary>
        /// ESC 键。
        /// </summary>
        Escape = 13,

        /// <summary>
        /// IME 转换键。
        /// </summary>
        ImeConvert = 14,

        /// <summary>
        /// IME 非转换键。
        /// </summary>
        ImeNonConvert = 15,

        /// <summary>
        /// IME 接受键。
        /// </summary>
        ImeAccept = 16,

        /// <summary>
        /// IME 模式切换键。
        /// </summary>
        ImeModeChange = 17,

        /// <summary>
        /// 空格键。
        /// </summary>
        Space = 18,

        /// <summary>
        /// Page Up 键。
        /// </summary>
        PageUp = 19,

        /// <summary>
        /// Page Up 键（别名）。
        /// </summary>
        Prior = 19,

        /// <summary>
        /// Page Down 键。
        /// </summary>
        Next = 20,

        /// <summary>
        /// Page Down 键（别名）。
        /// </summary>
        PageDown = 20,

        /// <summary>
        /// End 键。
        /// </summary>
        End = 21,

        /// <summary>
        /// Home 键。
        /// </summary>
        Home = 22,

        /// <summary>
        /// 左方向键。
        /// </summary>
        Left = 23,

        /// <summary>
        /// 上方向键。
        /// </summary>
        Up = 24,

        /// <summary>
        /// 右方向键。
        /// </summary>
        Right = 25,

        /// <summary>
        /// 下方向键。
        /// </summary>
        Down = 26,

        /// <summary>
        /// Select 键。
        /// </summary>
        Select = 27,

        /// <summary>
        /// Print 键。
        /// </summary>
        Print = 28,

        /// <summary>
        /// Execute 键。
        /// </summary>
        Execute = 29,

        /// <summary>
        /// Print Screen 键。
        /// </summary>
        PrintScreen = 30,

        /// <summary>
        /// Print Screen 键（别名）。
        /// </summary>
        Snapshot = 30,

        /// <summary>
        /// Insert 键。
        /// </summary>
        Insert = 31,

        /// <summary>
        /// Delete 键。
        /// </summary>
        Delete = 32,

        /// <summary>
        /// Help 键。
        /// </summary>
        Help = 33,

        /// <summary>
        /// 数字 0 键。
        /// </summary>
        D0 = 34,

        /// <summary>
        /// 数字 1 键。
        /// </summary>
        D1 = 35,

        /// <summary>
        /// 数字 2 键。
        /// </summary>
        D2 = 36,

        /// <summary>
        /// 数字 3 键。
        /// </summary>
        D3 = 37,

        /// <summary>
        /// 数字 4 键。
        /// </summary>
        D4 = 38,

        /// <summary>
        /// 数字 5 键。
        /// </summary>
        D5 = 39,

        /// <summary>
        /// 数字 6 键。
        /// </summary>
        D6 = 40,

        /// <summary>
        /// 数字 7 键。
        /// </summary>
        D7 = 41,

        /// <summary>
        /// 数字 8 键。
        /// </summary>
        D8 = 42,

        /// <summary>
        /// 数字 9 键。
        /// </summary>
        D9 = 43,

        /// <summary>
        /// A 键。
        /// </summary>
        A = 44,

        /// <summary>
        /// B 键。
        /// </summary>
        B = 45,

        /// <summary>
        /// C 键。
        /// </summary>
        C = 46,

        /// <summary>
        /// D 键。
        /// </summary>
        D = 47,

        /// <summary>
        /// E 键。
        /// </summary>
        E = 48,

        /// <summary>
        /// F 键。
        /// </summary>
        F = 49,

        /// <summary>
        /// G 键。
        /// </summary>
        G = 50,

        /// <summary>
        /// H 键。
        /// </summary>
        H = 51,

        /// <summary>
        /// I 键。
        /// </summary>
        I = 52,

        /// <summary>
        /// J 键。
        /// </summary>
        J = 53,

        /// <summary>
        /// K 键。
        /// </summary>
        K = 54,

        /// <summary>
        /// L 键。
        /// </summary>
        L = 55,

        /// <summary>
        /// M 键。
        /// </summary>
        M = 56,

        /// <summary>
        /// N 键。
        /// </summary>
        N = 57,

        /// <summary>
        /// O 键。
        /// </summary>
        O = 58,

        /// <summary>
        /// P 键。
        /// </summary>
        P = 59,

        /// <summary>
        /// Q 键。
        /// </summary>
        Q = 60,

        /// <summary>
        /// R 键。
        /// </summary>
        R = 61,

        /// <summary>
        /// S 键。
        /// </summary>
        S = 62,

        /// <summary>
        /// T 键。
        /// </summary>
        T = 63,

        /// <summary>
        /// U 键。
        /// </summary>
        U = 64,

        /// <summary>
        /// V 键。
        /// </summary>
        V = 65,

        /// <summary>
        /// W 键。
        /// </summary>
        W = 66,

        /// <summary>
        /// X 键。
        /// </summary>
        X = 67,

        /// <summary>
        /// Y 键。
        /// </summary>
        Y = 68,

        /// <summary>
        /// Z 键。
        /// </summary>
        Z = 69,

        /// <summary>
        /// 左 Windows 键。
        /// </summary>
        LWin = 70,

        /// <summary>
        /// 右 Windows 键。
        /// </summary>
        RWin = 71,

        /// <summary>
        /// 应用程序键。
        /// </summary>
        Apps = 72,

        /// <summary>
        /// 休眠键。
        /// </summary>
        Sleep = 73,

        /// <summary>
        /// 数字小键盘 0 键。
        /// </summary>
        NumPad0 = 74,

        /// <summary>
        /// 数字小键盘 1 键。
        /// </summary>
        NumPad1 = 75,

        /// <summary>
        /// 数字小键盘 2 键。
        /// </summary>
        NumPad2 = 76,

        /// <summary>
        /// 数字小键盘 3 键。
        /// </summary>
        NumPad3 = 77,

        /// <summary>
        /// 数字小键盘 4 键。
        /// </summary>
        NumPad4 = 78,

        /// <summary>
        /// 数字小键盘 5 键。
        /// </summary>
        NumPad5 = 79,

        /// <summary>
        /// 数字小键盘 6 键。
        /// </summary>
        NumPad6 = 80,

        /// <summary>
        /// 数字小键盘 7 键。
        /// </summary>
        NumPad7 = 81,

        /// <summary>
        /// 数字小键盘 8 键。
        /// </summary>
        NumPad8 = 82,

        /// <summary>
        /// 数字小键盘 9 键。
        /// </summary>
        NumPad9 = 83,

        /// <summary>
        /// 乘号键。
        /// </summary>
        Multiply = 84,

        /// <summary>
        /// 加号键。
        /// </summary>
        Add = 85,

        /// <summary>
        /// 分隔符键。
        /// </summary>
        Separator = 86,

        /// <summary>
        /// 减号键。
        /// </summary>
        Subtract = 87,

        /// <summary>
        /// 小数点键。
        /// </summary>
        Decimal = 88,

        /// <summary>
        /// 除号键。
        /// </summary>
        Divide = 89,

        /// <summary>
        /// F1 功能键。
        /// </summary>
        F1 = 90,

        /// <summary>
        /// F2 功能键。
        /// </summary>
        F2 = 91,

        /// <summary>
        /// F3 功能键。
        /// </summary>
        F3 = 92,

        /// <summary>
        /// F4 功能键。
        /// </summary>
        F4 = 93,

        /// <summary>
        /// F5 功能键。
        /// </summary>
        F5 = 94,

        /// <summary>
        /// F6 功能键。
        /// </summary>
        F6 = 95,

        /// <summary>
        /// F7 功能键。
        /// </summary>
        F7 = 96,

        /// <summary>
        /// F8 功能键。
        /// </summary>
        F8 = 97,

        /// <summary>
        /// F9 功能键。
        /// </summary>
        F9 = 98,

        /// <summary>
        /// F10 功能键。
        /// </summary>
        F10 = 99,

        /// <summary>
        /// F11 功能键。
        /// </summary>
        F11 = 100,

        /// <summary>
        /// F12 功能键。
        /// </summary>
        F12 = 101,

        /// <summary>
        /// F13 功能键。
        /// </summary>
        F13 = 102,

        /// <summary>
        /// F14 功能键。
        /// </summary>
        F14 = 103,

        /// <summary>
        /// F15 功能键。
        /// </summary>
        F15 = 104,

        /// <summary>
        /// F16 功能键。
        /// </summary>
        F16 = 105,

        /// <summary>
        /// F17 功能键。
        /// </summary>
        F17 = 106,

        /// <summary>
        /// F18 功能键。
        /// </summary>
        F18 = 107,

        /// <summary>
        /// F19 功能键。
        /// </summary>
        F19 = 108,

        /// <summary>
        /// F20 功能键。
        /// </summary>
        F20 = 109,

        /// <summary>
        /// F21 功能键。
        /// </summary>
        F21 = 110,

        /// <summary>
        /// F22 功能键。
        /// </summary>
        F22 = 111,

        /// <summary>
        /// F23 功能键。
        /// </summary>
        F23 = 112,

        /// <summary>
        /// F24 功能键。
        /// </summary>
        F24 = 113,

        /// <summary>
        /// 数字锁定键。
        /// </summary>
        NumLock = 114,

        /// <summary>
        /// 滚动锁定键。
        /// </summary>
        Scroll = 115,

        /// <summary>
        /// 左 Shift 键。
        /// </summary>
        LeftShift = 116,

        /// <summary>
        /// 右 Shift 键。
        /// </summary>
        RightShift = 117,

        /// <summary>
        /// 左 Ctrl 键。
        /// </summary>
        LeftCtrl = 118,

        /// <summary>
        /// 右 Ctrl 键。
        /// </summary>
        RightCtrl = 119,

        /// <summary>
        /// 左 Alt 键。
        /// </summary>
        LeftAlt = 120,

        /// <summary>
        /// 右 Alt 键。
        /// </summary>
        RightAlt = 121,

        /// <summary>
        /// 浏览器后退键。
        /// </summary>
        BrowserBack = 122,

        /// <summary>
        /// 浏览器前进键。
        /// </summary>
        BrowserForward = 123,

        /// <summary>
        /// 浏览器刷新键。
        /// </summary>
        BrowserRefresh = 124,

        /// <summary>
        /// 浏览器停止键。
        /// </summary>
        BrowserStop = 125,

        /// <summary>
        /// 浏览器搜索键。
        /// </summary>
        BrowserSearch = 126,

        /// <summary>
        /// 浏览器收藏夹键。
        /// </summary>
        BrowserFavorites = 127,

        /// <summary>
        /// 浏览器主页键。
        /// </summary>
        BrowserHome = 128,

        /// <summary>
        /// 音量静音键。
        /// </summary>
        VolumeMute = 129,

        /// <summary>
        /// 音量降低键。
        /// </summary>
        VolumeDown = 130,

        /// <summary>
        /// 音量增加键。
        /// </summary>
        VolumeUp = 131,

        /// <summary>
        /// 媒体下一曲键。
        /// </summary>
        MediaNextTrack = 132,

        /// <summary>
        /// 媒体上一曲键。
        /// </summary>
        MediaPreviousTrack = 133,

        /// <summary>
        /// 媒体停止键。
        /// </summary>
        MediaStop = 134,

        /// <summary>
        /// 媒体播放/暂停键。
        /// </summary>
        MediaPlayPause = 135,

        /// <summary>
        /// 启动邮件键。
        /// </summary>
        LaunchMail = 136,

        /// <summary>
        /// 选择媒体键。
        /// </summary>
        SelectMedia = 137,

        /// <summary>
        /// 启动应用程序 1 键。
        /// </summary>
        LaunchApplication1 = 138,

        /// <summary>
        /// 启动应用程序 2 键。
        /// </summary>
        LaunchApplication2 = 139,

        /// <summary>
        /// OEM1 键。
        /// </summary>
        Oem1 = 140,

        /// <summary>
        /// OEM 分号键（别名）。
        /// </summary>
        OemSemicolon = 140,

        /// <summary>
        /// OEM 加号键。
        /// </summary>
        OemPlus = 141,

        /// <summary>
        /// OEM 逗号键。
        /// </summary>
        OemComma = 142,

        /// <summary>
        /// OEM 减号键。
        /// </summary>
        OemMinus = 143,

        /// <summary>
        /// OEM 句点键。
        /// </summary>
        OemPeriod = 144,

        /// <summary>
        /// OEM2 键。
        /// </summary>
        Oem2 = 145,

        /// <summary>
        /// OEM 问号键（别名）。
        /// </summary>
        OemQuestion = 145,

        /// <summary>
        /// OEM3 键。
        /// </summary>
        Oem3 = 146,

        /// <summary>
        /// OEM 波浪键（别名）。
        /// </summary>
        OemTilde = 146,

        /// <summary>
        /// 巴西键盘 AbntC1 键。
        /// </summary>
        AbntC1 = 147,

        /// <summary>
        /// 巴西键盘 AbntC2 键。
        /// </summary>
        AbntC2 = 148,

        /// <summary>
        /// OEM4 键。
        /// </summary>
        Oem4 = 149,

        /// <summary>
        /// OEM 左中括号键（别名）。
        /// </summary>
        OemOpenBrackets = 149,

        /// <summary>
        /// OEM5 键。
        /// </summary>
        Oem5 = 150,

        /// <summary>
        /// OEM 竖线键（别名）。
        /// </summary>
        OemPipe = 150,

        /// <summary>
        /// OEM6 键。
        /// </summary>
        Oem6 = 151,

        /// <summary>
        /// OEM 右中括号键（别名）。
        /// </summary>
        OemCloseBrackets = 151,

        /// <summary>
        /// OEM7 键。
        /// </summary>
        Oem7 = 152,

        /// <summary>
        /// OEM 引号键（别名）。
        /// </summary>
        OemQuotes = 152,

        /// <summary>
        /// OEM8 键。
        /// </summary>
        Oem8 = 153,

        /// <summary>
        /// OEM102 键。
        /// </summary>
        Oem102 = 154,

        /// <summary>
        /// OEM 反斜杠键（别名）。
        /// </summary>
        OemBackslash = 154,

        /// <summary>
        /// IME 处理键。
        /// </summary>
        ImeProcessed = 155,

        /// <summary>
        /// System 键。
        /// </summary>
        System = 156,

        /// <summary>
        /// 日文输入法字母键。
        /// </summary>
        DbeAlphanumeric = 157,

        /// <summary>
        /// OEM Attn 键（别名）。
        /// </summary>
        OemAttn = 157,

        /// <summary>
        /// 日文输入法片假名键。
        /// </summary>
        DbeKatakana = 158,

        /// <summary>
        /// OEM Finish 键（别名）。
        /// </summary>
        OemFinish = 158,

        /// <summary>
        /// 日文输入法平假名键。
        /// </summary>
        DbeHiragana = 159,

        /// <summary>
        /// OEM Copy 键（别名）。
        /// </summary>
        OemCopy = 159,

        /// <summary>
        /// 日文输入法 SBCS 字符键。
        /// </summary>
        DbeSbcsChar = 160,

        /// <summary>
        /// OEM Auto 键（别名）。
        /// </summary>
        OemAuto = 160,

        /// <summary>
        /// 日文输入法 DBCS 字符键。
        /// </summary>
        DbeDbcsChar = 161,

        /// <summary>
        /// OEM Enlw 键（别名）。
        /// </summary>
        OemEnlw = 161,

        /// <summary>
        /// 日文输入法罗马字键。
        /// </summary>
        DbeRoman = 162,

        /// <summary>
        /// OEM BackTab 键（别名）。
        /// </summary>
        OemBackTab = 162,

        /// <summary>
        /// Attn 键。
        /// </summary>
        Attn = 163,

        /// <summary>
        /// 日文输入法非罗马字键（别名）。
        /// </summary>
        DbeNoRoman = 163,

        /// <summary>
        /// CrSel 键。
        /// </summary>
        CrSel = 164,

        /// <summary>
        /// 日文输入法单词注册模式键（别名）。
        /// </summary>
        DbeEnterWordRegisterMode = 164,

        /// <summary>
        /// 日文输入法配置模式键（别名）。
        /// </summary>
        DbeEnterImeConfigureMode = 165,

        /// <summary>
        /// ExSel 键。
        /// </summary>
        ExSel = 165,

        /// <summary>
        /// 日文输入法清空字符串键（别名）。
        /// </summary>
        DbeFlushString = 166,

        /// <summary>
        /// EraseEof 键（别名）。
        /// </summary>
        EraseEof = 166,

        /// <summary>
        /// 日文输入法代码输入键。
        /// </summary>
        DbeCodeInput = 167,

        /// <summary>
        /// Play 键（别名）。
        /// </summary>
        Play = 167,

        /// <summary>
        /// 日文输入法非代码输入键。
        /// </summary>
        DbeNoCodeInput = 168,

        /// <summary>
        /// Zoom 键（别名）。
        /// </summary>
        Zoom = 168,

        /// <summary>
        /// 日文输入法确定字符串键。
        /// </summary>
        DbeDetermineString = 169,

        /// <summary>
        /// NoName 键（别名）。
        /// </summary>
        NoName = 169,

        /// <summary>
        /// 日文输入法对话转换模式键。
        /// </summary>
        DbeEnterDialogConversionMode = 170,

        /// <summary>
        /// Pa1 键（别名）。
        /// </summary>
        Pa1 = 170,

        /// <summary>
        /// OEM Clear 键。
        /// </summary>
        OemClear = 171,

        /// <summary>
        /// DeadCharProcessed 键。
        /// </summary>
        DeadCharProcessed = 172,

        /// <summary>
        /// 顶部数字键 0（主键盘区，非小键盘）。
        /// </summary>
        Num0,

        /// <summary>
        /// 顶部数字键 1（主键盘区，非小键盘）。
        /// </summary>
        Num1,

        /// <summary>
        /// 顶部数字键 2（主键盘区，非小键盘）。
        /// </summary>
        Num2,

        /// <summary>
        /// 顶部数字键 3（主键盘区，非小键盘）。
        /// </summary>
        Num3,

        /// <summary>
        /// 顶部数字键 4（主键盘区，非小键盘）。
        /// </summary>
        Num4,

        /// <summary>
        /// 顶部数字键 5（主键盘区，非小键盘）。
        /// </summary>
        Num5,

        /// <summary>
        /// 顶部数字键 6（主键盘区，非小键盘）。
        /// </summary>
        Num6,

        /// <summary>
        /// 顶部数字键 7（主键盘区，非小键盘）。
        /// </summary>
        Num7,

        /// <summary>
        /// 顶部数字键 8（主键盘区，非小键盘）。
        /// </summary>
        Num8,

        /// <summary>
        /// 顶部数字键 9（主键盘区，非小键盘）。
        /// </summary>
        Num9,
    }
}