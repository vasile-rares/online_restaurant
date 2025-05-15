using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace OnlineRestaurant.Models
{
    public class Utilizator
    {
        public Utilizator()
        {
            Comenzi = new ObservableCollection<Comanda>();
        }

        [Key]
        public int IdUtilizator { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nume { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Prenume { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefon { get; set; }

        [MaxLength(255)]
        public string? AdresaLivrare { get; set; }

        [Required]
        [MaxLength(255)]
        public string ParolaHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = "Client"; // "Client", "Angajat", etc.

        public virtual ObservableCollection<Comanda> Comenzi { get; set; }

        public string NumeComplet => $"{Nume} {Prenume}";
    }
} 