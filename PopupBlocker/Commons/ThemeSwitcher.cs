using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Wpf.Ui.Appearance;

namespace PopupBlocker.Commons
{
    /// <summary>
    /// 应用主题的切换入口。
    /// 切换时会把当前界面截一张快照盖在最上层再淡出，
    /// 让新旧配色之间形成交叉过渡，而不是整体颜色瞬间跳变。
    /// </summary>
    public static class ThemeSwitcher
    {
        private const int CrossFadeMilliseconds = 260;

        /// <summary>
        /// 当前是否为深色主题。
        /// </summary>
        public static bool IsDark => ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;

        /// <summary>
        /// 切换主题。
        /// </summary>
        /// <param name="isDark">是否使用深色主题。</param>
        /// <param name="animate">是否带过渡动画。</param>
        public static void Apply(bool isDark, bool animate = true)
        {
            var theme = isDark ? ApplicationTheme.Dark : ApplicationTheme.Light;
            if (ApplicationThemeManager.GetAppTheme() == theme)
                return;

            /* 先截图再换主题，快照上留下的才是切换前的界面。
             * 截图属于锦上添花，整段都做了保护：
             * 万一画不出来，也只是少了过渡，主题该换还是要换。 */
            var window = FindTargetWindow();
            var host = window?.Content as Panel;
            var snapshot = host is null || !animate ? null : TryCreateSnapshotSafely(host);

            ApplicationThemeManager.Apply(theme);
            UpdateWindowBackground(window, theme);

            if (host is null || snapshot is null)
                return;

            try
            {
                // 根布局通常分了行，快照要跨满所有行列才能盖住整个窗口
                if (host is Grid grid)
                {
                    Grid.SetRowSpan(snapshot, Math.Max(1, grid.RowDefinitions.Count));
                    Grid.SetColumnSpan(snapshot, Math.Max(1, grid.ColumnDefinitions.Count));
                }

                host.Children.Add(snapshot);

                var crossFade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(CrossFadeMilliseconds))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                crossFade.Completed += (_, _) => host.Children.Remove(snapshot);
                snapshot.BeginAnimation(UIElement.OpacityProperty, crossFade);
            }
            catch
            {
                host.Children.Remove(snapshot);
            }
        }

        /// <summary>
        /// 跟随系统主题。
        /// </summary>
        public static void ApplySystemTheme() => ApplicationThemeManager.ApplySystemTheme();

        private static Window? FindTargetWindow()
        {
            var windows = Application.Current?.Windows;
            if (windows is null)
                return null;

            Window? fallback = null;
            foreach (Window window in windows)
            {
                if (!window.IsVisible || window.Content is not Panel)
                    continue;

                if (window.IsActive)
                    return window;

                fallback ??= window;
            }

            return fallback;
        }

        private static void UpdateWindowBackground(Window? window, ApplicationTheme theme)
        {
            /* 库只会替 Application.MainWindow 更新窗口背景，
             * 而本程序的主窗口并不是第一个创建的窗口，
             * 所以这里显式补一次，否则切换后窗口底色和标题栏会停在原来的配色。 */
            if (window is not Wpf.Ui.Controls.FluentWindow fluentWindow)
                return;

            try
            {
                WindowBackgroundManager.UpdateBackground(window, theme, fluentWindow.WindowBackdropType);
            }
            catch
            {
                // 背景效果只关乎观感，失败不影响主题本身已经切换
            }
        }

        private static Image? TryCreateSnapshotSafely(Panel host)
        {
            try
            {
                return TryCreateSnapshot(host);
            }
            catch
            {
                return null;
            }
        }

        private static Image? TryCreateSnapshot(Panel host)
        {
            if (host.ActualWidth < 1 || host.ActualHeight < 1)
                return null;

            try
            {
                var dpi = VisualTreeHelper.GetDpi(host);
                var bitmap = new RenderTargetBitmap(
                    (int)Math.Ceiling(host.ActualWidth * dpi.DpiScaleX),
                    (int)Math.Ceiling(host.ActualHeight * dpi.DpiScaleY),
                    96 * dpi.DpiScaleX,
                    96 * dpi.DpiScaleY,
                    PixelFormats.Pbgra32);
                bitmap.Render(host);
                bitmap.Freeze();

                return new Image
                {
                    Source = bitmap,
                    Stretch = Stretch.None,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    IsHitTestVisible = false,
                    Focusable = false,
                };
            }
            catch
            {
                // 截图失败就退化成直接切换，不影响功能
                return null;
            }
        }
    }
}
