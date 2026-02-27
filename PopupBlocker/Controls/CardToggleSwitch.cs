using System.Windows;
using Wpf.Ui.Controls;

namespace PopupBlocker.Controls
{
    public class CardToggleSwitch : System.Windows.Controls.ContentControl
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(CardToggleSwitch));

        public object Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(CardToggleSwitch),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        static CardToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CardToggleSwitch),
                new FrameworkPropertyMetadata(typeof(CardToggleSwitch)));
        }
    }
}
