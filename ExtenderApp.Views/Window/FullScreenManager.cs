using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 管理窗口全屏（移除所有边框并确保不出现边缘“透出背景”现象）。 进入全屏时会：
    /// - 保存并恢复 WindowStyle / ResizeMode / Topmost / WindowState / 位置与大小
    /// - 将 WindowStyle 设置为 None、ResizeMode 设置为 NoResize（去除边框与标题栏）
    /// - 根据 coverTaskbar 参数选择使用屏幕工作区或完整像素边界（覆盖任务栏）
    /// - 处理 DPI 换算并对结果进行向外扩展（避免像素四舍五入导致的 1px 缝隙）
    /// </summary>
    public sealed class FullScreenManager
    {
        private readonly Window _window;
        private bool _isFullScreen;

        // 保存的状态
        private WindowStyle _previousWindowStyle;

        private ResizeMode _previousResizeMode;
        private bool _previousTopmost;
        private WindowState _previousWindowState;
        private Rect _previousBounds;
        private bool _topmostChanged;

        public FullScreenManager(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        public bool IsFullScreen => _isFullScreen;

        /// <summary>
        /// 进入全屏（移除边框并覆盖屏幕）。 coverTaskbar = true: 使用监视器像素边界，包含任务栏（强制覆盖）。 coverTaskbar = false: 使用监视器工作区（不覆盖任务栏）。
        /// </summary>
        public void Enter(bool coverTaskbar = false)
        {
            if (_isFullScreen)
                return;

            // 保存旧状态
            _previousWindowStyle = _window.WindowStyle;
            _previousResizeMode = _window.ResizeMode;
            _previousTopmost = _window.Topmost;
            _previousWindowState = _window.WindowState;
            _previousBounds = new Rect(_window.Left, _window.Top, _window.Width, _window.Height);

            // 为精确设置位置/大小，先切到 Normal
            _window.WindowState = WindowState.Normal;

            // 获取监视器像素边界
            var hwnd = new WindowInteropHelper(_window).Handle;
            IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            var mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(mi);
            RECT boundsPx;
            if (!GetMonitorInfo(hMonitor, ref mi))
            {
                // 失败则退回到系统主屏幕
                boundsPx = coverTaskbar
                    ? new RECT { left = 0, top = 0, right = (int)SystemParameters.PrimaryScreenWidth, bottom = (int)SystemParameters.PrimaryScreenHeight }
                    : new RECT { left = (int)SystemParameters.WorkArea.Left, top = (int)SystemParameters.WorkArea.Top, right = (int)(SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width), bottom = (int)(SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height) };
            }
            else
            {
                boundsPx = coverTaskbar ? mi.rcMonitor : mi.rcWork;
            }

            // DPI 转换：设备像素 -> WPF 设备无关像素
            var source = PresentationSource.FromVisual(_window);
            Rect targetRect;
            if (source?.CompositionTarget != null)
            {
                var matrix = source.CompositionTarget.TransformFromDevice;
                var dpiRect = TransformRectByMatrix(matrix, new Rect(boundsPx.left, boundsPx.top, boundsPx.right - boundsPx.left, boundsPx.bottom - boundsPx.top));

                // 向外扩展（避免四舍五入/像素化留缝）
                double left = Math.Floor(dpiRect.Left) - 1;
                double top = Math.Floor(dpiRect.Top) - 1;
                double width = Math.Ceiling(dpiRect.Width) + 2;
                double height = Math.Ceiling(dpiRect.Height) + 2;
                targetRect = new Rect(left, top, Math.Max(0, width), Math.Max(0, height));
            }
            else
            {
                double left = Math.Floor((double)boundsPx.left) - 1;
                double top = Math.Floor((double)boundsPx.top) - 1;
                double width = Math.Ceiling((double)(boundsPx.right - boundsPx.left)) + 2;
                double height = Math.Ceiling((double)(boundsPx.bottom - boundsPx.top)) + 2;
                targetRect = new Rect(left, top, Math.Max(0, width), Math.Max(0, height));
            }

            // 去除边框与标题栏，禁止调整大小，并确保在最顶部以覆盖任务栏
            _window.WindowStyle = WindowStyle.None;
            _window.ResizeMode = ResizeMode.NoResize;

            // 记录并设置 Topmost
            _previousTopmost = _window.Topmost;
            _window.Topmost = true;
            _topmostChanged = true;

            // 设置位置和尺寸（保持 Normal 状态）
            _window.Left = targetRect.Left;
            _window.Top = targetRect.Top;
            _window.Width = targetRect.Width;
            _window.Height = targetRect.Height;

            // 确保已应用尺寸（有时需要再次刷新状态）
            _window.UpdateLayout();

            _isFullScreen = true;
        }

        /// <summary>
        /// 退出全屏并恢复之前保存的窗口状态。
        /// </summary>
        public void Exit()
        {
            if (!_isFullScreen)
                return;

            try
            {
                // 恢复位置与大小
                _window.Left = _previousBounds.Left;
                _window.Top = _previousBounds.Top;
                _window.Width = _previousBounds.Width;
                _window.Height = _previousBounds.Height;
            }
            catch
            {
                // 忽略恢复失败
            }

            // 恢复 WindowState
            _window.WindowState = _previousWindowState;

            // 恢复 WindowStyle / ResizeMode
            _window.WindowStyle = _previousWindowStyle;
            _window.ResizeMode = _previousResizeMode;

            // 恢复 Topmost（如果我们修改过）
            if (_topmostChanged)
            {
                _window.Topmost = _previousTopmost;
                _topmostChanged = false;
            }

            _isFullScreen = false;
        }

        /// <summary>
        /// 切换全屏状态。
        /// </summary>
        public void Toggle(bool coverTaskbar = false)
        {
            if (_isFullScreen) Exit(); else Enter(coverTaskbar);
        }

        #region Win32 interop (monitor)

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        #endregion Win32 interop (monitor)

        #region Helpers

        private static Rect TransformRectByMatrix(Matrix matrix, Rect rect)
        {
            // 把矩形四个角逐点转换，再取包围矩形
            var p1 = matrix.Transform(new Point(rect.Left, rect.Top));
            var p2 = matrix.Transform(new Point(rect.Right, rect.Top));
            var p3 = matrix.Transform(new Point(rect.Right, rect.Bottom));
            var p4 = matrix.Transform(new Point(rect.Left, rect.Bottom));

            double left = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
            double top = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
            double right = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
            double bottom = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

            return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }

        #endregion Helpers
    }
}