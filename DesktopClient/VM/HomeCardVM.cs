using DesktopClient.Helpers;
using DesktopClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.VM
{
    class HomeCardVM : ViewModelBase
    {
        private readonly Card _card;


        public AsyncRelayCommand SaveCardCommand { get; } // команда пишет в бд выбранный целевой силос

        public HomeCardVM()
        {
            SaveCardCommand = new AsyncRelayCommand(DoSave, CanSave);
        }

        private async Task DoSave()
        {

        }

        private bool CanSave()
        {
            return true;
        }

    }
}
    