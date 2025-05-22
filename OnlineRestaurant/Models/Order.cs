using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace OnlineRestaurant.Models
{
    public enum OrderStatus
    {
        registered,
        preparing,
        [EnumMember(Value = "out for delivery")]
        out_for_delivery,
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
            OrderMenus = new List<OrderMenu>();
            TotalAmount = 0;
        }

        [Key]
        public Guid IdOrder { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        [Column(TypeName = "datetime2(0)")]
        public DateTime OrderDate { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("IdUser")]
        public virtual User User { get; set; }

        public virtual ICollection<OrderDish> OrderDishes { get; set; }

        public virtual ICollection<OrderMenu> OrderMenus { get; set; }

        [NotMapped]
        public decimal TotalOrder => CalculateTotal();
        
        [NotMapped]
        public string ShortId => IdOrder.ToString().Substring(0, 8).ToUpper();

        private decimal CalculateTotal()
        {
            decimal total = 0;
            
            // Calculate total for individual dishes
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
            
            // Calculate total for menus
            if (OrderMenus != null)
            {
                foreach (var orderMenu in OrderMenus)
                {
                    if (orderMenu.Menu != null)
                    {
                        total += orderMenu.Menu.TotalPrice * orderMenu.Quantity;
                    }
                }
            }
            
            return total;
        }

        public void UpdateTotalAmount()
        {
            TotalAmount = CalculateTotal();
        }
    }
} 