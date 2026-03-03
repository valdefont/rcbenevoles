using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web.Models
{


    public class CollecteFormViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Le centre est requis.")]
        public int? CentreID { get; set; }

        [Required(ErrorMessage = "L'enseigne est requise.")]
        public int? EnseigneDetailID { get; set; }

        [Required(ErrorMessage = "Le poids est requis.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le poids doit être positif.")]
        public decimal? Poids { get; set; }

        public IEnumerable<SelectListItem> Centres { get; set; }
        public IEnumerable<SelectListItem> EnseignesDetail { get; set; }
    }



}
