using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PrintIndexModel
    {
        public List<PrintIndexPeriod> Periods { get; set; }

        public dal.models.Benevole Benevole { get; set; }

        public Int16 SelectedYear { get; set; }

        public List<int> ListYears { get; set; }

        public PrintIndexModel()
        {
            this.Periods = new List<PrintIndexPeriod>();
        }
    }

    public class PrintIndexPeriod
    {
        public int PeriodId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
     
        public dal.models.Adresse Adresse { get; set; }

        public dal.models.Vehicule Vehicule { get; set; }

        public List<dal.models.Pointage> PointagesPeriod { get; set; }

        public string Remark { get; set; }
        public  List<PrintIndexPeriod> printIndexPeriods { get; set; }


    }
}
