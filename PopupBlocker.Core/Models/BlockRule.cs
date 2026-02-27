using System.Text.Json.Serialization;

namespace PopupBlocker.Core.Models
{
    public sealed class InterceptorRule : IPopupInfo
    {
        #region 属性
        /// <summary>
        /// 规则类型是否为窗口类名
        /// </summary>
        [JsonPropertyName("isWindowClass")]
        public bool IsWindowClass { get; set; }
        /// <summary>
        /// 规则类型是否为窗口标题
        /// </summary>
        [JsonIgnore]
        public bool IsWindowTitle => !IsWindowClass;
        /// <summary>
        /// 窗口类名
        /// </summary>
        [JsonPropertyName("windowClass")]
        public string WindowClass { get; init; }
        /// <summary>
        /// 窗口标题
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; init; }
        #endregion

        #region 构造函数
        [JsonConstructor]
        public InterceptorRule(string windowClass, bool isWindowClass, string windowTitle, bool isActive = true)
        {
            IsWindowClass = isWindowClass;
            WindowClass = windowClass;
            WindowTitle = windowTitle;
            IsActive = isActive;
        }
        public InterceptorRule(string windowClass, bool isActive = true) : this(windowClass, true, string.Empty, isActive) { }
        public InterceptorRule(string windowClass, string windowTitle, bool isWindowClass = true, bool isActive = true) : this(windowClass, isWindowClass, windowTitle, isActive) { }
        #endregion

        #region 方法
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{(IsActive ? "✓" : "✗")}] {(IsWindowClass ? "WindowClass" : "WindowTitle")}: {Pattern}";
        #endregion

        #region IPopupInfo
        private long _blockedCount = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        /// <summary>
        /// 匹配字符串
        /// </summary>
        [JsonIgnore]
        public string Pattern { get => IsWindowClass ? WindowClass : WindowTitle; }
        /// <summary>
        /// 拦截次数
        /// </summary>
        [JsonPropertyName("blockedCount")]
        public long BlockedCount
        {
            get => _blockedCount;
            init => _blockedCount = value;
        }

        /// <summary>
        /// 重置拦截次数
        /// </summary>
        public void ResetBlockCount() => _blockedCount = 0;
        /// <summary>
        /// 增加拦截次数
        /// </summary>
        public void AddBlockCount() => ++_blockedCount;
        #endregion
    }
}
