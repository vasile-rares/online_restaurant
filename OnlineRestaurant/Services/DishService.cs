using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class DishService : RestaurantDataService<Dish>
    {
        public DishService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Dish>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Include(p => p.DishAllergens)
                    .ThenInclude(pa => pa.Allergen)
                .Include(p => p.MenuDishes)
                    .ThenInclude(mp => mp.Menu)
                .ToListAsync();
        }

        public override async Task<Dish> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Include(p => p.DishAllergens)
                    .ThenInclude(pa => pa.Allergen)
                .Include(p => p.MenuDishes)
                    .ThenInclude(mp => mp.Menu)
                .FirstOrDefaultAsync(p => p.IdDish == (int)id);
        }

        public async Task<IEnumerable<Dish>> GetByCategory(int idCategory)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.IdCategory == idCategory)
                .ToListAsync();
        }
    }
} 