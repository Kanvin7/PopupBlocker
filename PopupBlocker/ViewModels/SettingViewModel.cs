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
                catch (Exception ex)
                {
                    // 启动失败不能中断整个设置的加载，否则用户其余的设置会被一起丢掉
                    LoggerService.Error($"拦截服务启动失败：{ex.Message}");
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
                catch (Exception ex)
                {
                    LoggerService.Error($"设置开机自启失败：{ex.Message}");
                }
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 是否使用深色主题
        /// </summary>
        [JsonPropertyName("isDarkTheme")]
        public bool IsDarkTheme
        {
            get => Commons.ThemeSwitcher.IsDark;
            set
            {
                try
                {
                    Commons.ThemeSwitcher.Apply(value);
                }
                catch (Exception ex)
                {
                    LoggerService.Error($"切换主题失败：{ex.Message}");
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
            catch (Exception ex)
            {
                // 读不出来就退回默认值，但要把原因记下来，
                // 否则用户只会看到"设置莫名其妙全没了"
                LoggerService.Warning($"读取设置失败，将使用默认设置：{ex.Message}");
                setting = new SettingViewModel();
            }
            setting.ConfirmLoaded();
            return setting;
        }

        public void SaveSetting()
        {
            try
            {
                FileOperation.WriteJsonToFile(AppPath.SettingFilePath, this);
            }
            catch (Exception ex)
            {
                // 保存失败只记录，界面上已经生效的开关不受影响
                LoggerService.Error($"保存设置失败：{ex.Message}");
            }
        }
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
