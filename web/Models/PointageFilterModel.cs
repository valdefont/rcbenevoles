using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PointageFilterModel
    {
        public IEnumerable<dal.models.Centre> Centres { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un centre")]
        public int? CentreID { get; set; }

        public string Term { get; set; }
    }
}
