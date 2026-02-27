using PopupBlocker.Core.Services;

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
            cts_Header.DataContext = Utility.Commons.Singleton<ServiceManager>.Instance.GetService<ViewModels.SettingViewModel>(ServiceType.DefaultSettingService);
        }
    }
}
