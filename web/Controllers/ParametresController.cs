using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dal;
using Microsoft.AspNetCore.Authorization;
using web.Models;
using Microsoft.VisualBasic;
using System.Collections;
using dal.models;
using System.Diagnostics.Eventing.Reader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Globalization;

namespace web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ParametresController : RCBenevoleController
    {
        public ParametresController(RCBenevoleContext context)
        {
            _context = context;
        }

        [HttpPost, ActionName("GetRates")]
        public IActionResult GetRates()
        {
            Int16 Year = Convert.ToInt16(Request.Form["year"]);
            IEnumerable<BaremeFiscalLigne> bfl = ListBaremeFiscalLigne(Year);
            ViewModelBaremeFiscalLigne mymodel = new ViewModelBaremeFiscalLigne();
            if (bfl.Count() > 0) {
                mymodel.ListBaremeFiscalLigne = bfl;
            }
            else
            {
                List<BaremeFiscalDefault> bfd = ListBaremeFiscalDefault();
                List<BaremeFiscalLigne> bfl_new = new List<BaremeFiscalLigne>();
                foreach (BaremeFiscalDefault bfd_elem in bfd)
                {
                    BaremeFiscalLigne bfl_elem = new BaremeFiscalLigne();

                    bfl_elem.Annee = Year;
                    bfl_elem.NbChevaux = bfd_elem.NbChevaux;
                    bfl_elem.LimiteKm = bfd_elem.LimiteKm;
                    bfl_elem.Coef = 0;
                    bfl_elem.Ajout = 0;
                    bfl_new.Add(bfl_elem);

                }
                _context.BaremeFiscalLignes.AddRange(bfl_new);
                _context.SaveChanges();
                mymodel.ListBaremeFiscalLigne = bfl_new;
            }

            // Get Frais
            Frais LigneFrais = GetFraisWithYear(Year);
            if (LigneFrais == null)
            {
                LigneFrais = new Frais{
                    Annee = Year,
                    TauxKilometrique = 0,
                    PourcentageVehiculeElectrique = 20
                };
            }
            mymodel.PourcentageVehiculeElectrique = LigneFrais.PourcentageVehiculeElectrique ?? 0;

            return PartialView(mymodel);
        }

            public IActionResult Index()
        {
            ViewModelBaremeFiscalLigne mymodel = new ViewModelBaremeFiscalLigne();
            mymodel.ListYears = GetAllYearsPlusOne();

            return View(mymodel);
        }

        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult PostIndex()
        {
            ViewModelBaremeFiscalLigne mymodel = new ViewModelBaremeFiscalLigne();

            var listKeys = new List<string>(Request.Form.Keys.Where(s=>s.StartsWith("coef_")));

            var formdata = Request.Form;
            bool haserror = false;
            var yearstring = Convert.ToInt16(formdata["selectYear"].ToString());
            int PourcentageElec = Convert.ToInt16(formdata["PourcentageVehiculeElectrique"]);

            // Save Frais
            var lnFrais = _context.Frais.SingleOrDefault(f => f.Annee == yearstring);
            if (lnFrais==null)
            {
                lnFrais = new Frais
                {
                    Annee = yearstring,
                    PourcentageVehiculeElectrique = PourcentageElec,
                    TauxKilometrique = 0
                };
                _context.Frais.AddRange(lnFrais);
            }
            else
            {
                lnFrais.PourcentageVehiculeElectrique = PourcentageElec;
            }
           
            _context.SaveChanges();

            // Save Rates 
            foreach (string key in listKeys)
            {

                string[] formStr = key.Split("_");
                int nbchevaux = Convert.ToInt16(formStr[1]);
                int limitekm = Convert.ToInt32(formStr[2]);               

                string coef_val = formdata["coef_" + formStr[1].ToString() + "_" + formStr[2].ToString()].ToString();

                if (!decimal.TryParse(coef_val, NumberStyles.Any, new CultureInfo("fr-FR"), out decimal coef) || coef < 0 || coef == 0)
                {
                    haserror = true;
                }
                else
                {
                    decimal.TryParse(formdata["ajout_" + formStr[1].ToString() + "_" + formStr[2].ToString()], out decimal ajout);
                    var lnbareme = _context.BaremeFiscalLignes.SingleOrDefault(f => f.Annee == yearstring
                                                                                && f.NbChevaux == nbchevaux
                                                                                && f.LimiteKm == limitekm);                   
                    lnbareme.Coef = coef;
                    lnbareme.Ajout = ajout;                  
                    
                }
            }

            if (haserror)
                ViewBag.ErrorMessage = "Un des taux n'est pas correct. Veuillez le corriger pour sauvegarder l'ensemble des données";
            else
            {
                LogInfo("Taux kilométriques modifié");
                SetGlobalMessage("Les taux ont été sauvegardés avec succès", EGlobalMessageType.Success);
                _context.SaveChanges();
            }
          
            mymodel.ListYears = GetAllYearsPlusOne();
            return View(mymodel);
        }

        

        private List<int> GetAllYearsPlusOne()
        {
            var list = new List<int>(_context.BaremeFiscalLignes.Select(x => x.Annee).Distinct().OrderBy(s => s));
            if (list!=null && list.Count() > 0)
            {
                var maxAnnee = _context.BaremeFiscalLignes.Max(f => f.Annee);
                list.Add(maxAnnee + 1);
            }
            else
            {
                list.Add(Convert.ToInt32(DateTime.Now.Year.ToString("0000")));
            }
            
            return list;

        }

        private IEnumerable<dal.models.BaremeFiscalLigne> ListBaremeFiscalLigne(Int16 SelectedYear)
        {
            IEnumerable<dal.models.BaremeFiscalLigne> list = new List<dal.models.BaremeFiscalLigne>(_context.BaremeFiscalLignes)
                                                            .Where(x=>x.Annee==SelectedYear).OrderBy(s=>s.NbChevaux).ThenBy(s=>s.LimiteKm);          

            return list;
        }

        private dal.models.Frais GetFraisWithYear(int Year)
        {           
            Frais LigneFrais = new List<dal.models.Frais>(_context.Frais).Where(x => x.Annee == Year).FirstOrDefault();  
            return LigneFrais;
        }

        private List<dal.models.BaremeFiscalDefault> ListBaremeFiscalDefault()
        {
            List<dal.models.BaremeFiscalDefault> list = new List<dal.models.BaremeFiscalDefault>(_context.BaremeFiscalDefault.OrderBy(f => f.id));

            return list;
        }

        public IActionResult Save()
        {
            ViewModelBaremeFiscalLigne mymodel = new ViewModelBaremeFiscalLigne();
            mymodel.ListYears = GetAllYearsPlusOne();
            return View(mymodel);
        }
    }
}