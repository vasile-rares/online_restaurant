using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using System;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class DatabaseInitializer
    {
        private readonly RestaurantDbContext _context;

        public DatabaseInitializer(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Asigură-te că baza de date există (o va crea dacă nu există)
                await _context.Database.EnsureCreatedAsync();
                
                // Alternativ, poți folosi migrările pentru a crea/actualiza schema
                // await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // Loghează eroarea sau afișează un mesaj utilizatorului
                System.Diagnostics.Debug.WriteLine($"Eroare la inițializarea bazei de date: {ex.Message}");
                throw; // Re-aruncă excepția pentru a fi tratată în UI
            }
        }
    }
} 