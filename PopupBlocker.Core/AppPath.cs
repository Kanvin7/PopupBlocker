using System.IO;

namespace PopupBlocker.Core
{
    public static class AppPath
    {
        private static readonly string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string _appFolder = Path.Combine(_appDataPath, AppName);
        private static readonly string _ruleConfigFilePath = Path.Combine(_appFolder, RuleConfigFileName);
        private static readonly string _settingFilePath = Path.Combine(_appFolder, SettingFileName);

        static AppPath()
        {
            Directory.CreateDirectory(_appFolder);
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            ExecutingPath = process.MainModule!.FileName;
        }

        public const string RuleConfigFileName = "PopupBlocker_RuleConfig.json";
        public static string RuleConfigFilePath => _ruleConfigFilePath;
        public const string SettingFileName = "PopupBlocker_Setting.json";
        public static string SettingFilePath => _settingFilePath;

        public const string AppName = "PopupBlocker";
        public const string AutoRunSwitchProperty = "/autorun";

        /// <summary>
        /// 单实例互斥体名称，保证同一时间只有一个程序在运行。
        /// </summary>
        public const string SingleInstanceMutexName = "PopupBlocker.SingleInstance";

        /// <summary>
        /// 唤醒已有实例的信号名称。
        /// </summary>
        public const string ActivateSignalName = "PopupBlocker.Activate";

        public static string ExecutingPath { get; }
    }
}
