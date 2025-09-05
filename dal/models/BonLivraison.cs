using System;
using System.ComponentModel.DataAnnotations;


namespace dal.models
{
    public class BonLivraison
    {
        [Key]
        public int ID { get; set; }
        
        [Display(Name = "Numéro de Bulletin")]
        public string NumBulletin { get; set; }

        [Required(ErrorMessage = "Le centre est requis.")]
        [Display(Name = "Centre")]
        public int? CentreID { get; set; }
        public Centre Centre { get; set; }

        [Required(ErrorMessage = "La date de livraison est requise.")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime DateLivraison { get; set; }


        [Required(ErrorMessage = "Le poids est requis.")]
        [Range(1, int.MaxValue, ErrorMessage = "Le poids doit être un nombre positif.")]
        [Display(Name = "Poids")]       
        public int Poids { get; set; }

        [Display(Name = "Crée par")]
        public int? UtilisateurID { get; set; }
        public Utilisateur Utilisateur { get; set; }

        [Required]
        [Display(Name = "Crée le")]
        [DataType(DataType.Date)]
        public DateTime DateCreation { get; set; }


    }
}
