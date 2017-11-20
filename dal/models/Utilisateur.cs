using System;
using System.ComponentModel.DataAnnotations;

namespace dal.models
{
    public class Utilisateur
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        public Centre CentreGere { get; set; }
    }
}
