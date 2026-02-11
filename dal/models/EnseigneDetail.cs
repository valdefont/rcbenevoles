
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    [Table("EnseigneDetail")]
    public class EnseigneDetail
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "L'enseigne est obligatoire.")]
        [Display(Name = "Enseigne")]
        public int EnseigneID { get; set; }

        [Display(Name = "Enseigne")]
        public Enseigne Enseigne { get; set; }

        [Required(ErrorMessage = "La commune est obligatoire.")]
        [Display(Name = "Code Commune")]
        public int CodeCommuneID { get; set; }

        [Display(Name = "Code Commune")]
        public CodeCommune CodeCommune { get; set; }

        [Required(ErrorMessage = "Le centre est obligatoire.")]
        [Display(Name = "Centre")]
        public int CentreID { get; set; }

        [Display(Name = "Centre")]
        public Centre Centre { get; set; }

        
        [MaxLength(500)]
        [Display(Name = "Adresse")]
        public string Adresse { get; set; }       


        [Required]
        [Display(Name = "Actif")]
        public bool EstActif { get; set; } = true;

        public ICollection<EnseigneDetailUtilisateurs> EnseigneDetailUtilisateurs { get; set; } = new List<EnseigneDetailUtilisateurs>();
    }
}
