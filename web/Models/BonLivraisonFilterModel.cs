using System;
using System.Collections.Generic;


namespace web.Models
{
    public class BonLivraisonFilterModel
    {
        public int CentreID { get; set; }
        public string NumBulletin { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        public IEnumerable<dal.models.Centre> Centres { get; set; }

        public IEnumerable<dal.models.BonLivraison> Results { get; set; }
    }

}
