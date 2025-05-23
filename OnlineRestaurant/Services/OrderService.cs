using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

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
                        .ThenInclude(m => m.MenuDishes)
                            .ThenInclude(md => md.Dish)
                .ToListAsync();
        }

        public override async Task<Order> GetByIdAsync(object id)
        {
            try
            {
                var orderId = (Guid)id;
                var parameter = new SqlParameter("@OrderId", System.Data.SqlDbType.UniqueIdentifier) { Value = orderId };

                var order = await _context.Set<Order>()
                    .FromSqlRaw("EXEC GetOrderById @OrderId", parameter)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (order != null)
                {
                    // Load OrderDishes with their Dishes
                    var orderDishes = await _context.Set<OrderDish>()
                        .Include(od => od.Dish)
                        .Where(od => od.IdOrder == orderId)
                        .ToListAsync();
                    order.OrderDishes = orderDishes;

                    // Load OrderMenus with their Menus and MenuDishes
                    var orderMenus = await _context.Set<OrderMenu>()
                        .Include(om => om.Menu)
                            .ThenInclude(m => m.MenuDishes)
                                .ThenInclude(md => md.Dish)
                        .Where(om => om.IdOrder == orderId)
                        .ToListAsync();
                    order.OrderMenus = orderMenus;
                }

                return order;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetByIdAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Include(c => c.OrderDishes)
                    .ThenInclude(cp => cp.Dish)
                .Include(c => c.OrderMenus)
                    .ThenInclude(om => om.Menu)
                        .ThenInclude(m => m.MenuDishes)
                            .ThenInclude(md => md.Dish)
                .Where(c => c.IdUser == userId)
                .OrderByDescending(c => c.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetActiveOrdersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting GetActiveOrdersAsync");

                // Get the base orders from stored procedure
                var orders = await _context.Set<Order>()
                    .FromSqlRaw("EXEC GetActiveOrders")
                    .ToListAsync();

                // Load related data using the DbContext
                foreach (var order in orders)
                {
                    await _context.Entry(order).Reference(o => o.User).LoadAsync();
                    
                    // Load OrderDishes with their Dishes
                    var orderDishes = await _context.Set<OrderDish>()
                        .Include(od => od.Dish)
                        .Where(od => od.IdOrder == order.IdOrder)
                        .ToListAsync();
                    order.OrderDishes = orderDishes;

                    // Load OrderMenus with their Menus and MenuDishes
                    var orderMenus = await _context.Set<OrderMenu>()
                        .Include(om => om.Menu)
                            .ThenInclude(m => m.MenuDishes)
                                .ThenInclude(md => md.Dish)
                        .Where(om => om.IdOrder == order.IdOrder)
                        .ToListAsync();
                    order.OrderMenus = orderMenus;
                }

                System.Diagnostics.Debug.WriteLine($"Found {orders.Count} active orders");
                return orders.OrderByDescending(o => o.OrderDate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetActiveOrdersAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 