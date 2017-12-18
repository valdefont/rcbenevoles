using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PrintModel
    {
        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public int MonthCount { get; set; }

        public dal.models.Benevole Benevole { get; set; }

        public decimal FraisKm { get; set; }

        public decimal TotalDistance { get; set; }
    }
}
