using DesktopClient.Helpers;
using DesktopClient.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.VM
{
    public class MainWindowVM : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }

        public MainWindowVM()
        {
            // стартуем со страницы авторизации
            CurrentViewModel = new AutorizationDialogVM(this);
        }

        public void NavigateTo(ViewModelBase vm) => CurrentViewModel = vm;
    }
}
