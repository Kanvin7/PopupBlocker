using PopupBlocker.Utility.Interfaces;

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
        private readonly LoggerService _logger = loggerService;
        private readonly Dictionary<Models.BlockRules, WindowCloseService> _checkList = [];

        protected override void OnThreadCreated(Microsoft.Diagnostics.Tracing.Parsers.Kernel.ThreadTraceData data)
        {
            var rules = ruleConfigService.FindRules(data.ProcessName);
            if (rules is not null)
            {
                _checkList.TryAdd(rules, new WindowCloseService(rules, _logger, ruleConfigService));
                _checkList[rules].CheckAndBlockWindows();
            }
        }
        #endregion
    }
}
