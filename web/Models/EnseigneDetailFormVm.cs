using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dal.models
{
    public class EnseigneDetailFormVm
    {
        [Required]
        public EnseigneDetail Detail { get; set; } = new EnseigneDetail
        {
            EstActif = true
        };

        [Display(Name = "Utilisateurs")]
        public List<int> UtilisateurIds { get; set; } = new();
    }

}
