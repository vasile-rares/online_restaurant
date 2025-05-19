using Microsoft.Extensions.Configuration;
using System;

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

        // Discount configuration methods
        public decimal GetValueThreshold()
        {
            return Convert.ToDecimal(_configuration["OrderDiscounts:ValueThreshold"]);
        }

        public int GetOrderCountThreshold()
        {
            return Convert.ToInt32(_configuration["OrderDiscounts:OrderCountThreshold"]);
        }

        public int GetTimeIntervalDays()
        {
            return Convert.ToInt32(_configuration["OrderDiscounts:TimeIntervalDays"]);
        }

        public decimal GetDiscountPercentage()
        {
            return Convert.ToDecimal(_configuration["OrderDiscounts:DiscountPercentage"]);
        }

        public decimal GetFreeShippingThreshold()
        {
            return Convert.ToDecimal(_configuration["OrderDiscounts:FreeShippingThreshold"]);
        }

        public decimal GetShippingCost()
        {
            return Convert.ToDecimal(_configuration["OrderDiscounts:ShippingCost"]);
        }

        public int GetLowStockThreshold()
        {
            // Try to get the value from config; default to 15 if not found
            if (int.TryParse(_configuration["OrderDiscounts:LowStockThreshold"], out int threshold))
                return threshold;
            return 15;
        }
        
        public decimal GetMenuDiscountPercentage()
        {
            // Try to get the menu discount percentage; default to 15% if not found
            if (decimal.TryParse(_configuration["OrderDiscounts:MenuDiscountPercentage"], out decimal discount))
                return discount;
            return 15.0m;
        }
    }
} 