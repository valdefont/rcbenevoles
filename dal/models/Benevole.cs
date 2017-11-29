using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dal.models
{
    public class Benevole
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public string AdresseLigne1 { get; set; }

        public string AdresseLigne2 { get; set; }

        public string AdresseLigne3 { get; set; }

        [Required]
        public string CodePostal { get; set; }

        [Required]
        public string Ville { get; set; }

        [Required]
        public string Telephone { get; set; }

        [Required]
        public int CentreID { get; set; }

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
