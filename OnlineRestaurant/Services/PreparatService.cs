using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class PreparatService : RestaurantDataService<Preparat>
    {
        public PreparatService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Preparat>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Categorie)
                .Include(p => p.Fotografii)
                .Include(p => p.PreparatAlergeni)
                    .ThenInclude(pa => pa.Alergen)
                .Include(p => p.MeniuPreparate)
                    .ThenInclude(mp => mp.Meniu)
                .ToListAsync();
        }

        public override async Task<Preparat> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(p => p.Categorie)
                .Include(p => p.Fotografii)
                .Include(p => p.PreparatAlergeni)
                    .ThenInclude(pa => pa.Alergen)
                .Include(p => p.MeniuPreparate)
                    .ThenInclude(mp => mp.Meniu)
                .FirstOrDefaultAsync(p => p.IdPreparat == (int)id);
        }

        public async Task<IEnumerable<Preparat>> GetByCategorie(int idCategorie)
        {
            return await _dbSet
                .Include(p => p.Categorie)
                .Include(p => p.Fotografii)
                .Where(p => p.IdCategorie == idCategorie)
                .ToListAsync();
        }
    }
} 