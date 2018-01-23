using System;
using System.Collections.Generic;
using System.Text;

namespace web.Models
{
    public class BenevoleAllAdresses
    {
        public dal.models.Benevole Benevole { get; set; }
        public IEnumerable<BenevoleAdresse> Adresses { get; set; }
    }

    public class BenevoleAdresse
    {
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public dal.models.Adresse Adresse { get; set; }

        public int PointagesCount { get; set; }
    }
}
