
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using web.Models;
using System.Collections.Generic;

namespace web.Controllers
{
    [Authorize]
    public class EnseigneDetailController : RCBenevoleController
    {
        public EnseigneDetailController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: EnseigneDetails
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .Include(e => e.Centre)
                .OrderBy(e => e.Enseigne.Name)
                .ToListAsync();

            return View(list);
        }

        // GET: EnseigneDetails/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var detail = await _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .Include(e => e.Centre)
                .Include(e => e.EnseigneDetailUtilisateurs)
                    .ThenInclude(link => link.Utilisateur)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (detail == null)
                return NotFound();

            return View(detail);
        }

        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            var vm = new EnseigneDetailFormVm(); // Detail.EstActif = true by default

            LoadDropdowns(vm);
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create(EnseigneDetailFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(vm);
                return View(vm);
            }

            var detail = vm.Detail;

            _context.Add(detail);
            await _context.SaveChangesAsync(); // need detail.ID

            if (vm.UtilisateurIds != null && vm.UtilisateurIds.Count > 0)
            {
                var links = vm.UtilisateurIds
                    .Distinct()
                    .Select(uid => new EnseigneDetailUtilisateurs
                    {
                        EnseigneDetailID = detail.ID,
                        UtilisateurID = uid,
                        EstActif = true,
                        CreeLe = DateTime.UtcNow
                    });

                _context.EnseigneDetailUtilisateurs.AddRange(links);
                await _context.SaveChangesAsync();
            }

            LogInfo("EnseigneDetail #{DetailID} créé", detail.ID);
            SetGlobalMessage("Le détail enseigne a été créé avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }



        private void LoadDropdowns(EnseigneDetailFormVm vm)
        {
            ViewData["EnseigneID"] = new SelectList(
                _context.Enseigne.AsNoTracking(),
                "ID",
                "Name", 
                vm.Detail.EnseigneID
            );

            ViewData["CentreID"] = new SelectList(
                _context.Centres.AsNoTracking(),
                "ID",
                "Nom",
                vm.Detail.CentreID
            );

            var users = _context.Utilisateurs
                .AsNoTracking()
                .OrderBy(u => u.Login)
                .Select(u => new { u.ID, Label = u.Login })
                .ToList();

            ViewData["Utilisateurs"] = new MultiSelectList(users, "ID", "Label", vm.UtilisateurIds);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var detail = await _context.EnseigneDetail
                .Include(e => e.CodeCommune)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (detail == null) return NotFound();

            var selectedUserIds = await _context.EnseigneDetailUtilisateurs
                .Where(x => x.EnseigneDetailID == detail.ID)
                .Select(x => x.UtilisateurID)
                .ToListAsync();

            var vm = new EnseigneDetailFormVm
            {
                Detail = detail,
                UtilisateurIds = selectedUserIds
            };

            LoadDropdowns(vm);            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int id, EnseigneDetailFormVm vm)
        {
            if (id != vm.Detail.ID) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(vm);
                return View(vm);
            }

            // Update main entity
            _context.Update(vm.Detail);
            await _context.SaveChangesAsync();

            // Sync links
            var existingUserIds = await _context.EnseigneDetailUtilisateurs
                .Where(x => x.EnseigneDetailID == vm.Detail.ID)
                .Select(x => x.UtilisateurID)
                .ToListAsync();

            var newUserIds = (vm.UtilisateurIds ?? new List<int>()).Distinct().ToList();

            var toAdd = newUserIds.Except(existingUserIds).ToList();
            var toRemove = existingUserIds.Except(newUserIds).ToList();

            if (toAdd.Count > 0)
            {
                var newLinks = toAdd.Select(uid => new EnseigneDetailUtilisateurs
                {
                    EnseigneDetailID = vm.Detail.ID,
                    UtilisateurID = uid,
                    EstActif = true,
                    CreeLe = DateTime.UtcNow
                });
                _context.EnseigneDetailUtilisateurs.AddRange(newLinks);
            }

            if (toRemove.Count > 0)
            {
                var linksToRemove = await _context.EnseigneDetailUtilisateurs
                    .Where(x => x.EnseigneDetailID == vm.Detail.ID && toRemove.Contains(x.UtilisateurID))
                    .ToListAsync();

                _context.EnseigneDetailUtilisateurs.RemoveRange(linksToRemove);
            }

            if (toAdd.Count > 0 || toRemove.Count > 0)
                await _context.SaveChangesAsync();

            LogInfo("EnseigneDetail #{DetailID} modifié", vm.Detail.ID);
            SetGlobalMessage("Le détail enseigne a été modifié avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }

        // GET: EnseigneDetails/Delete/5
        // GET: EnseigneDetails/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var detail = await _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .Include(e => e.Centre)
                // load links + utilisateur
                .Include(e => e.EnseigneDetailUtilisateurs)
                    .ThenInclude(link => link.Utilisateur)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (detail == null)
                return NotFound();

            return View(detail);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detail = await _context.EnseigneDetail
                .AsNoTracking()
                .SingleOrDefaultAsync(d => d.ID == id);

            if (detail == null)
                return NotFound();

            // 1️⃣ Block: if ANY Collecte uses this EnseigneDetail → do NOT delete
            bool hasCollectes = await _context.Collecte
                .AsNoTracking()
                .AnyAsync(c => c.EnseigneDetailID == id);

            if (hasCollectes)
            {
                SetGlobalMessage(
                    "Impossible de supprimer : des collectes existent pour ce détail d'enseigne.",
                    EGlobalMessageType.Error
                );

                return RedirectToAction("Delete", new { id });
            }

            // 2️⃣ Block if assignments exist (optional, depending on your FK behavior)
            bool hasAssignments = await _context.EnseigneDetailUtilisateurs
                .AsNoTracking()
                .AnyAsync(a => a.EnseigneDetailID == id);

            if (hasAssignments)
            {
                SetGlobalMessage(
                    "Impossible de supprimer : ce détail d'enseigne possède des affectations utilisateurs.",
                    EGlobalMessageType.Error
                );

                return RedirectToAction("Delete", new { id });
            }

            // 3️⃣ Safe delete
            _context.EnseigneDetail.Remove(new EnseigneDetail { ID = id });

            try
            {
                await _context.SaveChangesAsync();
                SetGlobalMessage("Le détail d'enseigne a été supprimé avec succès.", EGlobalMessageType.Success);
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                SetGlobalMessage(
                    "Impossible de supprimer : des enregistrements liés existent.",
                    EGlobalMessageType.Error
                );
                return RedirectToAction("Delete", new { id });
            }
        }

        private bool DetailExists(int id)
        {
            return _context.EnseigneDetail.Any(e => e.ID == id);
        }
            

        [HttpGet]
        public async Task<IActionResult> SearchCommune(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new object[0]);

            var results = await _context.CodeCommune
                .Where(c => c.NomCommune.StartsWith(term))
                .OrderBy(c => c.NomCommune)
                .Select(c => new {
                    label = c.NomCommune,  // the text shown in autocomplete
                    value = c.NomCommune,  // when user clicks
                    id = c.ID              // the actual PK
                })
                .Take(20)
                .ToListAsync();

            return Json(results);
        }

    }
}

