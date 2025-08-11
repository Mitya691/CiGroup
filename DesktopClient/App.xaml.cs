using DesktopClient.Services;
using DesktopClient.VM;
using Microsoft.Extensions.DependencyInjection;
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

            // Сервисы
            serviceCollection.AddSingleton<IAuthService, AuthService>();
           // serviceCollection.AddSingleton<IUserService, UserService>();

            // VM
            serviceCollection.AddSingleton<MainWindowVM>();
            serviceCollection.AddTransient<AutorizationDialogVM>();
            serviceCollection.AddTransient<RegisterDialogVM>();
            serviceCollection.AddTransient<HomeDialogVM>();

            Services = serviceCollection.BuildServiceProvider();

            // Запускаем главное окно
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowVM>()
            };
            mainWindow.Show();
        }
    }

}

