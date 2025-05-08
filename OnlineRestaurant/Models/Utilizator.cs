using System.Collections.Generic;

namespace OnlineRestaurant.Models
{
    public class Utilizator
    {
        public int Id { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string AdresaLivrare { get; set; }
        public string Parola { get; set; }

        // Navigation property
        public virtual ICollection<Comanda> Comenzi { get; set; }
    }
} 