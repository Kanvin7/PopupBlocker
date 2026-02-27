namespace PopupBlocker.Core.Services
{
    /// <summary>
    /// 服务类型枚举
    /// </summary>
    /// <remarks>
    /// 自定义服务请从0x100开始，0xhh为软件保留
    /// </remarks>
    public static partial class ServiceType
    {
        public const int DefaultLoggerService = 0x00;
        public const int DefaultRuleConfigService = 0x01;
        public const int DefaultPopupBlockService = 0x02;
        public const int DefaultAutoRunService = 0x03;
        public const int DefaultSettingService = 0x04;
    }

    public interface IService
    {
        /// <summary>
        /// 服务开关
        /// </summary>
        public bool Switch { get; set; }
    }
}
