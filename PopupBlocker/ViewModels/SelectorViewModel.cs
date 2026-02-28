using PopupBlocker.Controls;
using PopupBlocker.Utility.Commons;
using System.Windows;
using System.Windows.Input;

namespace PopupBlocker.ViewModels
{
    public class SelectorViewModel : ViewModelServiceBase
    {
        public SelectorViewModel()
        {
            ConfirmToolVisibility = Visibility.Hidden;
        }

        private readonly WindowInfo _windowInfo = new();

        private Visibility _confirmToolVisibility;
        public Visibility ConfirmToolVisibility
        {
            get => _confirmToolVisibility;
            set
            {
                _confirmToolVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string _processName = string.Empty;
        public string ProcessName
        {
            get => _processName;
            set
            {
                _processName = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ClickCommand => new RelayCommand(obj =>
        {
            if (ConfirmToolVisibility != Visibility.Hidden)
                return;
            if (obj is SelectorWindow selector)
            {
                ConfirmToolVisibility = Visibility.Visible;
                _windowInfo.Handle = selector.GetWindowHandleFromMouse();
                ProcessName = _windowInfo.ProcessName;
            }
        });

        public ICommand ConfirmProcessCommand => new RelayCommand(obj =>
        {
            if (obj is SelectorWindow selector)
            {
                RuleConfigService.AddRule(ProcessName);
                selector.Close();
            }
        });

        public ICommand ConfirmWindowCommand => new RelayCommand(obj =>
        {
            if (obj is SelectorWindow selector)
            {
                RuleConfigService.AddRule(ProcessName, _windowInfo.WindowClass, _windowInfo.WindowTitle);
                selector.Close();
            }
        });

        public ICommand CancelCommand => new RelayCommand(obj =>
        {
            if (obj is SelectorWindow selector)
            {
                selector.Close();
            }
        });

        public ICommand EscapeCommand => new RelayCommand(obj =>
        {
            if (obj is SelectorWindow selector)
            {
                switch (ConfirmToolVisibility)
                {
                    case Visibility.Visible:
                        selector.StartMouseFollow();
                        ConfirmToolVisibility = Visibility.Hidden;
                        break;
                    case Visibility.Hidden:
                        selector.Close();
                        break;
                }
            }
        });
    }
}
