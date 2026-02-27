namespace PopupBlocker.Core.Models
{
    public interface IPopupInfo : IPopupCount
    {
        public bool IsActive { get; set; }
        public string Pattern { get; }

        /// <summary>
        /// 切换启用状态
        /// </summary>
        public void ChangeActivityStatus() => IsActive = !IsActive;
    }
}
