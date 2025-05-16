using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class UserService : RestaurantDataService<User>
    {
        public UserService(RestaurantDbContext context) : base(context)
        {
        }

        public override async Task<User?> GetByIdAsync(object id)
        {
            if (id is int userId)
            {
                return await _dbSet
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.IdUser == userId);
            }
            
            return null;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> VerifyUniqueEmailAsync(string email, int? userId = null)
        {
            if (userId.HasValue)
            {
                return !await _dbSet.AnyAsync(u => u.Email == email && u.IdUser != userId.Value);
            }
            
            return !await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> Authenticate(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return null;

            // Check password directly
            if (password == user.Password)
                return user;

            return null;
        }

        public async Task<User?> Register(User user, string password)
        {
            // Check if a user with this email already exists
            if (await GetByEmailAsync(user.Email) != null)
                return null;

            // Store the password in plain text
            user.Password = password;

            // Save the user
            await AddAsync(user);
            await SaveChangesAsync();
            return user;
        }
    }
} 