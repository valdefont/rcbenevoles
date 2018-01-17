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
        [Display(Name = "Centre")]
        public int CentreID { get; set; }

        [Display(Name = "Centre")]
        public Centre Centre { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Demi-journées")]
        [Range(1, 2, ErrorMessage = "Le nombre de demi-journées doit être 1 ou 2")]
        public int NbDemiJournees { get; set; }
    }
}
