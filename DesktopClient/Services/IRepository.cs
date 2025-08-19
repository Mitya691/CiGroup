using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopClient.Model;

namespace DesktopClient.Services
{
    internal interface IRepository : IDisposable
    {
        // Стартовая загрузка – 30 свежих карточек
        Task<List<Card>> GetLast30CardsAsync(CancellationToken ct = default);

        // Догон по маркеру (по EndTime или Id)
        Task<List<Card>> GetCardsClosedAfterAsync(DateTime lastEndTime, int take = 100, CancellationToken ct = default);

        // Сохранение выбора/правок с карточки
        Task UpdateCardSelectionAsync(long id, string userSelection, string userName, CancellationToken ct = default);
    }
}
