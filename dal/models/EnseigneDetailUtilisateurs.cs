using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    [Table("EnseigneDetailUtilisateurs")]
    public class EnseigneDetailUtilisateurs
    {
        // Composite PK: configure in OnModelCreating (see below)

        [Required]
        [Display(Name = "Enseigne détail")]
        public int EnseigneDetailID { get; set; }

        [Display(Name = "Enseigne détail")]
        public EnseigneDetail EnseigneDetail { get; set; }

        [Required]
        [Display(Name = "Utilisateur")]
        public int UtilisateurID { get; set; }

        [Display(Name = "Utilisateur")]
        public Utilisateur Utilisateur { get; set; }

        [Required]
        [Display(Name = "Actif")]
        public bool EstActif { get; set; } = true;

        [Display(Name = "Créé le")]
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    }
}
