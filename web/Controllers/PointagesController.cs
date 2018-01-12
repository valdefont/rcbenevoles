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
using Microsoft.AspNetCore.Authorization;

namespace web.Controllers
{
    [Authorize]
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

            var query = _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .Where(b => b.Adresses.Any(a => a.CentreID == filter.CentreID))
                .Where(b => b.Nom.ToLower().StartsWith(filter.Term.Trim().ToLower()));

            var model = query.Select(b => new BenevolePointageListItemModel
            {
                BenevoleID = b.ID,
                BenevoleNom = b.Nom,
                BenevolePrenom = b.Prenom,
                ShowAddressWarning = b.Adresses.Any(a => a.CentreID == filter.CentreID && a.IsCurrent)
            }).AsEnumerable();

            return View(model);
        }

        // GET: Pointages/Details/5
        [HttpGet("Pointages/Benevole/{id}")]
        public async Task<IActionResult> Benevole(int id, int? year, int? month, bool partial = false)
        {
            var now = DateTime.Now;

            if (year == null)
                year = now.Year;

            if (month == null)
                month = now.Month;

            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if(benevole == null)
                return NotFound();

            var centreGere = GetCurrentUser().Centre;

            if (centreGere != null && !benevole.Adresses.Any(a => a.CentreID == centreGere.ID))
                return Forbid();

            var model = new PointagesBenevoleModel
            {
                Centre = centreGere,
                Benevole = benevole,
                MonthDate = new DateTime(year.Value, month.Value, 1)
            };

            var dateFrom = new DateTime(year.Value, month.Value, 1);

            int dayOfWeekStartMonday = (int)dateFrom.DayOfWeek - 1;
            if (dayOfWeekStartMonday == -1)
                dayOfWeekStartMonday = 6; // dimanche

            dateFrom = dateFrom.AddDays(0 - dayOfWeekStartMonday);

            var dateTo = dateFrom.AddDays(PointagesBenevoleModel.CALENDAR_ROW_COUNT * PointagesBenevoleModel.CALENDAR_DAY_COUNT);

            var pointages = _context.Pointages
                .Where(p => p.BenevoleID == id)
                .Where(p => p.Date >= dateFrom && p.Date < dateTo)
                .OrderBy(p => p.Date)
                .ToDictionary(p => p.Date);

            var currentDate = dateFrom;

            var adresses = benevole.Adresses
                .Where(a => a.DateChangement < dateTo)
                .OrderBy(a => a.DateChangement)
                .ToList();

            int addressIndex = 0;

            DateTime? nextChangeAddressDate = null;

            if(addressIndex + 1 < adresses.Count)
                nextChangeAddressDate = adresses[addressIndex + 1].DateChangement;

            for (int r = 0; r < PointagesBenevoleModel.CALENDAR_ROW_COUNT; r++)
            {
                var row = new CalendarRow();

                for (int i = 0; i < PointagesBenevoleModel.CALENDAR_DAY_COUNT; i++)
                {
                    while (nextChangeAddressDate != null && currentDate.Date >= nextChangeAddressDate)
                    {
                        addressIndex++;

                        if (addressIndex + 1 < adresses.Count)
                            nextChangeAddressDate = adresses[addressIndex + 1].DateChangement;
                        else
                            nextChangeAddressDate = null;
                    }

                    var item = new CalendarItem
                    {
                        Date = currentDate,
                        Pointage = pointages.GetValueOrDefault(currentDate),
                        IsCurrentMonth = (currentDate.Month == month && currentDate.Year == year),
                        RowIndex = r,
                        ColumnIndex = i,
                    };

                    if(centreGere != null && adresses[addressIndex].CentreID != centreGere.ID)
                        item.DisabledByCenter = true;

                    row.Items.Add(item);

                    currentDate = currentDate.AddDays(1);
                }

                model.CalendarRows.Add(row);
            }

            string viewName = "Benevole";

            if (partial)
                viewName = "_BenevoleCalendarContent";

            return View(viewName, model);
        }

        [HttpGet("Pointages/Benevole/{id}/editcreate")]
        public async Task<IActionResult> BenevoleEditOrCreate(int id, DateTime date)
        {
            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound();

            var centreId = benevole.GetAdresseFromDate(date).CentreID;
            var userCentreId = GetCurrentUser().CentreID;

            var pointage = await _context.Pointages
                .SingleOrDefaultAsync(p => p.BenevoleID == id && p.Date == date);

            ViewBag.DisabledForCenter = (userCentreId != null && centreId != userCentreId);

            if (pointage == null)
            {
                pointage = new dal.models.Pointage
                {
                    BenevoleID = id,
                    CentreID = centreId,
                    Date = date,
                    NbDemiJournees = 1,
                };
            }

            return PartialView(pointage);
        }

        [HttpPost("Pointages/Benevole/{id}/editcreate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BenevoleEditOrCreate(int id, [FromBody] [Bind("BenevoleID,Date,NbDemiJournees")] Pointage pointage)
        {
            if (id != pointage.BenevoleID)
                return BadRequest("id does not match BenevoleId");

            var existing = _context.Pointages
                .Where(p => p.BenevoleID == pointage.BenevoleID && p.Date == pointage.Date.Date)
                .SingleOrDefault();

            if (existing != null)
                pointage.ID = existing.ID;

            var benevole = _context.Benevoles.Include(b => b.Adresses).SingleOrDefault(b => b.ID == pointage.BenevoleID);
            var centreId = benevole.GetAdresseFromDate(pointage.Date).CentreID;

            pointage.CentreID = centreId;

            if (existing != null && existing.CentreID != pointage.CentreID)
                return BadRequest("centre id does not match with existing pointage");

            var userCentreId = GetCurrentUser().CentreID;
            if (userCentreId != null && centreId != userCentreId)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    if (existing != null)
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

                return Json(new { id = pointage.ID });
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


        [HttpGet("Pointages/Benevole/{id}/printindex")]
        public async Task<IActionResult> PrintIndex(int id)
        {
            var benevole = await _context.Benevoles
                .Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .Include(b => b.Pointages)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound("Bénévole non trouvé");

            var model = new PrintIndexModel
            {
                Benevole = benevole,
            };

            // début : début de période à partir du premier pointage du bénévole
            int periodId;
            var start = benevole.Pointages.OrderBy(p => p.Date).Select(p => p.Date).FirstOrDefault(); 

            if(start == DateTime.MinValue)
                return View(model);
            else
            {
                if(start.Month >= 5)
                {
                    start = new DateTime(start.Year, 5, start.Day);
                    periodId = 2;
                }
                else
                {
                    start = new DateTime(start.Year, 1, start.Day);
                    periodId = 1;
                }
            }

            // fin : fin de période à partir du dernier pointage du bénévole
            var end = benevole.Pointages.OrderByDescending(p => p.Date).Select(p => p.Date).FirstOrDefault();

            if(end.Month >= 5)
                end = new DateTime(end.Year + 1, 1, 1);
            else
                end = new DateTime(end.Year, 5, 1);


            // calcul des périodes
            DateTime periodStart;
            DateTime periodEnd;

            PrintIndexPeriod latestPeriod = null;

            for(int year = start.Year; year <= end.AddDays(-1).Year; year++)
            {
                for(int p = periodId; p <= 2; p++)
                {
                    switch(periodId)
                    {
                        case 1:
                            {
                                periodStart = new DateTime(year, 1, 1);
                                periodEnd = new DateTime(year, 5, 1);
                            }
                            break;
                        case 2:
                            {
                                periodStart = new DateTime(year, 5, 1);
                                periodEnd = new DateTime(year + 1, 1, 1);
                            }
                            break;
                        default:
                            return BadRequest("Période invalide");
                    }

                    if(periodStart > end)
                        break;

                    var adressesWithDates = benevole.GetAdressesInPeriod(periodStart, periodEnd);

                    foreach(var adrDate in adressesWithDates.Keys.OrderBy(k => k))
                    {
                        if(latestPeriod != null)
                            latestPeriod.End = adrDate.AddDays(-1);

                        var period = new PrintIndexPeriod
                        {
                            Start = adrDate,
                            End = periodEnd,
                            Adresse = adressesWithDates[adrDate],
                        };

                        model.Periods.Add(period);

                        latestPeriod = period;
                    }
                }

                periodId = 1;
            }

            if(latestPeriod != null)
                latestPeriod.End = end.AddDays(-1);

            return View(model);
        }

        [HttpGet("Pointages/Benevole/{id}/print")]
        public async Task<IActionResult> Print(int id, int addressId, int period, int year)
        {
            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound("Bénévole non trouvé");

            if (!IsBenevoleAllowed(benevole))
                return Forbid();

            var adresse = benevole.Adresses
                .SingleOrDefault(a => a.ID == addressId);

            if(adresse == null)
                return NotFound("Adresse non trouvée");

            if (!IsCentreAllowed(adresse.Centre))
                return Forbid();

            DateTime periodStart;
            DateTime periodEnd;

            switch(period)
            {
                case 1:
                    {
                        periodStart = new DateTime(year, 1, 1);
                        periodEnd = new DateTime(year, 5, 1);
                    }
                    break;
                case 2:
                    {
                        periodStart = new DateTime(year, 5, 1);
                        periodEnd = new DateTime(year + 1, 1, 1);
                    }
                    break;
                default:
                    return BadRequest("Période invalide");
            }

            var frais = _context.Frais.SingleOrDefault(f => f.Annee == year);

            if(frais == null)
                return NotFound("Frais non trouvé");

            var start = periodStart;
            var end = periodEnd;

            if(adresse.DateChangement > start)
                start = adresse.DateChangement;

            var totalDemiJournees = _context.Pointages
                .Where(p => p.BenevoleID == adresse.BenevoleID && p.CentreID == adresse.CentreID)
                .Where(p => p.Date >= start && p.Date < end)
                .Sum(p => p.NbDemiJournees);

            int monthCount;

            if(periodStart.Year == periodEnd.Year)
                monthCount = periodEnd.Month - periodStart.Month;
            else
                monthCount = periodEnd.Month + 12 - periodStart.Month;

            PrintModel model = new PrintModel
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd.AddDays(-1),
                Benevole = benevole,
                FraisKm = frais.TauxKilometrique,
                MonthCount = monthCount,
                TotalDistance = totalDemiJournees,
            };

            return View(model);
        }

        private bool PointageExists(int id)
        {
            return _context.Pointages.Any(e => e.ID == id);
        }
    }
}
