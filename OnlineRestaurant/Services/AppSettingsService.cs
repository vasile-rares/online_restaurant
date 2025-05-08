using Microsoft.Extensions.Configuration;

namespace OnlineRestaurant.Services
{
    public class AppSettingsService
    {
        private readonly IConfiguration _configuration;

        public AppSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ConnectionString => _configuration.GetConnectionString("DefaultConnection");
    }
} 