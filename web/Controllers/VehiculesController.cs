using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using web.Models;
using web.Filters;
using Microsoft.AspNetCore.Authorization;

namespace web.Controllers
{
    [Authorize]
    public class VehiculesController : RCBenevoleController
    {
        public VehiculesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Vehicules
        public async Task<IActionResult> Index(int idBenevole)
        {
            var benevole = await _context.Benevoles
                .Include(b => b.Vehicules).SingleOrDefaultAsync(b => b.ID == idBenevole);

            if(benevole == null)
                return NotFound();

            if(!IsBenevoleAllowed(benevole))
                return Forbid();

            var vehicules = new List<BenevoleVehicule>();

            var model = new BenevoleAllVehicules
            {
                Benevole = benevole,
                Vehicules = vehicules,
            };

            Vehicule currentVeh = null;

            foreach(var veh in benevole.Vehicules.OrderByDescending(a => a.DateChangement))
            {
                vehicules.Add(new BenevoleVehicule
                {
                    Vehicule = veh,
                    DateStart = (veh.DateChangement != DateTime.MinValue) ? (DateTime?)veh.DateChangement : null,
                    DateEnd = (currentVeh != null) ? (DateTime?)currentVeh.DateChangement : null,
                    PointagesCount = _context.Pointages.Count(p => p.VehiculeID == veh.id),
                });

                currentVeh = veh;
            }

            return View(model);
        }

        // GET: vehicules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicule = await _context.Vehicule.Include(a => a.Benevole)
                .SingleOrDefaultAsync(m => m.id == id);

            if (vehicule == null)
                return NotFound();

            ViewBag.DateChanged = false;         
            ViewBag.Force = false;        

            return View(vehicule);
        }

