using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;

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
                    var orderDishes = await _context.Set<OrderDish>()
                        .Include(od => od.Dish)
                        .Where(od => od.IdOrder == orderId)
                        .ToListAsync();
                    order.OrderDishes = orderDishes;

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

                var orders = await _context.Set<Order>()
                    .FromSqlRaw("EXEC GetActiveOrders")
                    .ToListAsync();

                foreach (var order in orders)
                {
                    await _context.Entry(order).Reference(o => o.User).LoadAsync();

                    var orderDishes = await _context.Set<OrderDish>()
                        .Include(od => od.Dish)
                        .Where(od => od.IdOrder == order.IdOrder)
                        .ToListAsync();
                    order.OrderDishes = orderDishes;

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

        public async Task<Order?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@OrderId", SqlDbType.UniqueIdentifier) { Value = orderId },
                    new SqlParameter("@NewStatus", SqlDbType.Int) { Value = (int)newStatus }
                };

                var result = _context.Set<Order>()
                    .FromSqlRaw("EXEC UpdateOrderStatus @OrderId, @NewStatus", parameters)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (result != null)
                {
                    // Load related entities
                    var orderDishes = await _context.Set<OrderDish>()
                        .Include(od => od.Dish)
                        .Where(od => od.IdOrder == orderId)
                        .ToListAsync();
                    result.OrderDishes = orderDishes;

                    var orderMenus = await _context.Set<OrderMenu>()
                        .Include(om => om.Menu)
                            .ThenInclude(m => m.MenuDishes)
                                .ThenInclude(md => md.Dish)
                        .Where(om => om.IdOrder == orderId)
                        .ToListAsync();
                    result.OrderMenus = orderMenus;
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateOrderStatusAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}