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
        public Benevole Benevole { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int NbDemiJournees { get; set; }

        [Required]
        public decimal Distance { get; set; }
    }
}
