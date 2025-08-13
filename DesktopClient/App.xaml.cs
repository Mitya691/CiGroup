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

            string CS = "Server=localhost;Database=elevatordb;Uid=root;Pwd=Sd$#5186;SslMode=None;";

            // Сервисы
            serviceCollection.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            serviceCollection.AddSingleton<IAuthService>(sp =>
                 new AuthService(CS, sp.GetRequiredService<IPasswordHasher>()));
            serviceCollection.AddSingleton<IRegistrationService>(sp =>
                 new RegistrationService(CS, sp.GetRequiredService<IPasswordHasher>()));

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

