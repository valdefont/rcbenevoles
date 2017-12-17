using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PointagesBenevoleModel
    {
        public DateTime MonthDate { get; set; }

        public dal.models.Benevole Benevole { get; set; }

        public List<CalendarRow> CalendarRows { get; set; }

        public PointagesBenevoleModel()
        {
            this.CalendarRows = new List<CalendarRow>();
        }
    }

    public class CalendarRow
    {
        public List<CalendarItem> Items { get; set; }

        public CalendarRow()
        {
            this.Items = new List<CalendarItem>();
        }
    }

    public class CalendarItem
    {
        public DateTime Date { get; set; }

        public dal.models.Pointage Pointage { get; set; }

        public bool IsCurrentMonth { get; set; }

        public string GetPointageCssClass()
        {
            if (this.Pointage == null || this.Pointage.NbDemiJournees == 0)
                return "pointage none";
            else
            {
                if(this.Pointage.NbDemiJournees == 1)
                    return "pointage half";
                else
                    return "pointage full";
            }
        }

        public string GetDayTextCssClass()
        {
            if (this.IsCurrentMonth)
                return "current_month";
            else
                return "other_month";
        }
    }
}

