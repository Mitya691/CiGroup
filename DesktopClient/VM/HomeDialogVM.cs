using DesktopClient.Helpers;
using DesktopClient.Model;
using DesktopClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient.VM
{
    public class HomeDialogVM : ViewModelBase
    {
        private readonly ISQLRepository _repository;

        DateTime? FilterStart { get; set; }
        DateTime? FilterEnd { get; set; }

        RelayCommand NavigateToReport { get; }
        AsyncRelayCommand ApplyFilterCommand { get; }
        AsyncRelayCommand ResetFilterCommand { get; }

        private readonly List<string> targetSilosM1 = new List<string>() { "SL201", "SL202", "SL203", "SL204", "SL205", "SL206" };
        private readonly List<string> targetSilosM2 = new List<string>() { "SL1201", "SL1202", "SL1203", "SL1204", "SL1205", "SL1206" };

        private CancellationTokenSource? _cts;
        private readonly TimeSpan _pollPeriod;
        private Task? _pollTask;

        // маркер последнего закрытого интервала
        private DateTime _lastEndTs = new DateTime(1000, 1, 1);

        public ObservableCollection<HomeCardVM> Cards { get; } = new();

        public HomeDialogVM(MainWindowVM shell, ISQLRepository repository, TimeSpan? pollPeriod = null)
        {
            _repository = repository;
            _pollPeriod = pollPeriod ?? TimeSpan.FromSeconds(5);

            ApplyFilterCommand = new AsyncRelayCommand(GetCardsForFilter, CanGetCardsForFilter);
        }

        /// <summary>
        /// Первичная загрузка 30 карточек и запуск опроса
        /// </summary>
        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var cards = await _repository.GetLast30CardsAsync(ct); // ожидается по EndTs DESC
            App.Current.Dispatcher.Invoke(() =>
            {
                Cards.Clear();
                foreach (var c in cards)
                    if (c.Direction == "М1")
                    {
                        if (c.TargetSilo != null)
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM1, "Сохранено"));
                        else
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM1));
                    }
                    else if (c.Direction == "М2")
                    {
                        if (c.TargetSilo != null)
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM2, "Сохранено"));
                        else
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM2));
                    }
                if (Cards.Count > 0)
                    _lastEndTs = Cards[0].StopTime; // самая свежая сверху
            });

            StartPolling();
        }

        /// <summary>
        /// Запустить фоновый опрос БД
        /// </summary>
        public void StartPolling()
        {
            if (_pollTask is { IsCompleted: false }) return;

            _cts = new CancellationTokenSource();
            _pollTask = Task.Run(() => PollLoopAsync(_cts.Token));
        }

        /// <summary>
        /// Остановить фоновый опрос
        /// </summary>
        public void StopPolling()
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
        }

        private async Task PollLoopAsync(CancellationToken ct)
        {
            var timer = new PeriodicTimer(_pollPeriod);
            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    try
                    {
                        // спрашиваем репозиторий: появился ли новый закрытый перегон
                        var hasNew = await _repository.CheckCompletedIntervalAsync(_lastEndTs, ct);
                        if (!hasNew) continue;

                        // если есть — формируем и вставляем новую карточку
                        await _repository.InsertNewCard(_lastEndTs, ct);

                        var newestList = await _repository.GetLast30CardsAsync(ct);
                        var newest = newestList.FirstOrDefault();
                        if (newest is null) continue;

                        var option = newest.Direction == "M1" ? targetSilosM1 : targetSilosM2;
                        var vm = new HomeCardVM(_repository, newest, option);

                        if (vm.StopTime <= _lastEndTs) continue;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Cards.Insert(0, vm);
                            while (Cards.Count > 30)
                                Cards.RemoveAt(Cards.Count - 1);
                        });
                        _lastEndTs = vm.StopTime;
                    }
                    catch (OperationCanceledException) { }
                    catch
                    {
                        // TODO: логирование по месту, чтобы ошибка не рвала цикл
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        public async ValueTask DisposeAsync()
        {
            StopPolling();
            if (_pollTask is not null)
            {
                try { await _pollTask; } catch { /* ignore */ }
            }
            _cts?.Dispose();
        }

        private async Task GetCardsForFilter()
        {
            var cards = await _repository.GetCardsForFilter(FilterStart, FilterEnd);
            App.Current.Dispatcher.Invoke(() =>
            {
                Cards.Clear();
                foreach (var c in cards)
                    if (c.Direction == "М1")
                    {
                        if (c.TargetSilo != null)
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM1, "Сохранено"));
                        else
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM1));
                    }
                    else if (c.Direction == "М2")
                    {
                        if (c.TargetSilo != null)
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM2, "Сохранено"));
                        else
                            Cards.Add(new HomeCardVM(_repository, c, targetSilosM2));
                    }
            });
        }

        private bool CanGetCardsForFilter() =>
           true;
    }
}
