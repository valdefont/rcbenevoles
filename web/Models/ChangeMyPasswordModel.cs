using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class ChangeMyPasswordModel : ChangePasswordModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        public string OldPassword { get; set; }
    }
}
