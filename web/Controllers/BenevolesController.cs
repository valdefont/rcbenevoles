using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;

namespace web.Controllers
{
    public class BenevolesController : Controller
    {
        private readonly RCBenevoleContext _context;

        public BenevolesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Benevoles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Benevoles.ToListAsync());
        }

        // GET: Benevoles/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Benevoles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Benevoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Nom,Prenom,AdresseLigne1,AdresseLigne2,AdresseLigne3,CodePostal,Ville,Telephone")] Benevole benevole)
        {
            if (ModelState.IsValid)
            {
                _context.Add(benevole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(benevole);
        }

        // GET: Benevoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benevole = await _context.Benevoles.SingleOrDefaultAsync(m => m.ID == id);
            if (benevole == null)
            {
                return NotFound();
            }
            return View(benevole);
        }

        // POST: Benevoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nom,Prenom,AdresseLigne1,AdresseLigne2,AdresseLigne3,CodePostal,Ville,Telephone")] Benevole benevole)
        {
            if (id != benevole.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(benevole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BenevoleExists(benevole.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(benevole);
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
