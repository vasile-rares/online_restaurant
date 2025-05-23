using Microsoft.EntityFrameworkCore;
using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;

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
            var emailParam = new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email };

            var result = await _context.Set<User>()
                .FromSqlRaw("EXEC GetUserByEmail @Email", emailParam)
                .ToListAsync();

            return result.FirstOrDefault();
        }

        public async Task<bool> VerifyUniqueEmailAsync(string email, int? userId = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = (object?)userId ?? DBNull.Value }
            };

            var results = await _context.Database
                .SqlQueryRaw<int>("EXEC VerifyUniqueEmail @Email, @UserId", parameters)
                .ToListAsync();

            return results.FirstOrDefault() == 1;
        }

        public async Task<User?> Authenticate(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return null;

            if (password == user.Password)
                return user;

            return null;
        }

        public async Task<User?> Register(User user, string password)
        {
            if (!await VerifyUniqueEmailAsync(user.Email))
                return null;

            user.Password = password;

            await AddAsync(user);
            await SaveChangesAsync();
            return user;
        }
    }
}