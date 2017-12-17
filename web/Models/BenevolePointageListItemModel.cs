using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class BenevolePointageListItemModel
    {
        public int BenevoleID { get; set; }

        public string BenevoleNom { get; set; }

        public string BenevolePrenom { get; set; }

        public DateTime? LastPointage { get; set; }
    }
}
