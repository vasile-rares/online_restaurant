using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public static class RestaurantDataServiceExtensions
    {
        public static async Task<IEnumerable<Dish>> GetDishByCategory(this IRestaurantDataService<Dish> dishService, RestaurantDbContext context, int idCategory)
        {
            return await context.Dishes
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.IdCategory == idCategory)
                .ToListAsync();
        }
    }

    public class RestaurantDataService<T> : IRestaurantDataService<T> where T : class
    {
        protected readonly RestaurantDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RestaurantDataService(RestaurantDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            // Special handling for each entity type
            if (typeof(T) == typeof(Category))
            {
                return (IEnumerable<T>)await _context.Categories
                    .Include(c => c.Dishes)
                    .Include(c => c.Menus)
                    .ToListAsync();
            }
            else if (typeof(T) == typeof(Dish))
            {
                return (IEnumerable<T>)await _context.Dishes
                    .Include(p => p.Category)
                    .Include(p => p.Photos)
                    .Include(p => p.DishAllergens)
                        .ThenInclude(pa => pa.Allergen)
                    .Include(p => p.MenuDishes)
                        .ThenInclude(mp => mp.Menu)
                    .ToListAsync();
            }
            else if (typeof(T) == typeof(Menu))
            {
                return (IEnumerable<T>)await _context.Menus
                    .Include(m => m.MenuDishes)
                        .ThenInclude(md => md.Dish)
                            .ThenInclude(d => d.DishAllergens)
                                .ThenInclude(da => da.Allergen)
                    .ToListAsync();
            }
            else if (typeof(T) == typeof(Order))
            {
                return (IEnumerable<T>)await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDishes)
                        .ThenInclude(od => od.Dish)
                    .Include(o => o.OrderMenus)
                        .ThenInclude(om => om.Menu)
                            .ThenInclude(m => m.MenuDishes)
                                .ThenInclude(md => md.Dish)
                    .ToListAsync();
            }
            else
            {
                // Default implementation for other types
                return await _dbSet.ToListAsync();
            }
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            // Special handling for each entity type
            if (typeof(T) == typeof(Category) && id is int categoryId)
            {
                return (T)(object)await _context.Categories
                    .Include(c => c.Dishes)
                    .Include(c => c.Menus)
                    .FirstOrDefaultAsync(c => c.IdCategory == categoryId);
            }
            else if (typeof(T) == typeof(Dish) && id is int dishId)
            {
                return (T)(object)await _context.Dishes
                    .Include(p => p.Category)
                    .Include(p => p.Photos)
                    .Include(p => p.DishAllergens)
                        .ThenInclude(pa => pa.Allergen)
                    .Include(p => p.MenuDishes)
                        .ThenInclude(mp => mp.Menu)
                    .FirstOrDefaultAsync(p => p.IdDish == dishId);
            }
            else if (typeof(T) == typeof(User) && id is int userId)
            {
                return (T)(object)await _context.Users
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.IdUser == userId);
            }
            else
            {
                // Default implementation for other types
                return await _dbSet.FindAsync(id);
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(object id)
        {
            T entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        
        public void DetachEntity(T entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
    }
} 