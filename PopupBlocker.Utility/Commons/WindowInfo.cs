using PopupBlocker.Utility.Windows;
using System.Text;

namespace PopupBlocker.Utility.Commons
{
    public class WindowInfo
    {
        private const int _bufferSize = 256;
        private readonly StringBuilder _classBuilder = new(_bufferSize);
        private readonly StringBuilder _titleBuilder = new(_bufferSize);

        public UIntPtr Handle { get; set; }
        public string ProcessName => GetWindowThreadProcessName(Handle);
        public string WindowClass => GetWindowClass(Handle, _classBuilder);
        public string WindowTitle => GetWindowTitle(Handle, _titleBuilder);


        public static string GetWindowThreadProcessName(UIntPtr handle)
        {
            _ = WinAPI.GetWindowThreadProcessId(handle, out var processId);
            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }

        public static string GetWindowClass(UIntPtr handle, StringBuilder? buffer = null)
        {
            buffer ??= new StringBuilder(_bufferSize);
            _ = WinAPI.GetClassName(handle, buffer, buffer.Capacity);
            var className = buffer.ToString();
            if (className.StartsWith("HwndWrapper"))
                return className[12..className.IndexOf(';', 12)];
            else
                return className;
        }

        public static string GetWindowTitle(UIntPtr handle, StringBuilder? buffer = null)
        {
            buffer ??= new StringBuilder(_bufferSize);
            _ = WinAPI.GetWindowText(handle, buffer, buffer.Capacity);
            return buffer.ToString();
        }
    }
}
