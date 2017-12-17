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

namespace web.Controllers
{
    public class PointagesController : RCBenevoleController
    {
        public PointagesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Pointages
        public IActionResult Index()
        {
            PointageFilterModel model = new PointageFilterModel
            {
                Term = string.Empty
            };

            if (User.IsInRole("SuperAdmin"))
                model.Centres = _context.Centres;
            else
                model.CentreID = GetCurrentUser().CentreID;

            return View(model);
        }

        // AJAX : Pointages/Filter
        public IActionResult Filter(PointageFilterModel filter)
        {
            if (filter.Term == null)
                filter.Term = string.Empty;

            if (!User.IsInRole("SuperAdmin"))
                filter.CentreID = GetCurrentUser().CentreID;

            var query = _context.Benevoles
                .Where(b => b.CentreID == filter.CentreID)
                .Where(b => b.Nom.ToLower().StartsWith(filter.Term.Trim().ToLower()));

            var model = query.Select(b => new BenevolePointageListItemModel
            {
                BenevoleID = b.ID,
                BenevoleNom = b.Nom,
                BenevolePrenom = b.Prenom,
                LastPointage = null, //TODO
            }).AsEnumerable();

            return View(model);
        }

        // GET: Pointages/Details/5
        [HttpGet("Pointages/Benevole/{id}")]
        public async Task<IActionResult> Benevole(int id, int? year, int? month)
        {
            const int CALENDAR_ROW_COUNT = 6;
            const int CALENDAR_DAY_COUNT = 7;

            var now = DateTime.Now;

            if (year == null)
                year = now.Year;

            if (month == null)
                month = now.Month;

            var benevole = await _context.Benevoles.Include(b => b.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if(benevole == null)
                return NotFound();

            if (!IsBenevoleAllowed(benevole))
                return Forbid();

            var model = new PointagesBenevoleModel
            {
                Benevole = benevole,
                MonthDate = new DateTime(year.Value, month.Value, 1)
            };

            var dateFrom = new DateTime(year.Value, month.Value, 1);

            int dayOfWeekStartMonday = (int)dateFrom.DayOfWeek - 1;
            if (dayOfWeekStartMonday == -1)
                dayOfWeekStartMonday = 6; // dimanche

            dateFrom = dateFrom.AddDays(0 - dayOfWeekStartMonday);

            var dateTo = dateFrom.AddDays(CALENDAR_ROW_COUNT * CALENDAR_DAY_COUNT);

            var pointages = _context.Pointages
                .Where(p => p.BenevoleID == id)
                .Where(p => p.Date >= dateFrom && p.Date < dateTo)
                .OrderBy(p => p.Date)
                .ToDictionary(p => p.Date);

            var currentDate = dateFrom;

            for (int r = 0; r < CALENDAR_ROW_COUNT; r++)
            {
                var row = new CalendarRow();

                for (int i = 0; i < CALENDAR_DAY_COUNT; i++)
                {
                    var item = new CalendarItem
                    {
                        Date = currentDate,
                        Pointage = pointages.GetValueOrDefault(currentDate),
                        IsCurrentMonth = (currentDate.Month == month && currentDate.Year == year)
                    };

                    item.Date = currentDate;
                    row.Items.Add(item);

                    currentDate = currentDate.AddDays(1);
                }

                model.CalendarRows.Add(row);
            }

            return View(model);
        }

        [HttpGet("Pointages/Benevole/{id}/editcreate")]
        public async Task<IActionResult> BenevoleEditOrCreate(int id, DateTime date)
        {
            var benevole = await _context.Benevoles.Include(b => b.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound();

            if (!IsBenevoleAllowed(benevole))
                return Forbid();

            var pointage = await _context.Pointages
                .SingleOrDefaultAsync(p => p.BenevoleID == id && p.Date == date);

            if (pointage == null)
            {
                pointage = new dal.models.Pointage
                {
                    BenevoleID = id,
                    Date = date,
                    NbDemiJournees = 0,
                    Distance = 0,
                };
            }

            return PartialView(pointage);
        }

        [HttpPost("Pointages/Benevole/{id}/editcreate")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> BenevoleEditOrCreate(int id, [FromBody] [Bind("BenevoleID,Date,NbDemiJournees,Distance")] Pointage pointage)
        {
            if (id != pointage.BenevoleID)
                return BadRequest("id does not match BenevoleId");

            int? existingId = _context.Pointages
                .Where(p => p.BenevoleID == pointage.BenevoleID && p.Date == pointage.Date.Date)
                .Select(p => (int?)p.ID)
                .SingleOrDefault();

            if (existingId != null)
                pointage.ID = existingId.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    if (existingId != null)
                        _context.Update(pointage);
                    else
                        _context.Pointages.Add(pointage);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PointageExists(pointage.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok();
            }

            return View(pointage);
        }

        // GET: Pointages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pointage = await _context.Pointages
                .SingleOrDefaultAsync(m => m.ID == id);
            if (pointage == null)
            {
                return NotFound();
            }

            return View(pointage);
        }

        // GET: Pointages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pointages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,NbDemiJournees,Distance,BenevoleID")] Pointage pointage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pointage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pointage);
        }

        // GET: Pointages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pointage = await _context.Pointages.SingleOrDefaultAsync(m => m.ID == id);
            if (pointage == null)
            {
                return NotFound();
            }
            return View(pointage);
        }

        // POST: Pointages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,NbDemiJournees,Distance")] Pointage pointage)
        {
            if (id != pointage.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pointage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PointageExists(pointage.ID))
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
            return View(pointage);
        }

        // GET: Pointages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pointage = await _context.Pointages
                .SingleOrDefaultAsync(m => m.ID == id);
            if (pointage == null)
            {
                return NotFound();
            }

            return View(pointage);
        }

        // POST: Pointages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pointage = await _context.Pointages.SingleOrDefaultAsync(m => m.ID == id);
            _context.Pointages.Remove(pointage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PointageExists(int id)
        {
            return _context.Pointages.Any(e => e.ID == id);
        }
    }
}
