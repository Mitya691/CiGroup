using DesktopClient.Helpers;
using DesktopClient.VM;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.VM
{
    public class AutorizationDialogVM : ViewModelBase
    {
        private readonly MainWindowVM _shell;

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

        public RelayCommand LoginCommand { get; }
        public RelayCommand NavigateToRegisterCommand { get; }
        public RelayCommand ForgotPassCommand { get; }

        public AutorizationDialogVM(MainWindowVM shell)
        {
            _shell = shell;
            LoginCommand = new RelayCommand(DoLogin, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(() => _shell.NavigateTo(new RegisterDialogVM(_shell)));
        }
    
        protected void DoLogin()
        {
            //написать команду 
        }

        private bool CanLogin() =>
           !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
    }
}
