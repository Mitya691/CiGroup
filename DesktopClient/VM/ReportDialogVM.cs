using DesktopClient.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopClient.VM
{
    public class ReportDialogVM : ViewModelBase
    {
        private readonly MainWindowVM _shell;

        public AsyncRelayCommand NavigateToHomeCommand { get; }

        public ReportDialogVM(MainWindowVM shell)
        {
            _shell = shell;

            NavigateToHomeCommand = new AsyncRelayCommand(NavigateHomeAsync);
        }

        private async Task NavigateHomeAsync()
        {
            var HomePage = App.Services.GetRequiredService<HomeDialogVM>();
            await HomePage.InitializeAsync();
            _shell.NavigateTo(HomePage);
        }
    }
}
