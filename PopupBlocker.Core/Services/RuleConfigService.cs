using PopupBlocker.Core.Models;
using PopupBlocker.Utility.Commons;

namespace PopupBlocker.Core.Services
{
    public class RuleConfigService : IService
    {
        #region IService
        public bool Switch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region 私有字段
        // 存储拦截规则的列表
        private readonly BlockRuleList _ruleList = [];
        // 日志服务，用于记录日志信息
        private readonly LoggerService _logger;
        #endregion

        public RuleConfigService(LoggerService loggerService)
        {
            _logger = loggerService;
            LoadRuleList();
        }

        /// <summary>
        /// 规则变化通知事件，当规则发生变化时触发此事件。
        /// </summary>
        public event Action<IEnumerable<BlockRules>>? RulesChanged;
        public long BlockedCount => _ruleList.Sum(r => r.BlockedCount);

        #region 规则列表的公共方法
        public void LoadRuleList(string? filePath = null)
        {
            var fp = filePath ?? AppPath.RuleConfigFilePath;
            _ruleList.Clear();
            try
            {
                var rules = FileOperation.ReadJsonFromFile<BlockRuleList>(fp);
                _ruleList.AddRange(rules);

                if (filePath is not null)
                    SaveRuleList();
            }
            catch (Exception ex)
            {
                _logger.Warning($"加载拦截规则失败：{ex.Message}");
                return;
            }
            _logger.Info($"拦截规则已成功加载：{fp}");
        }

        public void SaveRuleList(string? filePath = null, bool isNotifyUI = true)
        {
            var fp = filePath ?? AppPath.RuleConfigFilePath;
            try
            {
                FileOperation.WriteJsonToFile(fp, _ruleList);
            }
            catch (Exception ex)
            {
                _logger.Warning($"保存拦截规则失败：{ex.Message}");
                return;
            }
            var info = $"拦截规则已成功保存：{fp}";
            if (filePath is null)
                _logger.Debug(info);
            else
                _logger.Info(info);
            if (isNotifyUI)
                RulesChanged?.Invoke(GetAllRules());
        }

        public BlockRules? FindRules(string processName) => _ruleList.Find(r => r.ProcessName == processName);

        public void ResetAllCounts()
        {
            _ruleList.ForEach(r => r.ResetBlockCount());
            SaveRuleList();
            _logger.Info("所有拦截规则的计数已重置");
        }

        public void ResetRulesCount(BlockRules rules)
        {
            // 能重置就是存在规则，存在就可以直接指定规则，无需搜索
            rules.ResetBlockCount();
            SaveRuleList();
            _logger.Info($"进程的拦截规则计数已重置：{rules.ProcessName}");
        }

        /* 这是浅拷贝，所以你对新列表的修改也会影响原始列表
         * 这是你能通过数据绑定直接影响原始列表的原因
         * 也是此处对于规则方法能如此轻松实现的原因
         * 如果使用深拷贝，你必须给每个规则方法加上搜索逻辑，以找到并更新原始列表中的规则
         */
        public BlockRuleList GetAllRules() => [.. _ruleList];
        #endregion

        #region 规则的公共方法
        public void AddRule(string processName, InterceptorRule? rule = null)
        {
            // 添加无法确定规则是否存在，不确定就不能指定规则，需要搜索
            var rules = FindRules(processName);
            if (rule is null)
            {
                if (rules is null)
                {
                    var newRules = new BlockRules(processName);
                    _ruleList.Add(newRules);
                    SaveRuleList();
                    _logger.Info($"拦截规则已成功添加：{newRules}");
                }
                else
                    _logger.Warning($"拦截规则已存在：{rules}");
            }
            else
            {
                if (rules is null)
                {
                    var newRules = new BlockRules(processName, [rule]);
                    _ruleList.Add(newRules);
                    SaveRuleList();
                    _logger.Info($"拦截规则已成功添加：{newRules.ProcessName} - {rule}");
                }
                else if (rules.AddRule(rule))
                    _logger.Info($"拦截规则已成功添加：{rules.ProcessName} - {rule}");
                else
                    _logger.Warning($"拦截规则已存在：{rules.ProcessName} - {rule}");
            }
        }
        public void AddRule(string processName, string className, string windowTitle, bool isWindowClass = true) => AddRule(processName, new InterceptorRule(className, windowTitle, isWindowClass));

        public void RemoveRule(BlockRules rules, InterceptorRule? rule = null)
        {
            if (rule is null)
                _ruleList.Remove(rules);
            else
            {
                rules.RemoveRule(rule);
                if (rules.Rules!.Count == 0)
                    _ruleList.Remove(rules);
            }
            SaveRuleList();
            _logger.Info($"拦截规则已成功删除：{(rule is null ? rules : $"{rules.ProcessName} - {rule}")}");
        }

        public static IPopupCount? FindRule(BlockRules? rules, string className, string windowTitle)
        {
            if (rules is null || !rules.IsActive)
                return null;
            if (rules.IsProcessName)
                return rules;
            return rules.FindRule(className, windowTitle);
        }
        public IPopupCount? FindRule(string processName, string className, string windowTitle) => FindRule(FindRules(processName), className, windowTitle);

        public void ChangeRuleActivityStatus(IPopupInfo rule)
        {
            rule.ChangeActivityStatus();
            SaveRuleList();
        }

        [Obsolete("请使用ResetRulesCount()方法")]
        public void ResetRuleCount(IPopupCount rule)
        {
            // ??? 你没事吧？仔细用你的小脑瓜子想一下，这有用吗？
            rule.ResetBlockCount();
            SaveRuleList();
            throw new InvalidOperationException("哦？你是不是m？因为你漏了一个s，这个s正在用异常敲打你");
            // 你不会以为我写了就能用吧？（doge
        }

        public void AddRuleCount(IPopupCount rule)
        {
            rule.AddBlockCount();
            SaveRuleList();
        }
        #endregion
    }
}
