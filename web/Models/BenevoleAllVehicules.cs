using System;
using System.Collections.Generic;
using System.Text;

namespace web.Models
{
    public class BenevoleAllVehicules
    {
        public dal.models.Benevole Benevole { get; set; }
        public IEnumerable<BenevoleVehicule> Vehicules { get; set; }
    }

    public class BenevoleVehicule
    {
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public dal.models.Vehicule Vehicule { get; set; }

        public int PointagesCount { get; set; }
    }
}
