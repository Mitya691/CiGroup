using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopClient.Model;

namespace DesktopClient.Services
{
    public interface ISQLRepository 
    {
        // Стартовая загрузка – 30 свежих карточек
        Task<List<Card>> GetLast30CardsAsync(CancellationToken ct = default);

        Task<List<Card>> GetCardsForInterval(DateTime? filterStart, DateTime? filterStop, CancellationToken ct = default);

        Task<bool> CheckCompletedIntervalAsync(DateTime lastEndInterval, CancellationToken ct = default);

        Task UpdateCardTargetSiloAsync(long id, string targetSilo, CancellationToken ct = default);

        Task InsertNewCard(DateTime lastEndInterval, CancellationToken ct = default);
    }
}
