using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using OnlineRestaurant.ViewModels;

namespace OnlineRestaurant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public static IConfiguration Configuration { get; private set; }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Încărcăm configurația
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Configurare servicii
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMessage = $"A apărut o eroare neașteptată: {e.Exception.Message}";
                string detailsMessage = $"Detalii: {e.Exception.StackTrace}";

                // Log error to debug output
                System.Diagnostics.Debug.WriteLine($"[ERROR] {errorMessage}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] {detailsMessage}");

                // If there's an inner exception, show that too
                if (e.Exception.InnerException != null)
                {
                    string innerErrorMessage = $"Eroare internă: {e.Exception.InnerException.Message}";
                    string innerDetailsMessage = $"Detalii: {e.Exception.InnerException.StackTrace}";

                    System.Diagnostics.Debug.WriteLine($"[ERROR INNER] {innerErrorMessage}");
                    System.Diagnostics.Debug.WriteLine($"[ERROR INNER] {innerDetailsMessage}");

                    detailsMessage += $"\n\n{innerErrorMessage}\n{innerDetailsMessage}";
                }

                // Afișăm un mesaj de eroare utilizatorului
                MessageBox.Show($"{errorMessage}\n\n{detailsMessage}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // If an error occurs in our error handler, show a simpler message
                MessageBox.Show("A apărut o eroare neașteptată în aplicație.",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Marcăm excepția ca tratată pentru a preveni închiderea aplicației
                e.Handled = true;
            }
        }

        private void ConfigureServices(ServiceCollection services)
        {
            try
            {
                // Adăugăm configurația
                services.AddSingleton<IConfiguration>(Configuration);

                // Înregistrare servicii pentru aplicație
                services.AddSingleton<AppSettingsService>();
                services.AddSingleton<UserCredentialsService>();

                // Obținem o instanță a AppSettingsService pentru a accesa ConnectionString
                var serviceProvider = services.BuildServiceProvider();
                var appSettingsService = serviceProvider.GetRequiredService<AppSettingsService>();

                // Configurare DbContext cu setări pentru a permite operațiuni concurente
                services.AddDbContext<RestaurantDbContext>(options =>
                        {
                            options.UseSqlServer(appSettingsService.ConnectionString);

                            // Permite accesul simultan la context
                            options.EnableSensitiveDataLogging(true); // Pentru debugging

                            // Configurăm context-ul pentru a nu urmări modificările entităților,
                            // ceea ce îl face mai potrivit pentru operațiuni de doar citire
                            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                        }, ServiceLifetime.Transient); // Folosim Transient în loc de Scoped pentru a evita refolosirea aceluiași context

                // Servicii pentru entități
                services.AddTransient<IRestaurantDataService<Dish>, RestaurantDataService<Dish>>();
                services.AddTransient<IRestaurantDataService<Category>, RestaurantDataService<Category>>();
                services.AddTransient<IRestaurantDataService<Allergen>, RestaurantDataService<Allergen>>();
                services.AddTransient<IRestaurantDataService<Menu>, RestaurantDataService<Menu>>();
                services.AddTransient<IRestaurantDataService<User>, UserService>();
                services.AddTransient<IRestaurantDataService<Order>, OrderService>();

                // Servicii specializate
                services.AddTransient<OrderService>();
                services.AddTransient<UserService>();

                // ViewModels
                services.AddSingleton<UserViewModel>();
                services.AddTransient<ShoppingCartViewModel>();
                services.AddTransient<MenuRestaurantViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<RegisterViewModel>();
                services.AddTransient<UserProfileViewModel>();
                services.AddTransient<OrdersViewModel>();
                services.AddTransient<EmployeeViewModel>();
                services.AddTransient<MainViewModel>();

                // Vizualizări
                services.AddSingleton<MainWindow>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la configurarea serviciilor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Asigurăm că baza de date există
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                    context.Database.EnsureCreated();
                }

                var mainWindow = serviceProvider.GetService<MainWindow>();
                mainWindow?.Show();

                if (mainWindow == null)
                {
                    MessageBox.Show("Nu s-a putut crea fereastra principală. Verificați conexiunea la baza de date.",
                        "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la pornirea aplicației: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}