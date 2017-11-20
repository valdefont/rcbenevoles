using System;
using System.ComponentModel.DataAnnotations;

namespace dal.models
{
    public class Benevole
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public string AdresseLigne1 { get; set; }

        public string AdresseLigne2 { get; set; }

        public string AdresseLigne3 { get; set; }

        [Required]
        public string CodePostal { get; set; }

        [Required]
        public string Ville { get; set; }

        [Required]
        public string Telephone { get; set; }

        [Required]
        public Centre Centre { get; set; }

    }
}
