using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DesktopClient.Services
{
    /// <summary>
    /// Реализация сервиса создания отчета в форматет xlsx 
    /// SDK NanoXLSX
    /// </summary>
    class ReportService : IReportService
    {
        public Task<byte[]> NewReport(DateTime? Start, DateTime? Stop)
        {

        }
    }
}
