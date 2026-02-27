using PopupBlocker.Utility.Commons;
using PopupBlocker.Utility.Interfaces;
using PopupBlocker.Utility.Windows;

namespace PopupBlocker.Core.Services
{
    public class PopupBlockService(LoggerService loggerService, RuleConfigService ruleConfigService) : ETWThreadMonitor, IService
    {
        #region IService
        public bool Switch
        {
            get => Status != Status.Init && Status != Status.Stop;
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }
        #endregion

        #region 服务启动与停止逻辑
        protected override void OnStart()
        {
            base.OnStart();
            _logger.Info("拦截器已启动");
        }

        protected override void OnStop()
        {
            base.OnStop();
            _logger.Info("拦截器已停止");
        }
        #endregion

        #region 监控逻辑
        [ThreadStatic]
        private static Models.BlockRules? _rules;
        private readonly object _monitorLock = new();
        private readonly LoggerService _logger = loggerService;
        private readonly RuleConfigService _config = ruleConfigService;

        protected override void OnThreadCreated(Microsoft.Diagnostics.Tracing.Parsers.Kernel.ThreadTraceData data)
        {
            var rules = _config.FindRules(data.ProcessName);
            if (rules is not null)
                Task.Run(() => CheckAndBlockWindows(rules));
        }

        private void CheckAndBlockWindows(Models.BlockRules rules)
        {
            _rules = rules;
            lock (_monitorLock)
            {
                WinAPI.EnumWindows(EnumWindowCallback, IntPtr.Zero);
            }
        }

        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private bool EnumWindowCallback(UIntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (hWnd == UIntPtr.Zero || !WinAPI.IsWindowVisible(hWnd))
                    return true;

                var processName = WindowInfo.GetWindowThreadProcessName(hWnd);

                if (processName != _rules!.ProcessName)
                    return true;

                var className = WindowInfo.GetWindowClass(hWnd);
                var windowTitle = WindowInfo.GetWindowTitle(hWnd);

                _logger.Debug($"检查窗口：{processName} - {className} - {windowTitle}");
                var rule = RuleConfigService.FindRule(_rules, className, windowTitle);
                if (rule is null)
                    return true;
                _logger.Info($"拦截窗口：{processName} - {className} - {windowTitle}");

                CloseWindowSafely(hWnd);
                _config.AddRuleCount(rule);
            }
            catch (Exception ex)
            {
                _logger.Warning($"检查窗口时出错：{ex.Message}");
            }
            return true;
        }

        private void CloseWindowSafely(UIntPtr hWnd)
        {
            try
            {
                // 首先尝试优雅关闭
                WinAPI.PostMessage(hWnd, WinAPI.WM_CLOSE, UIntPtr.Zero, IntPtr.Zero);

                Thread.Sleep(50);

                // 如果窗口仍然存在，强制销毁
                if (WinAPI.IsWindowVisible(hWnd))
                    WinAPI.DestroyWindow(hWnd);
            }
            catch (Exception ex)
            {
                _logger.Debug($"关闭窗口时出错：{ex.Message}");
                // 最后手段：隐藏窗口
                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
        }
        #endregion
    }
}
