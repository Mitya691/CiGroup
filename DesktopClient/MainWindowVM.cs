using DesktopClient.Helpers;
using DesktopClient.VM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.VM
{
    public class MainWindowVM : ViewModelBase
    {
        private readonly IServiceProvider _sp;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }

        public MainWindowVM(IServiceProvider sp)
        {
            _sp = sp;
            // ВАЖНО: создаём дочернюю VM так, чтобы shell пришёл параметром, а не через DI
            CurrentViewModel = ActivatorUtilities.CreateInstance<AutorizationDialogVM>(_sp, this);
        }

        public void NavigateTo(ViewModelBase vm) => CurrentViewModel = vm;
    }
}
