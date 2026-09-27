using PopupBlocker.Core.Services;
using PopupBlocker.Utility.Commons;
using System.Windows;

namespace PopupBlocker.Views
{
    /// <summary>
    /// Tray.xaml 的交互逻辑
    /// </summary>
    public partial class Tray : Window
    {
        public Tray(bool isShowMainWindow = true)
        {
            // 加载服务，确保核心能正常运行
            // 其创建了一些IDisposable对象，记得在OnClosed中释放资源
            Singleton<ServiceManager>.Instance.RegisterService(ServiceType.DefaultSettingService, ViewModels.SettingViewModel.LoadSetting());
            StartActivateListener();
            // 创建主窗口，按需显示
            if (isShowMainWindow)
            {
                _mainWindow = new();
                _mainWindow.Show();
            }
            // 如果是图标异常，请务必按照项目文件(PopupBlocker.csproj)中的注释进行操作
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_mainWindow?.IsClosed ?? false)
                _mainWindow!.Close();
            _activateSignal?.Dispose();
            _activateSignal = null;
            _trayMessageSource?.Dispose();
            _trayMessageSource = null;
            // 谁创建谁释放！
            Singleton<ServiceManager>.Instance.Dispose();
            base.OnClosed(e);
        }

        #region 资源管理器重启后重新注册托盘图标
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 窗口句柄这时才存在，挂钩要放在这里做
            HookTaskbarCreated();
        }

        /* 资源管理器一旦重启（系统更新、崩溃恢复都会遇到），
         * 系统会广播 TaskbarCreated 要求各程序重新把图标交上去。
         * 界面库不处理这件事，不补这一步，图标就会永久消失。 */
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern uint RegisterWindowMessage(string message);

        private System.Windows.Interop.HwndSource? _trayMessageSource;
        private uint _taskbarCreatedMessage;

        private void HookTaskbarCreated()
        {
            _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
            if (_taskbarCreatedMessage == 0)
                return;

            _trayMessageSource = PresentationSource.FromVisual(this)
                as System.Windows.Interop.HwndSource;
            _trayMessageSource?.AddHook(TrayMessageHook);
        }

        private IntPtr TrayMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if ((uint)msg == _taskbarCreatedMessage)
                niTray.Register();

            return IntPtr.Zero;
        }
        #endregion

        #region 单实例：重复启动时把已有窗口叫到前台
        private EventWaitHandle? _activateSignal;
        private Thread? _activateListener;

        private void StartActivateListener()
        {
            try
            {
                _activateSignal = new EventWaitHandle(false, EventResetMode.AutoReset, Core.AppPath.ActivateSignalName);
            }
            catch
            {
                return;
            }

            _activateListener = new Thread(() =>
            {
                while (_activateSignal!.WaitOne())
                    Dispatcher.Invoke(() => ShowMainWindow("Home"));
            })
            {
                IsBackground = true,
                Name = "SingleInstanceListener",
            };
            _activateListener.Start();
        }
        #endregion

        #region 系统托盘菜单事件
        private MainWindow? _mainWindow;

        private void ShowMainWindow(string pageIdOrTargetTag)
        {
            if (_mainWindow?.IsClosed ?? true)
                _mainWindow = new MainWindow(pageIdOrTargetTag);
            else
            {
                _mainWindow.ChangePage(pageIdOrTargetTag);
                if (_mainWindow.WindowState == WindowState.Minimized)
                    _mainWindow.WindowState = WindowState.Normal;
            }
            _mainWindow.Show();
            _mainWindow.Activate();
        }

        private void niTray_LeftClick(Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e) => ShowMainWindow("Home");
        private void niMenu_Home_Click(object sender, RoutedEventArgs e) => ShowMainWindow("Home");
        private void niMenu_Setting_Click(object sender, RoutedEventArgs e) => ShowMainWindow("Setting");
        private void niMenu_Close_Click(object sender, RoutedEventArgs e) => this.Close();
        #endregion
    }
}
