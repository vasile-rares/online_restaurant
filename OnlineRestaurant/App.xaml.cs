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
            // Afișăm un mesaj de eroare utilizatorului
            MessageBox.Show($"A apărut o eroare neașteptată: {e.Exception.Message}\n\nDetalii: {e.Exception.StackTrace}", 
                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Marcăm excepția ca tratată pentru a preveni închiderea aplicației
            e.Handled = true;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            try
            {
                // Adăugăm configurația
                services.AddSingleton<IConfiguration>(Configuration);
                
                // Înregistrare servicii pentru aplicație
                services.AddSingleton<AppSettingsService>();
                
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
                services.AddTransient<IRestaurantDataService<Preparat>, PreparatService>();
                services.AddTransient<IRestaurantDataService<Categorie>, CategorieService>();
                services.AddTransient<IRestaurantDataService<Alergen>, RestaurantDataService<Alergen>>();
                services.AddTransient<IRestaurantDataService<Meniu>, RestaurantDataService<Meniu>>();
                services.AddTransient<IRestaurantDataService<Utilizator>, UtilizatorService>();
                services.AddTransient<IRestaurantDataService<Comanda>, ComandaService>();

                // Servicii specializate
                services.AddTransient<PreparatService>();
                services.AddTransient<CategorieService>();
                services.AddTransient<ComandaService>();
                services.AddTransient<UtilizatorService>();

                // ViewModels
                services.AddTransient<MeniuRestaurantViewModel>();
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

