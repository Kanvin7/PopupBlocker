using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace PopupBlocker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            /* 并不是说Debug模式不需要管理员权限
             * 只是提供一个快速定位bug在不在核心的方法
             * 没有管理员权限的程序，核心一定不会工作
             */
#if !DEBUG
            // 检查是否以管理员身份运行
            if (!IsRunningAsAdministrator())
            {
                // 直接通过UAC请求管理员权限
                var processInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath,
                    UseShellExecute = true,
                    Verb = "runas" // 这会触发Windows UAC提示框
                };

                try
                {
                    Process.Start(processInfo)?.Dispose();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    /* 依伊的低语
                     * ai 还是太温柔了，这种小逝干嘛要叨扰用户（doge
                     */
                    // 用户拒绝UAC请求或启动失败，直接退出
                    //MessageBox.Show("程序需要管理员权限才能正常运行。", "需要管理员权限", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // 关闭当前实例
                Current.Shutdown();
                return;
            }
#endif
            // 正常启动逻辑
            base.OnStartup(e);

            /* 同一时间只保留一个实例。
             * 关闭窗口并不会结束程序（它会留在托盘里继续拦截），
             * 如果此时从快捷方式再次启动，就会变成两个实例各写一份设置，
             * 互相覆盖，表现为"设置被改回去了"。 */
            _singleInstanceMutex = new Mutex(true, Core.AppPath.SingleInstanceMutexName, out var isFirstInstance);
            if (!isFirstInstance)
            {
                // 自动启动时安静退出即可；手动启动则把已有窗口叫到前台
                if (!IsAutoRunLaunch(e))
                    ActivateExistingInstance();

                Current.Shutdown();
                return;
            }

#if true    // 上面的空间用于临时测试，有需要记得改为false
            new Views.Tray(e.Args.Length == 0 || e.Args[0] != Core.AppPath.AutoRunSwitchProperty).Show();
#endif
        }

        private Mutex? _singleInstanceMutex;

        private static bool IsAutoRunLaunch(StartupEventArgs e) =>
            e.Args.Length > 0 && e.Args[0] == Core.AppPath.AutoRunSwitchProperty;

        /// <summary>
        /// 通知已经在运行的那个实例把主窗口显示出来。
        /// </summary>
        private static void ActivateExistingInstance()
        {
            try
            {
                using var signal = EventWaitHandle.OpenExisting(Core.AppPath.ActivateSignalName);
                signal.Set();
            }
            catch
            {
                // 对方可能刚好在退出，这时给用户一个交代，避免看起来"点了没反应"
                MessageBox.Show(
                    "程序已经在运行了，可以在系统托盘里找到它。",
                    "轻量级弹窗拦截器",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
