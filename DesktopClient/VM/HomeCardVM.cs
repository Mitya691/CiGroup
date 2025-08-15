using DesktopClient.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.VM
{
    class HomeCardVM : ViewModelBase
    {
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public string SourceSilo { get; }
        public string TargetSilo { get; }
        public string Direction { get; }

        private double _weight1;
        public double Weight1
        {
            get { return _weight1; }
            set
            {
                Set(ref _weight1, value);
            }
        }

        private double _weight2;
        public double Weight2
        {
            get { return _weight2; }
            set
            {
                Set(ref _weight2, value);
            }
        }

        private double _mainWeight;
        public double MainWeight
        {
            get { return _mainWeight; }
            set
            {
                Set(ref _mainWeight, value);
            }
        }

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
    