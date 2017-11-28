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
    public class CentresController : Controller
    {
        private readonly RCBenevoleContext _context;

        public CentresController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Centres
        public async Task<IActionResult> Index()
        {
            return View(await _context.Centres.ToListAsync());
        }

        // GET: Centres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Centres
                .SingleOrDefaultAsync(m => m.ID == id);
            if (centre == null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // GET: Centres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Centres/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Nom,Adresse")] Centre centre)
        {
            if (ModelState.IsValid)
            {
                _context.Add(centre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(centre);
        }

        // GET: Centres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Centres.SingleOrDefaultAsync(m => m.ID == id);
            if (centre == null)
            {
                return NotFound();
            }
            return View(centre);
        }

        // POST: Centres/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nom,Adresse")] Centre centre)
        {
            if (id != centre.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(centre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CentreExists(centre.ID))
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
            return View(centre);
        }

        // GET: Centres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Centres
                .SingleOrDefaultAsync(m => m.ID == id);
            if (centre == null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // POST: Centres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var centre = await _context.Centres.SingleOrDefaultAsync(m => m.ID == id);
            _context.Centres.Remove(centre);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CentreExists(int id)
        {
            return _context.Centres.Any(e => e.ID == id);
        }
    }
}
