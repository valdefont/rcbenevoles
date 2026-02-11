
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using web.Models;

namespace web.Controllers
{
    [Authorize]
    public class EnseigneController : RCBenevoleController
    {
        public EnseigneController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: /Enseignes
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Enseigne
                .Include(e => e.Details)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return View(list);
        }

        // GET: /Enseignes/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var enseigne = await _context.Enseigne
                .Include(e => e.Details)
                    .ThenInclude(d => d.CodeCommune)
                .Include(e => e.Details)
                    .ThenInclude(d => d.Centre)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (enseigne == null) return NotFound();

            return View(enseigne);
        }

        // GET: /Enseignes/Create
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Enseignes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([Bind("ID,Name")] Enseigne model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Enseigne.Add(model);
            await _context.SaveChangesAsync();

            LogInfo("Enseigne #{EnseigneID} ({Name}) créée", model.ID, model.Name);
            SetGlobalMessage("L’enseigne a été créée avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Enseignes/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var enseigne = await _context.Enseigne.SingleOrDefaultAsync(e => e.ID == id);
            if (enseigne == null) return NotFound();

            return View(enseigne);
        }

        // POST: /Enseignes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] Enseigne model)
        {
            if (id != model.ID) return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                LogInfo("Enseigne #{EnseigneID} ({Name}) modifiée", model.ID, model.Name);
                SetGlobalMessage("L’enseigne a été modifiée avec succès", EGlobalMessageType.Success);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Enseigne.AnyAsync(e => e.ID == model.ID);
                if (!exists) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Enseignes/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var enseigne = await _context.Enseigne
                .Include(e => e.Details)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (enseigne == null) return NotFound();

            return View(enseigne);
        }

        // POST: /Enseignes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check existence (don’t load graph)
            var enseigne = await _context.Enseigne
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.ID == id);

            if (enseigne == null)
                return NotFound();

            // 1) Block if ANY Collecte exists for ANY detail of this enseigne
            var hasCollectes = await (
                from c in _context.Collecte.AsNoTracking()
                join ed in _context.EnseigneDetail.AsNoTracking()
                    on c.EnseigneDetailID equals ed.ID
                where ed.EnseigneID == id
                select 1
            ).AnyAsync();

            if (hasCollectes)
            {
                SetGlobalMessage(
                    "Impossible de supprimer : des collectes existent pour cette enseigne.",
                    EGlobalMessageType.Error
                );

                // Important: return immediately to avoid calling SaveChanges
                return RedirectToAction(nameof(Delete), new { id });
            }

            // 2) Block if ANY EnseigneDetail exists (even if there are no collectes)
            var hasDetails = await _context.EnseigneDetail
                .AsNoTracking()
                .AnyAsync(d => d.EnseigneID == id);

            if (hasDetails)
            {
                SetGlobalMessage(
                    "Impossible de supprimer : des détails existent pour cette enseigne.",
                    EGlobalMessageType.Error
                );

                // Important: return immediately
                return RedirectToAction(nameof(Delete), new { id });
            }

            // 3) Safe to delete (no details, no collectes)
            // Use a stub entity to avoid loading the graph
            _context.Enseigne.Remove(new Enseigne { ID = id });

            try
            {
                await _context.SaveChangesAsync();

                LogInfo("Enseigne #{EnseigneID} ({Name}) supprimée", enseigne.ID, enseigne.Name);
                SetGlobalMessage("L’enseigne a été supprimée avec succès", EGlobalMessageType.Success);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Use string-first logging signature to avoid your CS1503 error
                LogError("Erreur lors de la suppression de l'enseigne #{EnseigneID}. Exception: {Exception}", id, ex);

                SetGlobalMessage(
                    "Suppression impossible : des enregistrements liés existent.",
                    EGlobalMessageType.Error
                );
                return RedirectToAction(nameof(Delete), new { id });
            }
        }


    }
}
