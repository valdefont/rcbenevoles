using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;

namespace dal.models
{
    public class Vehicule
    {
       
        [Key]
        public int id { get; set; }

        [Display(Name = "Nombre de chevaux")]
        [Required]
        public int NbChevaux { get; set; }

        [Display(Name = "Limite en km")]
        [Required]
        public int BenevoleID { get; set; }

        public Benevole Benevole { get; set; }

        [Required]
        public Boolean IsCurrent { get; set; }

        [Required]
        public DateTime DateChangement { get; set; }

        [Display(Name = "Véhicule électrique")]
        [Required]
        public Boolean IsElectric { get; set; }

        public Vehicule()
        {
            string str = DateTime.MinValue.ToString("yyyy-MM-dd hh:MM:ss");
            this.DateChangement = DateTime.ParseExact(str, "yyyy-MM-dd hh:MM:ss", CultureInfo.InvariantCulture);
        }

    }

    
}