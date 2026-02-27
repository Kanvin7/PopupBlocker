using PopupBlocker.Core;
using PopupBlocker.Utility.Commons;
using System.Text.Json.Serialization;

namespace PopupBlocker.ViewModels
{
    public class SettingViewModel : ViewModelServiceBase, Core.Services.IService
    {
        /// <summary>
        /// 是否启用日志记录
        /// </summary>
        [JsonPropertyName("isEnableLog")]
        public bool IsEnableLog
        {
            get => LoggerService.Switch;
            set
            {
                LoggerService.Switch = value;
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否启用拦截器
        /// </summary>
        [JsonPropertyName("isEnableBlock")]
        public bool IsEnableBlock
        {
            get => PopupBlockService.Switch;
            set
            {
                try
                {
                    PopupBlockService.Switch = value;
                }
                catch (UnauthorizedAccessException)
                {
                    LoggerService.Error("拦截服务启动失败，请以管理员身份运行程序！");
                }
                NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// 是否开机自启
        /// </summary>
        [JsonPropertyName("isAutoRun")]
        public bool IsAutoRun
        {
            get => AutoRunService.Switch;
            set
            {
                try
                {
                    AutoRunService.Switch = value;
                }
                catch (UnauthorizedAccessException)
                {
                    LoggerService.Error("设置开机自启失败，请以管理员身份运行程序！");
                }
                NotifyPropertyChanged();
            }
        }

        #region 加载和保存设置
        private bool _isLoaded;
        public void ConfirmLoaded() => _isLoaded = true;
        public static SettingViewModel LoadSetting()
        {
            SettingViewModel setting;
            try
            {
                setting = FileOperation.ReadJsonFromFile<SettingViewModel>(AppPath.SettingFilePath);
            }
            catch
            {
                setting = new SettingViewModel();
            }
            setting.ConfirmLoaded();
            return setting;
        }

        public void SaveSetting() => FileOperation.WriteJsonToFile(AppPath.SettingFilePath, this);
        #endregion

        #region IService
        [JsonIgnore]
        public bool Switch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        protected override void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (_isLoaded)
                SaveSetting();
            base.NotifyPropertyChanged(propertyName);
        }
    }
}
