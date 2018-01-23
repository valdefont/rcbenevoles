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
    public class AdressesController : RCBenevoleController
    {
        public AdressesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Adresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var adresse = await _context.Adresse.Include(a => a.Benevole)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (adresse == null)
                return NotFound();

            if(GetCurrentUser().CentreID != null && GetCurrentUser().CentreID != adresse.CentreID)
                return Forbid();

            ViewBag.DateChanged = false;
            ViewBag.CenterChanged = false;
            ViewBag.Force = false;

            SetViewBagCentres();

            return View(adresse);
        }

        // POST: Adresse/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,BenevoleID,DateChangement,AdresseLigne1,AdresseLigne2,AdresseLigne3,CodePostal,Ville,CentreID,DistanceCentre")] Adresse adresse, bool force = false)
        {
            ViewBag.DateChanged = false;
            ViewBag.CenterChanged = false;
            ViewBag.Force = force;

            SetViewBagCentres();

            if (id != adresse.ID)
                return NotFound("Les identifiants ne correspondent pas");

            var existing = _context.Adresse.Include(a => a.Benevole).ThenInclude(b => b.Adresses)
                .SingleOrDefault(b => b.ID == id);

            if(existing == null)
                return NotFound("L'adresse n'existe pas");

            adresse.BenevoleID = existing.BenevoleID;
            adresse.Benevole = existing.Benevole;

            if (!_context.ContainsCentre(adresse.CentreID))
                ModelState.AddModelError("CentreID", "Le centre n'existe pas");

            var user = GetCurrentUser();

            if (user.Centre != null)
            {
                if (adresse.CentreID != user.Centre.ID)
                    ModelState.AddModelError("CentreID", "Vous ne pouvez pas positionner l'adresse sur un autre centre que celui qui vous est affecté");
            }


            // --- Recherche si changement de date
            IQueryable<Pointage> impactedForDateChange = null;

            if(adresse.DateChangement < existing.DateChangement)
            {
                // On recherche si la date de changement demandée entre en conflit avec une autre adresse
                var adressesInPeriod = existing.Benevole
                    .GetAdressesInPeriod(adresse.DateChangement, existing.DateChangement)
                    .Where(p => p.Value != null)
                    .Select(p => p.Value);

                var conflictAdresses = adressesInPeriod.Where(a => a.ID != id)
                    .Where(a => a.DateChangement >= adresse.DateChangement);

                if(conflictAdresses.Count() > 0)
                {
                    var firstConflict = conflictAdresses.OrderByDescending(a => a.DateChangement).First();
                    ModelState.AddModelError("DateChangement", $"Une ou plusieurs autres adresses existent à partir du {firstConflict.DateChangement.ToString("d")}");
                }
                else
                {
                    impactedForDateChange = _context.Pointages
                        .Where(p => p.BenevoleID == existing.BenevoleID)
                        .Where(p => p.Date >= adresse.DateChangement)
                        .Where(p => p.Date < existing.DateChangement);

                    ViewBag.DateChanged = true;
                    ViewBag.StartDate = adresse.DateChangement;
                    ViewBag.EndDate = existing.DateChangement;
                
                    ViewBag.DateImpactedCount = impactedForDateChange.Count();
                }
            }
            else if(adresse.DateChangement > existing.DateChangement)
            {
                // On recherche si la date de changement demandée entre en conflit avec une autre adresse
                var adressesInPeriod = existing.Benevole
                    .GetAdressesInPeriod(existing.DateChangement, adresse.DateChangement)
                    .Where(p => p.Value != null)
                    .Select(p => p.Value);

                var conflictAdresses = adressesInPeriod.Where(a => a.ID != id)
                    .Where(a => a.DateChangement <= adresse.DateChangement);

                if(conflictAdresses.Count() > 0)
                {
                    var firstConflict = conflictAdresses.OrderBy(a => a.DateChangement).First();
                    ModelState.AddModelError("DateChangement", $"Une ou plusieurs autres adresses existent avant le {firstConflict.DateChangement.ToString("d")}.");
                }
                else
                {
                    impactedForDateChange = _context.Pointages
                        .Where(p => p.BenevoleID == existing.BenevoleID)
                        .Where(p => p.Date >= existing.DateChangement)
                        .Where(p => p.Date < adresse.DateChangement);

                    ViewBag.DateChanged = true;
                    ViewBag.StartDate = existing.DateChangement;
                    ViewBag.EndDate = adresse.DateChangement;

                    ViewBag.DateImpactedCount = impactedForDateChange.Count();
                }
            }

            // --- Recherche si changement de centre
            IQueryable<Pointage> impactedForCenterChange = null;

            if(adresse.CentreID != existing.CentreID)
            {
                ViewBag.CenterChanged = true;
                impactedForCenterChange = _context.Pointages.Where(p => p.AdresseID == id);

                ViewBag.CenterImpactedCount = impactedForCenterChange.Count();
            }

            if (!ModelState.IsValid)
                return View(adresse);

            // --- Consequences changement date/centre
            if(!force)
            {
                if((impactedForDateChange != null && impactedForDateChange.Count() > 0) 
                || (impactedForCenterChange != null && impactedForCenterChange.Count() > 0))
                {
                    ViewBag.Force = true;
                    return View(adresse);
                }
            }
            else
            {
                if(impactedForDateChange != null && impactedForDateChange.Count() > 0)
                    _context.Pointages.RemoveRange(impactedForDateChange);

                if(impactedForCenterChange != null && impactedForCenterChange.Count() > 0)
                    _context.Pointages.RemoveRange(impactedForCenterChange);
            }

            try
            {
                existing.DateChangement = adresse.DateChangement;
                existing.AdresseLigne1 = adresse.AdresseLigne1;
                existing.AdresseLigne2 = adresse.AdresseLigne2;
                existing.AdresseLigne3 = adresse.AdresseLigne3;
                existing.CodePostal = adresse.CodePostal;
                existing.Ville = adresse.Ville;
                existing.CentreID = adresse.CentreID;
                existing.DistanceCentre = adresse.DistanceCentre;
                
                _context.Update(existing);

                await _context.SaveChangesAsync();

                LogInfo("Adresse #{AdresseID} (du bénévole #{BenevoleID}) modifiée", adresse.ID, adresse.BenevoleID);
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!_context.Adresse.Any(a => a.ID == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("Details", "Benevoles", new { id = existing.BenevoleID });
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
