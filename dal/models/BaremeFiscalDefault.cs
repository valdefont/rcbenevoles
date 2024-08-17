using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace dal.models
{
    public class BaremeFiscalDefault
    {
        [Display(Name = "id")]
        [Required]
        public int id { get; set; }

        [Display(Name = "Nombre de chevaux")]
        [Required]
        public int NbChevaux { get; set; }

        [Display(Name = "Limite en km")]
        [Required]
        public int LimiteKm { get; set; }

    }
}