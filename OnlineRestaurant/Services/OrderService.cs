using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class OrderService : RestaurantDataService<Order>
    {
        public OrderService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .Include(c => c.OrderMenus)
                    .ThenInclude(om => om.Menu)
                .ToListAsync();
        }

        public override async Task<Order> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .Include(c => c.OrderMenus)
                    .ThenInclude(om => om.Menu)
                .FirstOrDefaultAsync(c => c.IdOrder == (Guid)id);
        }

        public async Task<IEnumerable<Order>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .Include(c => c.OrderMenus)
                    .ThenInclude(om => om.Menu)
                .Where(c => c.IdUser == userId)
                .OrderByDescending(c => c.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetActiveOrdersAsync()
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .Include(c => c.OrderMenus)
                    .ThenInclude(om => om.Menu)
                .Where(c => c.Status != OrderStatus.delivered && c.Status != OrderStatus.canceled)
                .OrderBy(c => c.OrderDate)
                .ToListAsync();
        }
    }
} 