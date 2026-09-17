using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PopupBlocker.Controls
{
    public class InterceptorRulesCard : ItemsControl
    {
        public static readonly DependencyProperty SaveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(SaveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRulesCard));

        public ICommand SaveRuleCommand
        {
            get => (ICommand)GetValue(SaveRuleCommandProperty);
            set => SetValue(SaveRuleCommandProperty, value);
        }

        public static readonly DependencyProperty ResetCountCommandProperty =
            DependencyProperty.Register(
                nameof(ResetCountCommand),
                typeof(ICommand),
                typeof(InterceptorRulesCard));

        public ICommand ResetCountCommand
        {
            get => (ICommand)GetValue(ResetCountCommandProperty);
            set => SetValue(ResetCountCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveRuleCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveRuleCommand),
                typeof(ICommand),
                typeof(InterceptorRulesCard),
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

        public InterceptorRulesCard()
        {
            // 整组规则卡片的删除：要淡出的是这条进程规则本身
            _animatedRemoveCommand = new AnimatedRemoveCommand(
                this,
                static parameter => (parameter as RemoveRuleParameters)?.Item1 ?? parameter,
                () => RemoveRuleCommand);
        }

        private static void OnRemoveRuleCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((InterceptorRulesCard)d)._animatedRemoveCommand?.RaiseCanExecuteChanged();

        static InterceptorRulesCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InterceptorRulesCard),
                new FrameworkPropertyMetadata(typeof(InterceptorRulesCard)));
        }
    }
}
