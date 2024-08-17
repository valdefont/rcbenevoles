using System;
using System.ComponentModel.DataAnnotations;

namespace dal.models
{
    public class Frais
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int Annee { get; set; }

        [Required]
        public decimal TauxKilometrique { get; set; }
   
        public int? PourcentageVehiculeElectrique { get; set; }
    }
}
