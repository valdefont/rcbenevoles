using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dal.models
{
    public class Siege
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Adresse { get; set; }

        public IEnumerable<Centre> Centres { get; set; }
    }
}
