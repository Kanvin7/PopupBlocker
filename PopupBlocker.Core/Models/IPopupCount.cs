namespace PopupBlocker.Core.Models
{
    public interface IPopupCount
    {
        public long BlockedCount { get; }

        public void ResetBlockCount();
        public void AddBlockCount();
    }
}
