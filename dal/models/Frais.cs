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
        public string TauxKilometrique { get; set; }
    }
}
