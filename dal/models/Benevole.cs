using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dal.models
{
    public class Benevole
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le nom de famille du bénévole")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le prénom du bénévole")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner l'adresse du bénévole")]
        [Display(Name = "Adresse")]
        public string AdresseLigne1 { get; set; }

        [Display(Name = "Adresse (ligne 2)")]
        public string AdresseLigne2 { get; set; }

        [Display(Name = "Adresse (ligne 3)")]
        public string AdresseLigne3 { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le code postal du bénévole")]
        [Display(Name = "Code postal")]
        public string CodePostal { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner la ville de domiciliation du bénévole")]
        [Display(Name = "Ville")]
        public string Ville { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner un numéro de téléphone du bénévole")]
        [Display(Name = "N° de téléphone")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner le centre de rattachement du bénévole")]
        [Display(Name = "Centre")]
        public int CentreID { get; set; }

        [Display(Name = "Centre")]
        public Centre Centre { get; set; }

        public string GetAdresseComplete(bool forHtml)
        {
            const string HTML_NEWLINE = "</br>";

            string newline;

            if (forHtml)
                newline = HTML_NEWLINE;
            else
                newline = Environment.NewLine;

            var builder = new StringBuilder();
            builder.Append(this.AdresseLigne1);

            if (!string.IsNullOrWhiteSpace(this.AdresseLigne2))
            {
                builder.Append(newline);
                builder.Append(this.AdresseLigne2);
            }

            if (!string.IsNullOrWhiteSpace(this.AdresseLigne3))
            {
                builder.Append(newline);
                builder.Append(this.AdresseLigne3);
            }

            builder.Append(newline);
            builder.Append(this.CodePostal);
            builder.Append(" ");
            builder.Append(this.Ville);

            return builder.ToString();
        }
    }
}
