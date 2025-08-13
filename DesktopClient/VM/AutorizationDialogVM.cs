using DesktopClient.Helpers;
using DesktopClient.VM;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopClient.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Eventing.Reader;

namespace DesktopClient.VM
{
    public class AutorizationDialogVM : ViewModelBase
    {
        private readonly MainWindowVM _shell;
        private readonly IAuthService _authService;

        private string _login;
        public string Login
        {
            get { return _login; }
            set 
            { 
                if (Set(ref _login, value))
                    LoginCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (Set(ref _password, value))
                    LoginCommand?.RaiseCanExecuteChanged();
            }
        }

        public AsyncRelayCommand LoginCommand { get; }
        public RelayCommand NavigateToRegisterCommand { get; }
        public RelayCommand ForgotPassCommand { get; }

        public AutorizationDialogVM(MainWindowVM shell, IAuthService authService)
        {
            
            _shell = shell;
            _authService = authService;

            LoginCommand = new AsyncRelayCommand(DoLoginAsync, CanLogin);

            NavigateToRegisterCommand = new RelayCommand(() => _shell.NavigateTo(App.Services.GetRequiredService<RegisterDialogVM>()));
        }
        
        private async Task DoLoginAsync()
        {
            try
            {
                bool ok = await _authService.SignInAsync(_login, _password);
                //написать команду 

                if (ok)
                    _shell.NavigateTo(new HomeDialogVM(_shell));
                else
                    System.Windows.MessageBox.Show("Неверный логин или пароль");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не удалось подключиться к базе данных");
            }
        }

        private bool CanLogin() =>
           !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
    }
}
