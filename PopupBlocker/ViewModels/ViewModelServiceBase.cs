using PopupBlocker.Core.Services;

namespace PopupBlocker.ViewModels
{
    /* 我知道你想问什么
     * 既然这里用静态，为什么还要ServiceManager？
     * 那是为了以后可能的扩展，Service才是变化的源头
     * 静态是为了减少内存占用，并让Service的变化直接体现在所有ViewModel上
     * 确保所有ViewModel不会因为Service的变更而混乱不堪
     */
    public class ViewModelServiceBase : ViewModelBase
    {
        protected static LoggerService LoggerService { get; set; }
        protected static RuleConfigService RuleConfigService { get; set; }
        protected static PopupBlockService PopupBlockService { get; set; }
        protected static AutoRunService AutoRunService { get; set; }

        static ViewModelServiceBase()
        {
            var sm = Utility.Commons.Singleton<ServiceManager>.Instance;
            LoggerService = sm.GetService<LoggerService>(ServiceType.DefaultLoggerService)!;
            RuleConfigService = sm.GetService<RuleConfigService>(ServiceType.DefaultRuleConfigService)!;
            PopupBlockService = sm.GetService<PopupBlockService>(ServiceType.DefaultPopupBlockService)!;
            AutoRunService = sm.GetService<AutoRunService>(ServiceType.DefaultAutoRunService)!;
        }
    }
}
