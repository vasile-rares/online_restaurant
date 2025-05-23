using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurant.Models
{
    public class User
    {
        public User()
        {
            Orders = new ObservableCollection<Order>();
        }

        [Key]
        public int IdUser { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? DeliveryAddress { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Client"; // "Client" sau "Angajat"

        public virtual ObservableCollection<Order> Orders { get; set; }

        public string FullName => $"{LastName} {FirstName}";
    }
}