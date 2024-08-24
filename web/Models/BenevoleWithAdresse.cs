using System;
using System.Collections.Generic;
using System.Text;

namespace web.Models
{
    public class BenevoleWithAdresseAndVehicule
    {
        public dal.models.Benevole Benevole { get; set; }
        public dal.models.Adresse Adresse { get; set; }
        public dal.models.Vehicule Vehicule { get; set; }
    }
}
