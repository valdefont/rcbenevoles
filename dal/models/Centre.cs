using System;
using System.ComponentModel.DataAnnotations;

namespace dal.models
{
    public class Centre
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Adresse { get; set; }
    }
}
