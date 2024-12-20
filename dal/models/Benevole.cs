using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required(ErrorMessage = "Veuillez renseigner un numéro de téléphone du bénévole")]
        [Display(Name = "N° de téléphone")]
        public string Telephone { get; set; }

        public List<Adresse> Adresses { get; set; }

        public List<Pointage> Pointages { get; set; }

        public List<Vehicule> Vehicules { get; set; }

        [NotMapped]
        public Adresse CurrentAdresse => Adresses?.SingleOrDefault(a => a.IsCurrent);

        [NotMapped]
        public virtual Vehicule? CurrentVehicule => Vehicules?.SingleOrDefault(a => a.IsCurrent);

        public Adresse GetAdresseFromDate(DateTime date)
        {
            var addresses = this.Adresses
                .Where(a => a.DateChangement <= date)
                .OrderByDescending(a => a.DateChangement);

            return addresses.FirstOrDefault();
        }

        public Vehicule GetVehiculeFromDate(DateTime date)
        {
            var vehicules = this.Vehicules
                .Where(a => a.DateChangement <= date)
                .OrderByDescending(a => a.DateChangement);

            return vehicules.FirstOrDefault();
        }


        public IDictionary<DateTime, Adresse> GetAdressesInPeriod(DateTime periodStart, DateTime? periodEnd, bool excludeEnd)
        {
            IEnumerable<Adresse> adresses;
            
            if(excludeEnd)
                adresses = this.Adresses.Where(a => a.DateChangement < periodEnd);
            else
                adresses = this.Adresses.Where(a => a.DateChangement <= periodEnd);

            var result = adresses.ToDictionary(a => a.DateChangement);
            
            // on ajoute un element pour le debut de periode sauf si une adresse a été placée exactement sur la date de debut de periode
            result.TryAdd(periodStart, null);

            bool periodStartSet = false;
            Adresse currentAddress = this.Adresses.Where(a => a.IsCurrent == true).FirstOrDefault();

            foreach (var date in result.Keys.OrderBy(d => d))
            {
                var addr = result[date];

                if(addr == null)     // period start
                {
                    periodStartSet = true;
                    addr = currentAddress;
                    result[date] = currentAddress;
                }

                if(date >= periodStart)
                    break;

                currentAddress = addr;

                if(!periodStartSet)
                    result.Remove(date);
            }

            return result;
        }

        public IDictionary<DateTime, Vehicule> GetVehiculesInPeriod(DateTime periodStart, DateTime periodEnd, bool excludeEnd)
        {
            IEnumerable<Vehicule> vehicules;

            if (excludeEnd)
            {
                vehicules = this.Vehicules.Where(a => a.DateChangement < periodEnd).ToList();
            }
            else
            {
                vehicules = this.Vehicules.Where(a => a.DateChangement <= periodEnd).ToList();
            }
                

            var result = vehicules.ToDictionary(a => a.DateChangement);
           
            /*bool periodStartSet = false;
            Vehicule currentVehicule = null;

            foreach (var date in result.Keys.OrderBy(d => d))
            {
                var veh = result[date];

                if (veh == null)     // period start
                {
                    periodStartSet = true;
                    veh = currentVehicule;
                    result[date] = currentVehicule;
                }

                if (date >= periodStart)
                    break;

                currentVehicule = veh;

                if (!periodStartSet)
                    result.Remove(date);
            }*/

            return result;
        }
    }
}