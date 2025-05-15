using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class UtilizatorService : RestaurantDataService<Utilizator>
    {
        public UtilizatorService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<Utilizator?> GetByIdAsync(object id)
        {
            if (id is int utilizatorId)
            {
                return await _dbSet
                    .Include(u => u.Comenzi)
                    .FirstOrDefaultAsync(u => u.IdUtilizator == utilizatorId);
            }
            
            return null;
        }

        public async Task<Utilizator?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> VerificaEmailUnicAsync(string email, int? idUtilizator = null)
        {
            if (idUtilizator.HasValue)
            {
                return !await _dbSet.AnyAsync(u => u.Email == email && u.IdUtilizator != idUtilizator.Value);
            }
            
            return !await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<Utilizator?> Autentificare(string email, string parola)
        {
            var utilizator = await GetByEmailAsync(email);
            if (utilizator == null) return null;

            // Verifică parola cu hash-ul stocat
            if (VerificaParola(parola, utilizator.ParolaHash))
                return utilizator;

            return null;
        }

        public async Task<Utilizator?> Inregistrare(Utilizator utilizator, string parola)
        {
            // Verifică dacă există deja un utilizator cu acest email
            if (await GetByEmailAsync(utilizator.Email) != null)
                return null;

            // Generează hash pentru parolă
            utilizator.ParolaHash = HashParola(parola);

            // Salvează utilizatorul
            await AddAsync(utilizator);
            await SaveChangesAsync();
            return utilizator;
        }

        private string HashParola(string parola)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(parola));
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerificaParola(string parola, string hash)
        {
            var hashParola = HashParola(parola);
            return hashParola == hash;
        }
    }
} 