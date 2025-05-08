using OnlineRestaurant.Data;
using OnlineRestaurant.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRestaurant.Services
{
    public class SecurityService
    {
        private readonly RestaurantDbContext _dbContext;

        public SecurityService(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Utilizator CurrentUser { get; private set; }

        public bool Authenticate(string email, string password)
        {
            var user = _dbContext.Utilizatori
                .FirstOrDefault(u => u.Email == email && u.Parola == password);
            
            if (user != null)
            {
                CurrentUser = user;
                return true;
            }
            
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
} 