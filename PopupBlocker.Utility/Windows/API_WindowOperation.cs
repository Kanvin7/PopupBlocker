using System.Runtime.InteropServices;

namespace PopupBlocker.Utility.Windows
{
    public static partial class WinAPI
    {
        #region 常量
        public const int SW_HIDE = 0;
        public const uint WM_CLOSE = 0x0010;
        #endregion

        #region 结构
        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumWindowsProc(UIntPtr hWnd, IntPtr lParam);
        #endregion

        #region 方法
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern UIntPtr FindWindow(string? className, string? windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern UIntPtr FindWindowEx(UIntPtr parentHandle, UIntPtr childAfter, string? className, string? windowTitle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(UIntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(UIntPtr hWnd, System.Text.StringBuilder windowTitle, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(UIntPtr hWnd, System.Text.StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(UIntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(UIntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(UIntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(UIntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(UIntPtr hWnd);
        #endregion
    }
}
