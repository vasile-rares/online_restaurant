using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class ComandaService : RestaurantDataService<Comanda>
    {
        public ComandaService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Comanda>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .ToListAsync();
        }

        public override async Task<Comanda> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .FirstOrDefaultAsync(c => c.IdComanda == (Guid)id);
        }

        public async Task<IEnumerable<Comanda>> GetByUtilizatorAsync(int idUtilizator)
        {
            return await _dbSet
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .Where(c => c.IdUtilizator == idUtilizator)
                .OrderByDescending(c => c.DataComanda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comanda>> GetActiveOrdersAsync()
        {
            return await _dbSet
                .Include(c => c.Utilizator)
                .Include(c => c.ComandaPreparate)
                    .ThenInclude(cp => cp.Preparat)
                .Where(c => c.Stare != StareComanda.livrata && c.Stare != StareComanda.anulata)
                .OrderBy(c => c.DataComanda)
                .ToListAsync();
        }
    }
} 