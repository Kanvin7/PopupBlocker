using Microsoft.Win32;
using PopupBlocker.Core.Models;
using PopupBlocker.Utility.Commons;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PopupBlocker.ViewModels
{
    public class PopupBlockViewModel : ViewModelServiceBase
    {
        public PopupBlockViewModel()
        {
            RuleConfigService.RulesChanged += RuleChangedEvent;
            RuleChangedEvent(RuleConfigService.GetAllRules());
        }

        #region 属性
        public long BlockedCount => RuleConfigService.BlockedCount;

        /// <summary>
        /// 界面绑定的规则列表。
        /// 这里用可观察集合，而不是每次整份替换，
        /// 否则规则一变所有卡片都会被重建：入场动画重播一遍，
        /// 已展开的进程分组也会被收拢，重排动画更无从谈起。
        /// </summary>
        public ObservableCollection<BlockRules> RuleList { get; } = [];

        public ICommand ResetAllCountCommand => new RelayCommand(obj =>
        {
            RuleConfigService.ResetAllCounts();
        });

        public ICommand ImportRulesCommand => new RelayCommand(obj =>
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "导入配置"
            };
            if (openFileDialog.ShowDialog() == true)
                RuleConfigService.LoadRuleList(openFileDialog.FileName);
        });

        public ICommand ExportRulesCommand => new RelayCommand(obj =>
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                FileName = Core.AppPath.RuleConfigFileName,
                Title = "导出配置"
            };
            if (saveFileDialog.ShowDialog() == true)
                RuleConfigService.SaveRuleList(saveFileDialog.FileName, false);
        });

        public ICommand WindowSelectCommand => new RelayCommand(obj =>
        {
            new Views.Selector().Show();
        });

        public ICommand SaveRuleCommand => new RelayCommand(obj =>
        {
            RuleConfigService.SaveRuleList(isNotifyUI: false);
        });

        public ICommand ResetCountCommand => new RelayCommand(obj =>
        {
            if (obj is BlockRules rules)
            {
                RuleConfigService.ResetRulesCount(rules);
            }
        });

        public ICommand RemoveRuleCommand => new RelayCommand(obj =>
        {
            if (obj is RemoveRuleParameters parameters)
            {
                RuleConfigService.RemoveRule((BlockRules)parameters.Item1, (InterceptorRule?)parameters.Item2);
            }
        });
        #endregion

        #region 方法
        private void RuleChangedEvent(IEnumerable<BlockRules> ruleList)
        {
            RuleList.Clear();
            foreach (var rules in ruleList)
                RuleList.Add(rules);
            NotifyPropertyChanged(nameof(BlockedCount));
        }
        #endregion
    }
}
