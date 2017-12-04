using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    public class Pointage
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Bénévole")]
        public int BenevoleID { get; set; }

        [Display(Name = "Bénévole")]
        public Benevole Benevole { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Demi-journées")]
        [Range(0, 2, ErrorMessage = "Le nombre de demi-journées doit être compris entre 0 et 2")]
        public int NbDemiJournees { get; set; }

        [Required]
        [Display(Name = "Distance A/R")]
        public decimal Distance { get; set; }
    }
}
