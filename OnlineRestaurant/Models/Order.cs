using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public enum OrderStatus
    {
        registered,
        preparing,
        outForDelivery,
        delivered,
        canceled
    }

    public class Order
    {
        public Order()
        {
            IdOrder = Guid.NewGuid();
            OrderDate = DateTime.Now;
            Status = OrderStatus.registered;
            OrderDishes = new List<OrderDish>();
        }

        [Key]
        public Guid IdOrder { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [ForeignKey("IdUser")]
        public virtual User User { get; set; }

        public virtual ICollection<OrderDish> OrderDishes { get; set; }

        [NotMapped]
        public decimal TotalOrder => CalculateTotal();

        private decimal CalculateTotal()
        {
            decimal total = 0;
            if (OrderDishes != null)
            {
                foreach (var orderDish in OrderDishes)
                {
                    if (orderDish.Dish != null)
                    {
                        total += orderDish.Dish.Price * orderDish.Quantity;
                    }
                }
            }
            return total;
        }
    }
} 