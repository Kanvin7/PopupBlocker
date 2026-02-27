using PopupBlocker.Core.Services;

namespace PopupBlocker.Views.Pages
{
    /// <summary>
    /// LogPage.xaml 的交互逻辑
    /// </summary>
    public partial class LogPage : System.Windows.Controls.Page
    {
        private readonly ServiceManager _serviceManager = Utility.Commons.Singleton<ServiceManager>.Instance;

        public LogPage()
        {
            InitializeComponent();
            this.DataContext = _serviceManager.GetService<ViewModels.SettingViewModel>(ServiceType.DefaultSettingService);
            // 订阅事件，并确保UI线程安全
            _serviceManager
                .GetService<LoggerService>(ServiceType.DefaultLoggerService)!
                .LogWritingEvent += (text) => Dispatcher.Invoke(() => tbLog.AppendText(text));
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 取消订阅事件，防止内存泄露
            _serviceManager
                .GetService<LoggerService>(ServiceType.DefaultLoggerService)!
                .LogWritingEvent -= (text) => Dispatcher.Invoke(() => tbLog.AppendText(text));
        }
    }
}
