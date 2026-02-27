using System.Text.Json.Serialization;

namespace PopupBlocker.Core.Models
{
    public sealed class BlockRules : IPopupInfo
    {
        #region 属性
        /// <summary>
        /// 规则类型是否为进程名称
        /// </summary>
        [JsonIgnore]
        public bool IsProcessName => Rules is null;
        /// <summary>
        /// 进程名称
        /// </summary>
        [JsonPropertyName("processName")]
        public string ProcessName { get; init; }
        /// <summary>
        /// 拦截规则
        /// </summary>
        [JsonPropertyName("rules")]
        public List<InterceptorRule>? Rules { get; init; }
        #endregion

        #region 构造函数
        [JsonConstructor]
        public BlockRules(string processName, List<InterceptorRule>? rules, bool isActive = true)
        {
            ProcessName = processName;
            Rules = rules;
            IsActive = isActive;
        }
        public BlockRules(string processName, bool isActive = true) : this(processName, null, isActive) { }
        #endregion

        #region 方法
        /// <summary>
        /// 添加规则
        /// </summary>
        /// <param name="rule"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public bool AddRule(InterceptorRule rule)
        {
            if (IsProcessName)
                throw new InvalidOperationException("按进程名称拦截时，不能添加规则");
            if (Rules!.Exists(r => r.WindowClass == rule.WindowClass && r.WindowTitle == rule.WindowTitle))
                return false;
            Rules.Add(rule);
            return true;
        }
        /// <summary>
        /// 删除规则
        /// </summary>
        /// <param name="rule"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveRule(InterceptorRule rule)
        {
            if (IsProcessName)
                throw new InvalidOperationException("按进程名称拦截时，不能删除规则");
            Rules!.Remove(rule);
        }
        /// <summary>
        /// 匹配规则
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IPopupCount? FindRule(string className, string windowTitle)
        {
            if (IsProcessName)
                throw new InvalidOperationException("按进程名称拦截时，不能匹配规则");

            var rule = Rules!.Find(r => r.Pattern == (r.IsWindowClass ? className : windowTitle));
            if (rule is null || !rule.IsActive)
                return null;
            return rule;
        }
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{(IsActive ? "✓" : "✗")}] {ProcessName}: {(IsProcessName ? "<Null>" : "<List>")}";
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
        public string Pattern => IsProcessName ? ProcessName : throw new InvalidOperationException("不按进程名称拦截时，不能获取匹配字符串");
        /// <summary>
        /// 拦截次数
        /// </summary>
        [JsonPropertyName("blockedCount")]
        public long BlockedCount
        {
            get
            {
                if (IsProcessName)
                    return _blockedCount;
                return Rules!.Sum(r => r.BlockedCount);
            }
            init => _blockedCount = value;
        }

        /// <summary>
        /// 重置拦截次数
        /// </summary>
        public void ResetBlockCount()
        {
            if (IsProcessName)
                _blockedCount = 0;
            else
                Rules!.ForEach(r => r.ResetBlockCount());
        }
        /// <summary>
        /// 增加拦截次数
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddBlockCount()
        {
            if (!IsProcessName)
                throw new InvalidOperationException("不按进程名称拦截时，不能增加拦截次数");
            ++_blockedCount;
        }
        #endregion
    }
}
