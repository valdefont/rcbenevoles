using System;
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
