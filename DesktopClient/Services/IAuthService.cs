using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.Services
{
    public interface IAuthService
    {
        Task<bool> SignInAsync(string login, string password, CancellationToken ct = default);
    }
}
