
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    [Table("Collecte")]
    public class Collecte
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "L'enseigne est requise.")]
        [Display(Name = "Enseigne")]
        public int? EnseigneDetailID { get; set; }
        public EnseigneDetail EnseigneDetail { get; set; }

        [Required(ErrorMessage = "Le poids est requis.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le poids doit être positif.")]
        [Display(Name = "Poids")]
        [Column(TypeName = "decimal(10,2)")] // Precision: 10 digits, 2 decimals
        public decimal Poids { get; set; }

        [Required]
        [Display(Name = "Crée par")]
        public int? UtilisateurID { get; set; }
        public Utilisateur Utilisateur { get; set; }

        [Required]
        [Display(Name = "Crée le")]
        [DataType(DataType.Date)]
        public DateTime DateCreation { get; set; }
    }
}

