using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PopupBlocker.Components
{
    /// <summary>
    /// 给删除命令包一层：先让目标卡片淡出，再真正执行删除。
    /// 配合 <see cref="FluidVirtualizingStackPanel"/>，就不会出现卡片凭空消失、其余卡片瞬移的生硬感。
    /// </summary>
    public class AnimatedRemoveCommand(
        ItemsControl owner,
        Func<object?, object?> itemSelector,
        Func<ICommand?> innerCommandAccessor) : ICommand
    {
        private const int FadeOutMilliseconds = 180;
        private bool _isRunning;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private FrameworkElement? FindContainer(object? item) =>
            item is null ? null : owner.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;

        #region ICommand
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => innerCommandAccessor()?.CanExecute(parameter) ?? false;

        public void Execute(object? parameter)
        {
            var innerCommand = innerCommandAccessor();
            if (innerCommand is null || _isRunning)
                return;

            var container = FindContainer(itemSelector(parameter));
            if (container is null)
            {
                innerCommand.Execute(parameter);
                return;
            }

            _isRunning = true;

            // 动画和兜底计时器谁先到谁生效，保证删除动作一定会被执行
            var isCommitted = false;
            var fallback = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FadeOutMilliseconds + 150) };

            void Commit()
            {
                if (isCommitted)
                    return;

                isCommitted = true;
                fallback.Stop();
                container.BeginAnimation(UIElement.OpacityProperty, null);
                _isRunning = false;
                innerCommand.Execute(parameter);
            }

            fallback.Tick += (_, _) => Commit();

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(FadeOutMilliseconds))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) => Commit();

            fallback.Start();
            container.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        #endregion
    }
}
