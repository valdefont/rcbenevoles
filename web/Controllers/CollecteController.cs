using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using dal;
using dal.models;
using System.IO;
using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using DocumentFormat.OpenXml.Spreadsheet;
using Org.BouncyCastle.Utilities;

namespace web.Controllers
{
    public class CollecteController : RCBenevoleController
    {
        public CollecteController(RCBenevoleContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();

            var enseigneDetailsQuery = _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .Where(e => e.EstActif);

            if (!User.IsInRole("SuperAdmin"))
                enseigneDetailsQuery = enseigneDetailsQuery.Where(e => allowedIds.Contains(e.ID));

            var model = new CollecteFilterModel
            {
                Centres = _context.Centres.ToList(),
                EnseigneDetails = enseigneDetailsQuery
                    .OrderBy(e => e.Enseigne.Name)
                    .ThenBy(e => e.CodeCommune.NomCommune)
                    .ToList()
            };

            return View(model);
        }



        [HttpGet]
        public IActionResult Filter(int CentreID, int EnseigneDetailID, DateTime? DateDebut, DateTime? DateFin)
        {
            var query = _context.Collecte
                .Include(b => b.EnseigneDetail).ThenInclude(e => e.Enseigne)
                .Include(b => b.EnseigneDetail).ThenInclude(e => e.CodeCommune)
                .Include(b => b.EnseigneDetail).ThenInclude(e => e.Centre)
                .Include(b => b.Utilisateur)
                .AsQueryable();

            if (!User.IsInRole("SuperAdmin"))
            {
                var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();
                query = query.Where(b => allowedIds.Contains(b.EnseigneDetail.ID));
            }

            if (CentreID > 0)
                query = query.Where(b => b.EnseigneDetail.CentreID == CentreID);

            if (EnseigneDetailID > 0)
                query = query.Where(b => b.EnseigneDetail.ID == EnseigneDetailID);

            if (DateDebut.HasValue)
                query = query.Where(b => b.DateCreation >= DateDebut.Value);

            if (DateFin.HasValue)
                query = query.Where(b => b.DateCreation <= DateFin.Value);

            var results = query
                .OrderByDescending(b => b.DateCreation)
                .Take(100)
                .ToList();

            return PartialView("_CollecteTable", results);
        }


        private IQueryable<int> GetAllowedEnseigneDetailIdsForCurrentUser()
        {
            if (User.IsInRole("SuperAdmin"))
                return _context.EnseigneDetail
                    .Where(e => e.EstActif)
                    .Select(e => e.ID);

            var idStr = User.FindFirst("UtilisateurID")?.Value;
            if (!int.TryParse(idStr, out var userId))
                return Enumerable.Empty<int>().AsQueryable();

            return _context.EnseigneDetailUtilisateurs
                .Where(x => x.UtilisateurID == userId && x.EstActif && x.EnseigneDetail.EstActif)
                .Select(x => x.EnseigneDetailID);
        }

