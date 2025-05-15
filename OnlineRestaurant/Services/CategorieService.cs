using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class CategorieService : RestaurantDataService<Categorie>
    {
        public CategorieService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Categorie>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Preparate)
                .Include(c => c.Meniuri)
                .ToListAsync();
        }

        public override async Task<Categorie> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(c => c.Preparate)
                .Include(c => c.Meniuri)
                .FirstOrDefaultAsync(c => c.IdCategorie == (int)id);
        }
    }
} 