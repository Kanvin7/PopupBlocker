using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PopupBlocker.Controls
{
    public class InterceptorRuleCard : ItemsControl
    {
        public static readonly DependencyProperty SaveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(SaveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRuleCard));

        public ICommand SaveRuleCommand
        {
            get => (ICommand)GetValue(SaveRuleCommandProperty);
            set => SetValue(SaveRuleCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRuleCard),
                new PropertyMetadata(null, OnRemoveRuleCommandChanged));

        public ICommand RemoveRuleCommand
        {
            get => (ICommand)GetValue(RemoveRuleCommandProperty);
            set => SetValue(RemoveRuleCommandProperty, value);
        }

        /// <summary>
        /// 界面实际绑定的删除命令。行为与 <see cref="RemoveRuleCommand"/> 一致，
        /// 只是会先让对应卡片淡出，再执行删除。
        /// </summary>
        public ICommand AnimatedRemoveCommand => _animatedRemoveCommand;

        private readonly AnimatedRemoveCommand _animatedRemoveCommand;

        public InterceptorRuleCard()
        {
            // 展开后的子规则删除：要淡出的是被点到的那一条
            _animatedRemoveCommand = new AnimatedRemoveCommand(
                this,
                static parameter => (parameter as RemoveRuleParameters)?.Item2 ?? parameter,
                () => RemoveRuleCommand);
        }

        private static void OnRemoveRuleCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((InterceptorRuleCard)d)._animatedRemoveCommand?.RaiseCanExecuteChanged();

        static InterceptorRuleCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InterceptorRuleCard),
                new FrameworkPropertyMetadata(typeof(InterceptorRuleCard)));
        }
    }
}
