using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace dal.models
{
    public class Benevole
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le nom de famille du bénévole")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le prénom du bénévole")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner un numéro de téléphone du bénévole")]
        [Display(Name = "N° de téléphone")]
        public string Telephone { get; set; }

        public IEnumerable<Adresse> Adresses { get; set; }

        public IEnumerable<Pointage> Pointages { get; set; }

        [NotMapped]
        public Adresse CurrentAdresse => Adresses?.SingleOrDefault(a => a.IsCurrent);
    }
}