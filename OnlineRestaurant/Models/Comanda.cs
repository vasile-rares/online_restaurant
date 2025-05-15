using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRestaurant.Models
{
    public enum StareComanda
    {
        inregistrata,
        sePregateste,
        aPlecatLaClient,
        livrata,
        anulata
    }

    public class Comanda
    {
        public Comanda()
        {
            IdComanda = Guid.NewGuid();
            DataComanda = DateTime.Now;
            Stare = StareComanda.inregistrata;
            ComandaPreparate = new List<ComandaPreparat>();
        }

        [Key]
        public Guid IdComanda { get; set; }

        [Required]
        public int IdUtilizator { get; set; }

        [Required]
        public DateTime DataComanda { get; set; }

        [Required]
        public StareComanda Stare { get; set; }

        [ForeignKey("IdUtilizator")]
        public virtual Utilizator Utilizator { get; set; }

        public virtual ICollection<ComandaPreparat> ComandaPreparate { get; set; }

        [NotMapped]
        public decimal TotalComanda => CalculeazaTotal();

        private decimal CalculeazaTotal()
        {
            decimal total = 0;
            if (ComandaPreparate != null)
            {
                foreach (var cp in ComandaPreparate)
                {
                    if (cp.Preparat != null)
                    {
                        total += cp.Preparat.Pret * cp.Cantitate;
                    }
                }
            }
            return total;
        }
    }
} 