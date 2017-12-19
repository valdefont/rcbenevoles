using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class ChangePasswordModel
    {
        [Display(Name = "Login du compte")]
        [Required]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Required(ErrorMessage = "Vous devez confirmer le nouveau mot de passe")]
        public string NewPasswordConfirm { get; set; }
    }
}
