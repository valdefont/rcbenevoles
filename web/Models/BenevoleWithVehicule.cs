using System;
using System.Collections.Generic;
using System.Text;

namespace web.Models
{
    public class BenevoleWithVehicule
    {
        public dal.models.Benevole Benevole { get; set; }
        public dal.models.Vehicule Vehicule { get; set; }
    }
}
