using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PrintIndexModel
    {
        public List<PrintIndexPeriod> Periods;

        public dal.models.Benevole Benevole { get; set; }

        public PrintIndexModel()
        {
            this.Periods = new List<PrintIndexPeriod>();
        }
    }

    public class PrintIndexPeriod
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public dal.models.Adresse Adresse { get; set; }
    }
}
