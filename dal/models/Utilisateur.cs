using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace dal.models
{
    public class Utilisateur
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Identifiant")]
        public string Login { get; set; }

        [Required]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Display(Name = "Centre géré")]
        public int? CentreID { get; set; }

        [Display(Name = "Centre géré")]
        public Centre Centre { get; set; }


        [Display(Name = "Pointages des bénévoles")]
        public bool app_pointage_benevoles { get; set; }

        [Display(Name = "Bon de livraison")]
        public bool app_bon_livraison { get; set; }

        [Display(Name = "Collecte")]
        public bool app_collecte { get; set; }

        public ICollection<EnseigneDetailUtilisateurs> EnseigneDetailUtilisateurs { get; set; } = new List<EnseigneDetailUtilisateurs>();



        public bool TestPassword(string password)
        {
            return EncryptPassword(password, GetSalt()) == this.Password;
        }

        public void SetPassword(string password)
        {
            this.Password = EncryptPassword(password, GetSalt());
        }

        private string GetSalt()
        {
            return $"{this.Login}/{this.Login.Length}";
        }

        public static string EncryptPassword(string password, string salt)
        {
            var hashMethod = SHA256.Create();

            var hash = hashMethod.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
            return Convert.ToBase64String(hash);
        }
    }
}