        private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> BuildEnseigneDetailSelectItems(IQueryable<int> allowedIds)
        {
            var query = _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .Where(e => e.EstActif);

            if (!User.IsInRole("SuperAdmin"))
                query = query.Where(e => allowedIds.Contains(e.ID));

            return query
                .AsNoTracking()
                .OrderBy(e => e.Enseigne.Name)
                .ThenBy(e => e.CodeCommune.NomCommune)
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.ID.ToString(),
                    Text = $"{e.Enseigne.Name} - {e.CodeCommune.NomCommune}"
                })
                .ToList();
        }



        // GET: Collecte/Create
        public IActionResult Create()
        {
            var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();

            ViewBag.EnseigneDetailItems = BuildEnseigneDetailSelectItems(allowedIds);

            var model = new Collecte();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Collecte collecte)
        {
            // We set the user server-side
            ModelState.Remove("UtilisateurID");

            // Validate chosen EnseigneDetailID is in allowed set (unless SuperAdmin)
            if (!User.IsInRole("SuperAdmin"))
            {
                var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();
                var isAllowed = allowedIds.Any(id => id == collecte.EnseigneDetailID);
                if (!isAllowed)
                {
                    ModelState.AddModelError("EnseigneDetailID",
                        "Vous n'êtes pas autorisé à sélectionner cette enseigne.");
                }
            }

            if (!ModelState.IsValid)
            {
                var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();
                ViewBag.EnseigneDetailItems = BuildEnseigneDetailSelectItems(allowedIds);
                return View(collecte);
            }

            collecte.UtilisateurID = GetCurrentUserId(); // your existing helper
            collecte.DateCreation = DateTime.Now;

            _context.Collecte.Add(collecte);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            var collecte = _context.Collecte
                .Include(c => c.EnseigneDetail).ThenInclude(e => e.Enseigne)
                .Include(c => c.EnseigneDetail).ThenInclude(e => e.CodeCommune)
                .FirstOrDefault(c => c.ID == id);

            if (collecte == null)
                return NotFound();

            if (!User.IsInRole("SuperAdmin"))
            {
                var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();
                // If the collecte’s enseigne is not allowed, forbid access
                bool canEdit = allowedIds.Any(x => x == collecte.EnseigneDetailID);
                if (!canEdit)
                    return Forbid(); // or return NotFound() if you prefer not to disclose existence
            }

            var allowedForDropdown = GetAllowedEnseigneDetailIdsForCurrentUser();
            ViewBag.EnseigneDetailItems = BuildEnseigneDetailSelectItems(allowedForDropdown);

            return View(collecte);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Collecte input)
        {
            if (id != input.ID)
                return BadRequest();

            // Load existing entity to verify original ownership & to update safely
            var collecte = _context.Collecte
                .FirstOrDefault(c => c.ID == id);

            if (collecte == null)
                return NotFound();

            if (!User.IsInRole("SuperAdmin"))
            {
                var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser();

                // Check user can see the existing collecte:
                bool canEditExisting = allowedIds.Any(x => x == collecte.EnseigneDetailID);
                if (!canEditExisting)
                    return Forbid();

                // Check the newly selected EnseigneDetailID is allowed
                bool canSelectNew = allowedIds.Any(x => x == input.EnseigneDetailID);
                if (!canSelectNew)
                    ModelState.AddModelError("EnseigneDetailID", "Vous n'êtes pas autorisé à sélectionner cette enseigne.");
            }

            if (!ModelState.IsValid)
            {
                var allowedForDropdown = GetAllowedEnseigneDetailIdsForCurrentUser();
                ViewBag.EnseigneDetailItems = BuildEnseigneDetailSelectItems(allowedForDropdown);
                return View(input);
            }

            // Update permitted fields
            collecte.EnseigneDetailID = input.EnseigneDetailID;
            collecte.Poids = input.Poids;
            // (Keep UtilisateurID/DateCreation immutable, unless you have an audit policy)

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }




        // GET: Collecte/Delete/5
        public IActionResult Delete(int id)
        {
            var collecte = _context.Collecte
                .Include(c => c.EnseigneDetail).ThenInclude(e => e.Enseigne)
                .Include(c => c.EnseigneDetail).ThenInclude(e => e.CodeCommune)
                .FirstOrDefault(c => c.ID == id);

            if (collecte == null)
                return NotFound();

            return View(collecte);
        }

        // POST: Collecte/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var collecte = _context.Collecte.Find(id);
            if (collecte == null)
                return NotFound();

            _context.Collecte.Remove(collecte);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }




        // Example method to get current user ID
        private int GetCurrentUserId()
        {

            var user = _context.Utilisateurs.Include(u => u.Centre).SingleOrDefault(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                return -1;
            }
            return user.ID;
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



        [HttpGet]
        public IActionResult ExportExcelCollecte(int CentreID, int EnseigneDetailID, DateTime? DateDebut, DateTime? DateFin)
        {

            // Base query + eager loads (still useful for the Excel content)
            var query = _context.Collecte
                .AsNoTracking()
                .Include(c => c.EnseigneDetail).ThenInclude(ed => ed.Centre)
                .Include(c => c.EnseigneDetail).ThenInclude(ed => ed.Enseigne)
                .Include(c => c.EnseigneDetail).ThenInclude(ed => ed.CodeCommune)
                .Include(c => c.Utilisateur)
                .AsQueryable();

            var allowedIds = GetAllowedEnseigneDetailIdsForCurrentUser(); // IQueryable<int>

            if (!User.IsInRole("SuperAdmin"))
            {
                var allowedIdsNullable = allowedIds.Select(x => (int?)x);
                query = query.Where(c => allowedIdsNullable.Contains(c.EnseigneDetailID));
            }

            // Existing filters
            if (CentreID > 0)
                query = query.Where(c => c.EnseigneDetail.CentreID == CentreID);

            if (EnseigneDetailID > 0)
                query = query.Where(c => c.EnseigneDetailID == EnseigneDetailID);

            if (DateDebut.HasValue)
                query = query.Where(c => c.DateCreation >= DateDebut.Value);

            if (DateFin.HasValue)
                query = query.Where(c => c.DateCreation <= DateFin.Value);

            // It’s often useful to keep a stable order in exports
            var list = query
                .OrderBy(c => c.EnseigneDetail.Centre.Nom)
                .ThenBy(c => c.EnseigneDetail.Enseigne.Name)
                .ThenBy(c => c.EnseigneDetail.CodeCommune.NomCommune)
                .ThenBy(c => c.DateCreation)
                .ToList();


            using (var workbook = new XLWorkbook())
            {
                //---------------------------------------------------------
                // Helper Styling Functions
                //---------------------------------------------------------
                void StyleHeader(IXLRange range)
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.FontColor = XLColor.Black;
                    range.Style.Fill.BackgroundColor = XLColor.FromArgb(220, 230, 241);
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                }

                void ApplyBorders(IXLRange range)
                {
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                void ApplyZebra(IXLWorksheet ws, int startRow, int endRow, int lastCol)
                {
                    for (int r = startRow; r <= endRow; r++)
                    {
                        if (r % 2 == 0)
                        {
                            ws.Range(r, 1, r, lastCol).Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);
                        }
                    }
                }

                //---------------------------------------------------------
                // 1️⃣ RAW DATA SHEET
                //---------------------------------------------------------
                var ws = workbook.Worksheets.Add("Données Collecte");

                ws.Cell(1, 1).Value = "Centre";
                ws.Cell(1, 2).Value = "Commune";
                ws.Cell(1, 3).Value = "Adresse";
                ws.Cell(1, 4).Value = "Enseigne";
                ws.Cell(1, 5).Value = "Poids (kg)";
                ws.Cell(1, 6).Value = "Créé le";
                ws.Cell(1, 7).Value = "Créé par";

                StyleHeader(ws.Range("A1:G1"));

                int row = 2;
                foreach (var c in list)
                {
                    ws.Cell(row, 1).Value = c.EnseigneDetail?.Centre?.Nom;
                    ws.Cell(row, 2).Value = c.EnseigneDetail?.CodeCommune?.NomCommune;
                    ws.Cell(row, 3).Value = c.EnseigneDetail?.Adresse;
                    ws.Cell(row, 4).Value = c.EnseigneDetail?.Enseigne?.Name;
                    ws.Cell(row, 5).Value = (double)c.Poids;
                    ws.Cell(row, 6).Value = c.DateCreation;
                    ws.Cell(row, 7).Value = c.Utilisateur?.Login;


                    row++;
                }

                int lastDataRow = row - 1;

                if (lastDataRow >= 2)
                {
                    ws.Range(2, 5, lastDataRow, 5).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range(2, 6, lastDataRow, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                }

                // Apply zebra manually
                ApplyZebra(ws, 2, lastDataRow, 7);

                // Borders
                ApplyBorders(ws.Range(1, 1, lastDataRow, 7));

                // Fit + Freeze
                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();


                //---------------------------------------------------------
                // 2️⃣ STATISTIQUES SHEET
                //---------------------------------------------------------
                var stats = list
                            .GroupBy(c => new {
                                Centre = c.EnseigneDetail?.Centre?.Nom,
                                Commune = c.EnseigneDetail?.CodeCommune?.NomCommune,
                                Adresse = c.EnseigneDetail?.Adresse,
                                Enseigne = c.EnseigneDetail?.Enseigne?.Name
                            })
                            .Select(g => new {
                                    g.Key.Centre,
                                    g.Key.Commune,
                                    g.Key.Adresse,
                                    g.Key.Enseigne,
                                    TotalCollectes = g.Count(),
                                    TotalPoids = g.Sum(x => x.Poids)
                                })
                            .OrderBy(x => x.Centre)
                            .ThenBy(x => x.Enseigne)
                            .ToList();

                var wsStats = workbook.Worksheets.Add("Statistiques");


                wsStats.Cell(1, 1).Value = "Centre";
                wsStats.Cell(1, 2).Value = "Commune";
                wsStats.Cell(1, 3).Value = "Adresse";
                wsStats.Cell(1, 4).Value = "Enseigne";
                wsStats.Cell(1, 5).Value = "Nombre collectes";
                wsStats.Cell(1, 6).Value = "Poids total (kg)";

                StyleHeader(wsStats.Range("A1:F1"));


                int sRow = 2;
                foreach (var s in stats)
                {

                    wsStats.Cell(sRow, 1).Value = s.Centre;
                    wsStats.Cell(sRow, 2).Value = s.Commune;
                    wsStats.Cell(sRow, 3).Value = s.Adresse;
                    wsStats.Cell(sRow, 4).Value = s.Enseigne;
                    wsStats.Cell(sRow, 5).Value = s.TotalCollectes;
                    wsStats.Cell(sRow, 6).Value = (double)s.TotalPoids;

                    sRow++;
                }

                int lastStatsRow = sRow - 1;

                ApplyZebra(wsStats, 2, lastStatsRow, 6);
                ApplyBorders(wsStats.Range(1, 1, lastStatsRow,6));

                // Footer
                wsStats.Cell(sRow, 1).Value = "TOTAL";
                wsStats.Cell(sRow, 1).Style.Font.Bold = true;

                wsStats.Cell(sRow, 4).Value = stats.Select(s => s.Enseigne).Distinct().Count();
                wsStats.Cell(sRow, 5).Value = stats.Sum(s => s.TotalCollectes);
                wsStats.Cell(sRow, 6).Value = (double)stats.Sum(s => s.TotalPoids);

                var footer = wsStats.Range(sRow, 1, sRow, 6);
                footer.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 242, 204);
                footer.Style.Font.Bold = true;
                footer.Style.Border.TopBorder = XLBorderStyleValues.Thick;

                wsStats.SheetView.FreezeRows(1);
                wsStats.Columns().AdjustToContents();


                //---------------------------------------------------------
                // 3️⃣ RETURN FILE
                //---------------------------------------------------------
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Collectes.xlsx"
                    );
                }
            }
        }




        // Helpers for styling
        private void StyleHeader(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.FontColor = XLColor.Black;      
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Fill.BackgroundColor = XLColor.FromArgb(220, 230, 241); // light blue
            range.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
        }

        private void StyleTableBorders(IXLRange range)
        {
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private void FitAndFreeze(IXLWorksheet ws, int lastRow, int lastCol)
        {
                  
            ws.SheetView.FreezeRows(1);                                 // Freeze header row
            ws.Columns(1, lastCol).AdjustToContents();                  // Auto-fit columns
            ws.Rows(1, lastRow).AdjustToContents();                     // Auto-fit rows
        }







    }
}



