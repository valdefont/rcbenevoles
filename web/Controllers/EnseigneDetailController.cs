
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using web.Models;

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
                .Include(e => e.Benevole)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (detail == null)
                return NotFound();

            return View(detail);
        }

        // GET: EnseigneDetails/Create
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            LoadDropdowns();

            var model = new EnseigneDetail
            {
                EstActif = true   // <-- ensure checkbox is checked by default
            };

            return View(model);
        }

        // POST: EnseigneDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([Bind("ID,EnseigneID,CodeCommuneID,CentreID,Adresse,BenevoleID,EstActif")]
                                               EnseigneDetail detail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detail);
                await _context.SaveChangesAsync();
                LogInfo("EnseigneDetail #{DetailID} créé", detail.ID);
                SetGlobalMessage("Le détail enseigne a été créé avec succès", EGlobalMessageType.Success);

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(detail);
            return View(detail);
        }

        // GET: EnseigneDetails/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var detail = await _context.EnseigneDetail
                    .Include(e => e.CodeCommune)   // IMPORTANT
                    .FirstOrDefaultAsync(e => e.ID == id);


            if (detail == null)
                return NotFound();

            LoadDropdowns(detail);
            return View(detail);
        }

        // POST: EnseigneDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int id,
            [Bind("ID,EnseigneID,CodeCommuneID,CentreID,Adresse,BenevoleID,EstActif")]
            EnseigneDetail detail)
        {
            if (id != detail.ID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detail);
                    await _context.SaveChangesAsync();

                    LogInfo("EnseigneDetail #{DetailID} modifié", detail.ID);
                    SetGlobalMessage("Le détail enseigne a été modifié avec succès", EGlobalMessageType.Success);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetailExists(detail.ID))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(detail);
            return View(detail);
        }

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
                .Include(e => e.Benevole)
                .SingleOrDefaultAsync(e => e.ID == id);

            if (detail == null)
                return NotFound();

            return View(detail);
        }

        // POST: EnseigneDetails/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detail = await _context.EnseigneDetail.SingleOrDefaultAsync(e => e.ID == id);

            if (detail == null)
                return NotFound();

            _context.EnseigneDetail.Remove(detail);
            await _context.SaveChangesAsync();

            LogInfo("EnseigneDetail #{DetailID} supprimé", detail.ID);
            SetGlobalMessage("Le détail enseigne a été supprimé avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }

        private bool DetailExists(int id)
        {
            return _context.EnseigneDetail.Any(e => e.ID == id);
        }

        private void LoadDropdowns(EnseigneDetail selected = null)
        {
            ViewBag.EnseigneID = new SelectList(_context.Enseigne.OrderBy(e => e.Name),
                                                "ID", "Name", selected?.EnseigneID);

            ViewBag.CodeCommuneID = new SelectList(_context.CodeCommune.OrderBy(c => c.NomCommune),
                                                   "ID", "NomCommune", selected?.CodeCommuneID);

            ViewBag.CentreID = new SelectList(_context.Centres.OrderBy(c => c.Nom),
                                              "ID", "Nom", selected?.CentreID);

            ViewBag.BenevoleID = new SelectList(_context.Benevoles.OrderBy(b => b.Nom),
                                                "ID", "Nom", selected?.BenevoleID);
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

