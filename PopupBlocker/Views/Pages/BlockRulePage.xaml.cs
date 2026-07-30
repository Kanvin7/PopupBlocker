using PopupBlocker.Core.Services;
using System.Windows;

namespace PopupBlocker.Views.Pages
{
    /// <summary>
    /// BlockRulePage.xaml 的交互逻辑
    /// </summary>
    public partial class BlockRulePage : System.Windows.Controls.Page
    {
        public BlockRulePage()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.PopupBlockViewModel();
            ctsHeader.DataContext = Utility.Commons.Singleton<ServiceManager>.Instance.GetService<ViewModels.SettingViewModel>(ServiceType.DefaultSettingService);
        }

        private void ResizeMiddleHeight(object? sender, SizeChangedEventArgs? e)
        {
            double height;
            if (sender is MainWindow window)
                height = window.Height - window.MainWindowTitleHeight - 5d;
            else
                height = ActualHeight;
            svMiddle.Height = height - ctsHeader.ActualHeight - gFooter.ActualHeight;
        }


        private Window? _mainWindow;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeMiddleHeight(null, null);
            _mainWindow = Window.GetWindow(this);
            _mainWindow.SizeChanged += ResizeMiddleHeight;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_mainWindow is not null)
                _mainWindow.SizeChanged -= ResizeMiddleHeight;
        }
    }
}