        // POST: vehicule/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,NbChevaux,DateChangement,IsElectric")] Vehicule vehicule, bool force = false)
        {
            ViewBag.DateChanged = false;           
            ViewBag.Force = force;     

            if (id != vehicule.id)
                return NotFound("Les identifiants ne correspondent pas");

            var existing = _context.Vehicule.Include(a => a.Benevole).SingleOrDefault(b => b.id == id);

            if(existing == null)
                return NotFound("Le véhicule n'existe pas");

            vehicule.BenevoleID = existing.BenevoleID;
            vehicule.Benevole = existing.Benevole;   

            // --- Recherche si changement de date
            IQueryable<Pointage> impactedForDateChange = null;

            if(vehicule.DateChangement < existing.DateChangement)
            {
                // On recherche si la date de changement demandée entre en conflit avec une autre adresse
                var vehiculesInPeriod = existing.Benevole
                    .GetVehiculesInPeriod(vehicule.DateChangement, existing.DateChangement, excludeEnd:false)
                    .Where(p => p.Value != null)
                    .Select(p => p.Value);

                var conflictVehicules = vehiculesInPeriod.Where(a => a.id != id)
                    .Where(a => a.DateChangement >= vehicule.DateChangement);

                if(conflictVehicules.Count() > 0)
                {
                    var firstConflict = conflictVehicules.OrderByDescending(a => a.DateChangement).First();
                    ModelState.AddModelError("DateChangement", $"Une ou plusieurs autres véhicules existent à partir du {firstConflict.DateChangement.ToString("d")}");
                }
                else
                {
                    impactedForDateChange = _context.Pointages
                        .Where(p => p.BenevoleID == existing.BenevoleID)
                        .Where(p => p.Date >= vehicule.DateChangement)
                        .Where(p => p.Date < existing.DateChangement);

                    ViewBag.DateChanged = true;
                    ViewBag.StartDate = vehicule.DateChangement;
                    ViewBag.EndDate = existing.DateChangement;
                
                    ViewBag.DateImpactedCount = impactedForDateChange.Count();
                }
            }
            else if(vehicule.DateChangement > existing.DateChangement)
            {
                // On recherche si la date de changement demandée entre en conflit avec une autre adresse
                var vehiculesInPeriod = existing.Benevole
                    .GetVehiculesInPeriod(existing.DateChangement, vehicule.DateChangement, excludeEnd:false)
                    .Where(p => p.Value != null)
                    .Select(p => p.Value);

                var conflictVehicules = vehiculesInPeriod.Where(a => a.id != id)
                    .Where(a => a.DateChangement <= vehicule.DateChangement);

                if(conflictVehicules.Count() > 0)
                {
                    var firstConflict = conflictVehicules.OrderBy(a => a.DateChangement).First();
                    ModelState.AddModelError("DateChangement", $"Une ou plusieurs autres véhicules existent avant le {firstConflict.DateChangement.ToString("d")}.");
                }
                else
                {
                    impactedForDateChange = _context.Pointages
                        .Where(p => p.BenevoleID == existing.BenevoleID)
                        .Where(p => p.Date >= existing.DateChangement)
                        .Where(p => p.Date < vehicule.DateChangement);

                    ViewBag.DateChanged = true;
                    ViewBag.StartDate = existing.DateChangement;
                    ViewBag.EndDate = vehicule.DateChangement;

                    ViewBag.DateImpactedCount = impactedForDateChange.Count();
                }
            }
                       

            if (!ModelState.IsValid)
                return View(vehicule);

            // --- Consequences changement date/centre
            if(!force)
            {
                if(impactedForDateChange != null && impactedForDateChange.Count() > 0)                 
                {
                    ViewBag.Force = true;
                    return View(vehicule);
                }
            }
            else
            {
                if(impactedForDateChange != null && impactedForDateChange.Count() > 0)
                    _context.Pointages.RemoveRange(impactedForDateChange);               
            }

            try
            {
                existing.DateChangement = vehicule.DateChangement;
                existing.NbChevaux = vehicule.NbChevaux;
                existing.IsElectric = vehicule.IsElectric;
                _context.Update(existing);

                await _context.SaveChangesAsync();

                LogInfo("Vehicule #{VehiculeID} (du bénévole #{BenevoleID}) modifié", vehicule.id, vehicule.BenevoleID);
                SetGlobalMessage("Le véhicule a été modifié avec succès", EGlobalMessageType.Success);
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!_context.Vehicule.Any(a => a.id == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("Details", "Benevoles", new { id = existing.BenevoleID });
        }

        // GET: Vehicule/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicule = await _context.Vehicule              
                .Include(a => a.Benevole).ThenInclude(b => b.Vehicules)
                .SingleOrDefaultAsync(a => a.id == id);

            if (vehicule == null)
                return NotFound();           

            if (vehicule.Benevole.Vehicules.Count() == 1)
                return BadRequest("Vous ne pouvez pas supprimer l'unique véhicule d'un bénévole");

            ViewBag.NombrePointages = await _context.Pointages.CountAsync(p => p.VehiculeID == id);

            return View(vehicule);
        }


        // POST: Benevoles/ChangeVehicule/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            int nbPointages = 0;
            var vehicule = await _context.Vehicule               
                .Include(a => a.Benevole).ThenInclude(b => b.Vehicules)
                .SingleOrDefaultAsync(a => a.id == id);

            if (vehicule == null)
                return NotFound();           

            if (vehicule.Benevole.Vehicules.Count() == 1)
                return BadRequest("Vous ne pouvez pas supprimer l'unique véhicule d'un bénévole");

            var pointages = _context.Pointages.Where(p => p.AdresseID == id);

            nbPointages = pointages.Count();
            if (nbPointages > 0)
            {                
                _context.Pointages.RemoveRange(pointages);
            }

            if(vehicule.IsCurrent)
            {
                vehicule.IsCurrent = false;
                var newcurrent = vehicule.Benevole.Vehicules
                    .OrderByDescending(a => a.DateChangement)
                    .FirstOrDefault(a => a.id != id);

                if (newcurrent != null)
                    newcurrent.IsCurrent = true;
            }

            _context.Vehicule.Remove(vehicule);

            await _context.SaveChangesAsync();

            if (pointages.Count() > 0)
                LogInfo("Suppression de {NombrePointages} pointages du benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom}) sur le véhicule #{VehiculeID}", nbPointages, vehicule.Benevole.ID, vehicule.Benevole.Prenom, vehicule.Benevole.Nom, vehicule.id);

            LogInfo("Vehicule {VehiculeID} supprimé du benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom})", vehicule.id, vehicule.Benevole.ID, vehicule.Benevole.Prenom, vehicule.Benevole.Nom);
            SetGlobalMessage("Le véhicule a été supprimé avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index), new { idBenevole = vehicule.BenevoleID });
        }


        private void SetViewBagCentres()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewBag.Centres = _context.Centres
                    .OrderBy(c => c.Nom)
                    .AsEnumerable();
            }
            else
            {
                ViewBag.Centres = _context.Centres
                    .Where(c => c.ID == GetCurrentUser().CentreID)
                    .AsEnumerable();
            }
        }
    }
}
