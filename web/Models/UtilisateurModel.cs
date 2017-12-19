using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class UtilisateurModel
    {
        [Required]
        public dal.models.Utilisateur Utilisateur { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Required(ErrorMessage = "Vous devez confirmer le nouveau mot de passe")]
        public string PasswordConfirm { get; set; }
    }
}
