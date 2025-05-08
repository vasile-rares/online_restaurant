using System;
using System.Collections.Generic;

namespace OnlineRestaurant.Models
{
    public class Comanda
    {
        public int Id { get; set; }
        public Guid CodUnic { get; set; }
        public int UtilizatorId { get; set; }
        public DateTime DataComanda { get; set; }
        public string Stare { get; set; }

        // Navigation properties
        public virtual Utilizator Utilizator { get; set; }
        public virtual ICollection<Preparat> Preparate { get; set; }
    }
} 