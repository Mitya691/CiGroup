using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.Services
{
    interface IReportService
    {
        Task<string> NewReport(DateTime? Start, DateTime? Stop);
        void SendReport(string reportPath, string mail);
    }
}
