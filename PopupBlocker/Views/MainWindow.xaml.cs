namespace PopupBlocker.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(string startPageIdOrTargetTag = "Home")
        {
            _startPageIdOrTargetTag = startPageIdOrTargetTag;
            InitializeComponent();
        }

        public bool IsClosed { get; private set; }
        public double MainWindowTitleHeight => tbMain.ActualHeight;
        public void ChangePage(string pageIdOrTargetTag) => nvMain.Navigate(pageIdOrTargetTag);

        #region 窗口事件
        private readonly string _startPageIdOrTargetTag;

        private void FluentWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) => nvMain.Navigate(_startPageIdOrTargetTag);
        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => IsClosed = true;
        #endregion
    }
}
