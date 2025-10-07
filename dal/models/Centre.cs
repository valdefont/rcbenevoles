using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    public class Centre
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Rue { get; set; }
     
        public string CodePostal { get; set; }
       
        public string Commune { get; set; }

        public string Telephone { get; set; }

        public string EMail { get; set; }

        public int SiegeID { get; set; }

        public Siege Siege { get; set; }
    }
}