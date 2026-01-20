using System;
using System.Collections.Generic;


namespace web.Models
{
    public class CollecteFilterModel
    {
        public int EnseigneDetailID { get; set; }
        public int CentreID { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        public IEnumerable<dal.models.Centre> Centres { get; set; }

        public IEnumerable<dal.models.EnseigneDetail> EnseigneDetails { get; set; }
    }

}
