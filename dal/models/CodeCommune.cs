using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dal.models
{
    [Table("CodeCommune")]
    public class CodeCommune
    {
        [Key]
        public int ID { get; set; }

        [Column("Code_commune_INSEE")]
        public string CodeCommuneINSEE { get; set; }

        [Column("Nom_Commune")]
        [MaxLength(255)]
        public string NomCommune { get; set; }

        [Column("Code_postal")]
        [MaxLength(10)]
        public string CodePostal { get; set; }

        [Column("Libelle_acheminement")]
        [MaxLength(255)]
        public string LibelleAcheminement { get; set; }

        [Column("SousCommune")]
        [MaxLength(255)]
        public string? SousCommune { get; set; }
    }
}
