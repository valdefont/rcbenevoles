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

namespace web.Controllers
{
    [AtLeastOneCenterExists]
    public class BenevolesController : RCBenevoleController
    {
        public BenevolesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Benevoles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Benevoles.Include(b => b.Centre).ToListAsync());
        }

        // GET: Benevoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var benevole = await _context.Benevoles.Include(b => b.Centre)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (benevole == null)
                return NotFound();

            return View(benevole);
        }

        // GET: Benevoles/Create
        public IActionResult Create()
        {
            ViewBag.Centres = _context.Centres
                .OrderBy(c => c.Nom)
                .AsEnumerable();

            return View();
        }

        // POST: Benevoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Nom,Prenom,AdresseLigne1,AdresseLigne2,AdresseLigne3,CodePostal,Ville,Telephone,CentreID")] Benevole benevole)
        {
            if (!_context.ContainsCentre(benevole.CentreID))
                ModelState.AddModelError("CentreID", "Le centre n'existe pas");

            if (!ModelState.IsValid)
                return View(benevole);

            _context.Add(benevole);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Benevoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            ViewBag.Centres = _context.Centres
                .OrderBy(c => c.Nom)
                .AsEnumerable();

            var benevole = await _context.Benevoles.SingleOrDefaultAsync(m => m.ID == id);

            if (benevole == null)
                return NotFound();


            return View(benevole);
        }

        // POST: Benevoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nom,Prenom,AdresseLigne1,AdresseLigne2,AdresseLigne3,CodePostal,Ville,Telephone,CentreID")] Benevole benevole)
        {
            if (id != benevole.ID)
                return NotFound();

            if (!_context.ContainsCentre(benevole.CentreID))
                ModelState.AddModelError("CentreID", "Le centre n'existe pas");

            var user = GetCurrentUser();

            if (user.Centre != null)
            {
                if (benevole.CentreID != user.Centre.ID)
                    ModelState.AddModelError("CentreID", "Vous ne pouvez pas créer de bénévole sur un autre centre que celui qui vous est affecté");
            }

            if (!ModelState.IsValid)
                return View(benevole);

            try
            {
                _context.Update(benevole);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BenevoleExists(benevole.ID))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Benevoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benevole = await _context.Benevoles
                .SingleOrDefaultAsync(m => m.ID == id);
            if (benevole == null)
            {
                return NotFound();
            }

            return View(benevole);
        }

        // POST: Benevoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benevole = await _context.Benevoles.SingleOrDefaultAsync(m => m.ID == id);
            _context.Benevoles.Remove(benevole);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BenevoleExists(int id)
        {
            return _context.Benevoles.Any(e => e.ID == id);
        }
    }
}
