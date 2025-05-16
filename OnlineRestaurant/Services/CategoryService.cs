using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class CategoryService : RestaurantDataService<Category>
    {
        public CategoryService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Dishes)
                .Include(c => c.Menus)
                .ToListAsync();
        }

        public override async Task<Category> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(c => c.Dishes)
                .Include(c => c.Menus)
                .FirstOrDefaultAsync(c => c.IdCategory == (int)id);
        }
    }
} 