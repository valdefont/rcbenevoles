
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    [Table("Enseigne")]
    public class Enseigne
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        // Navigation property: one Enseigne → many EnseigneDetail
        public ICollection<EnseigneDetail> Details { get; set; } = new List<EnseigneDetail>();
    }
}
