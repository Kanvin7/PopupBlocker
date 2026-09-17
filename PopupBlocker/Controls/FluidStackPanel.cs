using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PopupBlocker.Controls
{
    /// <summary>
    /// 子元素位置变化时会自动补间过渡的排列面板。
    /// 列表增删引起的重排会表现为平滑滑动，而不是瞬间跳位。
    /// </summary>
    public class FluidStackPanel : StackPanel
    {
        #region 属性
        /// <summary>
        /// 补间时长（毫秒）。
        /// </summary>
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register(
                nameof(Duration),
                typeof(int),
                typeof(FluidStackPanel),
                new FrameworkPropertyMetadata(260));

        public int Duration
        {
            get => (int)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }
        #endregion

        #region 补间逻辑
        private readonly Dictionary<UIElement, TranslateTransform> _transforms = [];

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            /* 排列开始前先记下上一轮的槽位，作为补间起点。
             * 用 LayoutSlot 而不是 TranslatePoint，是因为前者不含 RenderTransform，
             * 否则正在进行的动画会把测量结果带偏。 */
            var previousTops = new Dictionary<UIElement, double>();
            foreach (UIElement child in Children)
            {
                if (child is not FrameworkElement element)
                    continue;

                var slot = LayoutInformation.GetLayoutSlot(element);
                if (!slot.IsEmpty)
                    previousTops[child] = slot.Top;
            }

            var finalSize = base.ArrangeOverride(arrangeSize);

            var duration = TimeSpan.FromMilliseconds(Duration);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            foreach (UIElement child in Children)
            {
                if (child is not FrameworkElement element)
                    continue;

                var slot = LayoutInformation.GetLayoutSlot(element);
                if (slot.IsEmpty)
                    continue;

                if (!previousTops.TryGetValue(child, out var previousTop))
                    continue;

                // 当前视觉位置 = 上一轮槽位 + 尚未播完的偏移，据此算出需要补上的差值
                var current = _transforms.TryGetValue(child, out var existing) ? existing.Y : 0d;
                var offset = previousTop + current - slot.Top;
                if (Math.Abs(offset) < 0.5)
                    continue;

                // 从"原来的位置"滑到"现在的位置"，也就是把偏移动画回 0
                if (existing is null)
                {
                    existing = new TranslateTransform();
                    _transforms[child] = existing;
                    child.RenderTransform = existing;
                }

                existing.BeginAnimation(TranslateTransform.YProperty, null);
                existing.BeginAnimation(
                    TranslateTransform.YProperty,
                    new DoubleAnimation { From = offset, To = 0, Duration = duration, EasingFunction = easing });
            }

            // 已经离开列表的子元素不再需要保留补间状态
            if (_transforms.Count > Children.Count)
            {
                foreach (var stale in _transforms.Keys.Where(key => !Children.Contains(key)).ToArray())
                    _transforms.Remove(stale);
            }

            return finalSize;
        }
        #endregion
    }
}
