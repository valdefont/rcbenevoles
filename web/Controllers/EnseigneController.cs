
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
            var enseigne = await _context.Enseigne
                .Include(e => e.Details)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (enseigne == null) return NotFound();

            // Option: Bloquer la suppression si des détails existent
            // if (enseigne.Details?.Any() == true)
            // {
            //     SetGlobalMessage("Impossible de supprimer : des détails existent pour cette enseigne.", EGlobalMessageType.Error);
            //     return RedirectToAction(nameof(Delete), new { id });
            // }

            _context.Enseigne.Remove(enseigne);
            await _context.SaveChangesAsync();

            LogInfo("Enseigne #{EnseigneID} ({Name}) supprimée", enseigne.ID, enseigne.Name);
            SetGlobalMessage("L’enseigne a été supprimée avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }
    }
}
