using DesktopClient.Config;
using DesktopClient.Helpers;
using DesktopClient.Services;
using DesktopClient.VM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DesktopClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();

            string CS = "Server=localhost;Database=elevatordb;Uid=root;Pwd=Sd$#5186;SslMode=None;";
            string myarchiveCS = "Server=localhost;Database=myarchive;Uid=root;Pwd=Sd$#5186;SslMode=None;";
            // Сервисы

            serviceCollection.AddSingleton<ISettingsStore, SettingsStore>();

            serviceCollection.AddSingleton<ISQLRepository>(sp =>
            {
                var st = sp.GetRequiredService<ISettingsStore>();
                st.LoadSettings(); // гарантируем, что Current заполнен
                return new SQLRepository(st.Settings.DbConnectionString);
            });

            serviceCollection.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            serviceCollection.AddSingleton<IAuthService>(sp =>
                 new AuthService(CS, sp.GetRequiredService<IPasswordHasher>()));
            serviceCollection.AddSingleton<IRegistrationService>(sp =>
                 new RegistrationService(CS, sp.GetRequiredService<IPasswordHasher>()));
            
            serviceCollection.AddSingleton<CurrentUserStore>();
            serviceCollection.AddSingleton<IReportService>(sp =>
                new ReportService(sp.GetRequiredService<ISQLRepository>(), sp.GetRequiredService<CurrentUserStore>()));

            // VM
            serviceCollection.AddSingleton<MainWindowVM>();
            serviceCollection.AddTransient<AutorizationDialogVM>();
            serviceCollection.AddTransient<RegisterDialogVM>();
            serviceCollection.AddTransient<HomeDialogVM>();
            serviceCollection.AddTransient<ReportDialogVM>();

            Services = serviceCollection.BuildServiceProvider();

            Resources["CurrentUser"] = Services.GetRequiredService<CurrentUserStore>();

            // Запускаем главное окно
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowVM>()
            };
            mainWindow.Show();
        }
    }

}

