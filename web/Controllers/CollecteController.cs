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
            var model = new CollecteFilterModel
            {
                Centres = _context.Centres.ToList(),
                EnseigneDetails = new List<EnseigneDetail>()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Filter(int CentreID, int EnseigneDetailID, DateTime? DateDebut, DateTime? DateFin)
        {
            var query = _context.Collecte.AsQueryable();

            if (CentreID > 0)
                query = query.Where(b => b.EnseigneDetail.CentreID == CentreID);

            if (EnseigneDetailID > 0)
                query = query.Where(b => b.EnseigneDetail.ID == EnseigneDetailID);

            if (DateDebut.HasValue)
                query = query.Where(b => b.DateCreation >= DateDebut.Value);

            if (DateFin.HasValue)
                query = query.Where(b => b.DateCreation <= DateFin.Value);

            var results = query
                            .Include(b => b.EnseigneDetail).ThenInclude(b => b.Enseigne)
                            .Include(b => b.Utilisateur)
                            .OrderByDescending(b => b.DateCreation)
                            .Take(100)
                            .ToList();



            return PartialView("_CollecteTable", results);
        }


        public IActionResult Create()
        {

            SetViewBagCentres();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BonLivraison bonLivraison)
        {
            if (ModelState.IsValid)
            {
                // Get the last NumBulletin for the selected CentreID
                var lastBulletin = _context.BonLivraison
                    .Where(b => b.CentreID == bonLivraison.CentreID)
                    .OrderByDescending(b => b.ID)
                    .Select(b => b.NumBulletin)
                    .FirstOrDefault();

                int nextSuffix = 1;

                if (!string.IsNullOrEmpty(lastBulletin))
                {
                    // Assuming format is "BL-<CentreID>-<suffix>"
                    var parts = lastBulletin.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int lastSuffix))
                    {
                        nextSuffix = lastSuffix + 1;
                    }
                }

                bonLivraison.NumBulletin = $"BL-{bonLivraison.CentreID}-{nextSuffix}";
                bonLivraison.DateCreation = DateTime.Now;
                bonLivraison.UtilisateurID = GetCurrentUserId();

                _context.BonLivraison.Add(bonLivraison);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Centres = _context.Centres.ToList();
            return View(bonLivraison);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var bon = _context.BonLivraison
                .Include(b => b.Centre)
                .Include(b => b.Utilisateur)
                .FirstOrDefault(b => b.ID == id);

            if (bon == null)
                return NotFound();

            ViewBag.Centres = _context.Centres.ToList();
            return View(bon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BonLivraison bonLivraison)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.BonLivraison
                    .FirstOrDefault(b => b.ID == bonLivraison.ID);

                if (existing == null)
                    return NotFound();

                // Update only editable fields
                existing.CentreID = bonLivraison.CentreID;
                existing.DateLivraison = bonLivraison.DateLivraison;
                existing.Poids = bonLivraison.Poids;
                existing.NumBulletin = bonLivraison.NumBulletin;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Centres = _context.Centres.ToList();
            return View(bonLivraison);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var bon = _context.BonLivraison
                .Include(b => b.Centre)
                .Include(b => b.Utilisateur)
                .FirstOrDefault(b => b.ID == id);

            if (bon == null)
                return NotFound();

            return View(bon); // This view should ask for confirmation
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var bon = _context.BonLivraison.Find(id);
            if (bon == null)
                return NotFound();

            _context.BonLivraison.Remove(bon);
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
        public IActionResult ExportExcel(int CentreID, string NumBulletin, DateTime? DateDebut, DateTime? DateFin)
        {
            var query = _context.BonLivraison
                .Include(b => b.Centre)
                .Include(b => b.Utilisateur)
                .AsQueryable();

            if (CentreID > 0)
                query = query.Where(b => b.CentreID == CentreID);

            if (!string.IsNullOrEmpty(NumBulletin))
                query = query.Where(b => b.NumBulletin.Contains(NumBulletin));

            if (DateDebut.HasValue)
                query = query.Where(b => b.DateLivraison >= DateDebut.Value);

            if (DateFin.HasValue)
                query = query.Where(b => b.DateLivraison <= DateFin.Value);

            var list = query.ToList();

            using (var workbook = new XLWorkbook())
            {
                // Worksheet 1: Raw data
                var worksheet = workbook.Worksheets.Add("Données Bons de Livraison");
                worksheet.Cell(1, 1).Value = "Numéro Bulletin";
                worksheet.Cell(1, 2).Value = "Centre";
                worksheet.Cell(1, 3).Value = "Date";
                worksheet.Cell(1, 4).Value = "Poids";
                worksheet.Cell(1, 5).Value = "Crée le";
                worksheet.Cell(1, 6).Value = "Crée Par";

                int row = 2;
                foreach (var item in list)
                {
                    worksheet.Cell(row, 1).Value = item.NumBulletin;
                    worksheet.Cell(row, 2).Value = item.Centre?.Nom;
                    worksheet.Cell(row, 3).Value = item.DateLivraison.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 4).Value = item.Poids;
                    worksheet.Cell(row, 5).Value = item.DateCreation.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 6).Value = item.Utilisateur.Login;
                    row++;
                }

                // Helper function to group and summarize
                var groupedStats = list
                    .GroupBy(b => b.Centre?.Nom)
                    .Select(g => new
                    {
                        Centre = g.Key,
                        Count = g.Count(),
                        TotalPoids = g.Sum(b => b.Poids)
                    }).ToList();

                DateTime today = DateTime.Today;

                // Worksheet 2: Last 7 days
                var last7Days = list.Where(b => b.DateLivraison >= today.AddDays(-7)).ToList();
                var stats7Days = last7Days
                    .GroupBy(b => b.Centre?.Nom)
                    .Select(g => new { Centre = g.Key, Count = g.Count(), TotalPoids = g.Sum(b => b.Poids) })
                    .ToList();

                var worksheet2 = workbook.Worksheets.Add($"Stats Hebdo ({last7Days.Count} livr.)");
                worksheet2.Cell(1, 1).Value = "Centre";
                worksheet2.Cell(1, 2).Value = "Nombre de livraisons";
                worksheet2.Cell(1, 3).Value = "Poids total";

                int r2 = 2;
                foreach (var stat in stats7Days)
                {
                    worksheet2.Cell(r2, 1).Value = stat.Centre;
                    worksheet2.Cell(r2, 2).Value = stat.Count;
                    worksheet2.Cell(r2, 3).Value = stat.TotalPoids;
                    r2++;
                }

                // Worksheet 3: Current year
                var currentYear = today.Year;
                var yearList = list.Where(b => b.DateLivraison.Year == currentYear).ToList();
                var statsYear = yearList
                    .GroupBy(b => b.Centre?.Nom)
                    .Select(g => new { Centre = g.Key, Count = g.Count(), TotalPoids = g.Sum(b => b.Poids) })
                    .ToList();

                var worksheet3 = workbook.Worksheets.Add($"Stats Annuelles ({yearList.Count} livr.)");
                worksheet3.Cell(1, 1).Value = "Centre";
                worksheet3.Cell(1, 2).Value = "Nombre de livraisons";
                worksheet3.Cell(1, 3).Value = "Poids total";

                int r3 = 2;
                foreach (var stat in statsYear)
                {
                    worksheet3.Cell(r3, 1).Value = stat.Centre;
                    worksheet3.Cell(r3, 2).Value = stat.Count;
                    worksheet3.Cell(r3, 3).Value = stat.TotalPoids;
                    r3++;
                }

                // Worksheet 4: Custom period
                if (DateDebut.HasValue || DateFin.HasValue)
                {
                    var periodList = list.Where(b =>
                        (!DateDebut.HasValue || b.DateLivraison >= DateDebut.Value) &&
                        (!DateFin.HasValue || b.DateLivraison <= DateFin.Value)).ToList();

                    var statsPeriod = periodList
                        .GroupBy(b => b.Centre?.Nom)
                        .Select(g => new { Centre = g.Key, Count = g.Count(), TotalPoids = g.Sum(b => b.Poids) })
                        .ToList();

                    var worksheet4 = workbook.Worksheets.Add($"Stats Periode ({periodList.Count} livr.)");
                    worksheet4.Cell(1, 1).Value = "Centre";
                    worksheet4.Cell(1, 2).Value = "Nombre de livraisons";
                    worksheet4.Cell(1, 3).Value = "Poids total";

                    int r4 = 2;
                    foreach (var stat in statsPeriod)
                    {
                        worksheet4.Cell(r4, 1).Value = stat.Centre;
                        worksheet4.Cell(r4, 2).Value = stat.Count;
                        worksheet4.Cell(r4, 3).Value = stat.TotalPoids;
                        r4++;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "BonsLivraison.xlsx");
                }
            }
        }


        string GetSafeSheetName(string baseName, int count)
        {
            string name = $"{baseName} ({count} livraisons)";
            return name.Length > 31 ? name.Substring(0, 31) : name;
        }



        [HttpGet]
        public IActionResult createPDF(int id)
        {
            var bon = _context.BonLivraison
                .Where(b => b.ID == id)
                .Select(b => new
                {
                    b.NumBulletin,
                    b.DateLivraison,
                    b.Poids,
                    b.DateCreation,
                    CentreId = b.CentreID,
                    CentreNom = b.Centre.Nom,
                    CentreRue = b.Centre.Rue,
                    CentreCodePostal = b.Centre.CodePostal,
                    CentreCommune = b.Centre.Commune,
                    CentreTelephone = b.Centre.Telephone,
                    CentreEmail = b.Centre.EMail

                })
                .FirstOrDefault();

            if (bon == null)
                return NotFound();

            using (var stream = new MemoryStream())
            {
                float margin = 28.35f;
                var doc = new Document(PageSize.A4, margin, margin, margin, margin);
                var writer = PdfWriter.GetInstance(doc, stream);
                doc.Open();

                var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
                var small = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
                var smallBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.BLACK);

                var cb = writer.DirectContent;

                // Draw black border for the whole form
                cb.SetLineWidth(2f);
                cb.SetColorStroke(BaseColor.BLACK);
                cb.Rectangle(margin / 2, margin / 2, doc.PageSize.Width - margin, doc.PageSize.Height - margin);
                cb.Stroke();

                // HEADER: Indice centre (left), BON DE LIVRAISON N° (right, above Date)
                float pageWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
                float indiceWidth = pageWidth * 0.18f;
                float spacerWidth = pageWidth * 0.02f;
                float rightBlockWidth = pageWidth - indiceWidth - spacerWidth;

                PdfPTable headerTable = new PdfPTable(new float[] { indiceWidth, spacerWidth, rightBlockWidth });
                headerTable.WidthPercentage = 100;

                // Indice centre (very left, no border)
                PdfPTable indiceTable = new PdfPTable(2);
                indiceTable.WidthPercentage = 100;
                indiceTable.SetWidths(new float[] { 0.6f, 0.4f });
                PdfPCell indiceLabel = new PdfPCell(new Phrase("Indice centre", bold));
                indiceLabel.Border = Rectangle.NO_BORDER;
                indiceLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
                indiceLabel.VerticalAlignment = Element.ALIGN_MIDDLE;
                indiceLabel.PaddingRight = 2f;
                indiceTable.AddCell(indiceLabel);
                PdfPCell indiceVal = new PdfPCell(new Phrase(bon.CentreId.ToString(), bold));
                indiceVal.BackgroundColor = new BaseColor(173, 216, 230);
                indiceVal.HorizontalAlignment = Element.ALIGN_CENTER;
                indiceVal.VerticalAlignment = Element.ALIGN_MIDDLE;
                indiceVal.Border = Rectangle.NO_BORDER;
                indiceVal.PaddingLeft = 6f;
                indiceVal.PaddingRight = 6f;
                indiceTable.AddCell(indiceVal);
                PdfPCell indiceCell = new PdfPCell(indiceTable);
                indiceCell.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(indiceCell);

                // Spacer
                PdfPCell spacerCell = new PdfPCell(new Phrase(""));
                spacerCell.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(spacerCell);

                // Right block: BON DE LIVRAISON N° (top right), NumBulletin (blue), Date (right, below), Date rectangle (blue)
                PdfPTable rightBlock = new PdfPTable(2);
                rightBlock.WidthPercentage = 100;
                rightBlock.SetWidths(new float[] { 0.6f, 0.4f });

                // Top row: BON DE LIVRAISON N° (right), NumBulletin (blue rectangle)
                PdfPCell titleCell = new PdfPCell(new Phrase("BON DE LIVRAISON N°", titleFont));
                titleCell.Border = Rectangle.NO_BORDER;
                titleCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                titleCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                titleCell.PaddingRight = 8f;
                rightBlock.AddCell(titleCell);

                PdfPCell numBulletinCell = new PdfPCell(new Phrase(bon.NumBulletin, bold));
                numBulletinCell.BackgroundColor = new BaseColor(173, 216, 230);
                numBulletinCell.HorizontalAlignment = Element.ALIGN_CENTER;
                numBulletinCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                numBulletinCell.Border = Rectangle.NO_BORDER;
                numBulletinCell.Padding = 6f;
                rightBlock.AddCell(numBulletinCell);

                // Spacer row between BON DE LIVRAISON N° and Date
                PdfPCell spacer1 = new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.NO_BORDER,
                    FixedHeight = 6f, // Adjust height as needed
                    Colspan = 2
                };
                rightBlock.AddCell(spacer1);

                // Bottom row: Date label (right), Date value (blue rectangle, aligned with NumBulletin)
                PdfPCell dateLabel = new PdfPCell(new Phrase("Date", bold));
                dateLabel.Border = Rectangle.NO_BORDER;
                dateLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
                dateLabel.VerticalAlignment = Element.ALIGN_MIDDLE;
                dateLabel.PaddingRight = 8f;
                rightBlock.AddCell(dateLabel);

                PdfPCell dateVal = new PdfPCell(new Phrase(bon.DateLivraison.ToString("dd/MM/yyyy"), bold));
                dateVal.BackgroundColor = new BaseColor(173, 216, 230);
                dateVal.HorizontalAlignment = Element.ALIGN_CENTER;
                dateVal.VerticalAlignment = Element.ALIGN_MIDDLE;
                dateVal.Border = Rectangle.NO_BORDER;
                dateVal.PaddingLeft = 6f;
                dateVal.PaddingRight = 6f;
                rightBlock.AddCell(dateVal);

                PdfPCell rightBlockCell = new PdfPCell(rightBlock);
                rightBlockCell.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(rightBlockCell);

                doc.Add(headerTable);

                // Add more space between header and addresses
                doc.Add(new Paragraph(" "));

                // Addresses
                PdfPTable expDestTable = new PdfPTable(2);
                expDestTable.WidthPercentage = 100;
                expDestTable.SetWidths(new float[] { 1.2f, 1f });

                PdfPCell expCell = new PdfPCell(new Phrase("Expéditeur :\nLes Restos du Cœur du Haut-Rhin AD 68\n7 RUE DES PERDRIX\n68110 ILLZACH\nTéléphone: 03 89 53 85 45", normal));
                expCell.Border = Rectangle.NO_BORDER;
                expCell.Padding = 8f;
                expDestTable.AddCell(expCell);

                PdfPCell destCell = new PdfPCell(new Phrase($"Destinataire :\nCentre de {bon.CentreNom}\n{bon.CentreRue}\n{bon.CentreCodePostal} {bon.CentreCommune}\nTéléphone: {bon.CentreTelephone}\nE-mail: {bon.CentreEmail}", normal));

                destCell.Border = Rectangle.BOX;
                destCell.BorderColor = BaseColor.BLACK;
                destCell.Padding = 8f;
                destCell.CellEvent = new RoundedBorder();
                expDestTable.AddCell(destCell);

                doc.Add(expDestTable);

                // Add extra space between addresses and main table
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph(" "));

                // Main table: 5 columns, reduced height (3 rows)
                float[] colWidths = { 1f, 2.5f, 0.7f, 1.2f, 1.5f };
                PdfPTable table = new PdfPTable(colWidths);
                table.WidthPercentage = 100;

                string[] headers = { "Référence", "Désignation", "", "Quantité livrée (Kg)", "Observations" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = new PdfPCell(new Phrase(headers[i], bold));
                    cell.BackgroundColor = new BaseColor(220, 230, 241);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 6f;
                    cell.Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;
                    cell.BorderColor = BaseColor.BLACK;
                    cell.BorderWidthBottom = 1.5f;
                    table.AddCell(cell);
                }

                // Add 4 lines with Values:
                string[] descriptions = { "Protidiques", "Accompagnements", "Laitiers", "Desserts" };
                BaseColor yellow = new BaseColor(255, 255, 153); // light yellow
                foreach (var desc in descriptions)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        Phrase phrase = col == 1 ? new Phrase(desc, normal) : new Phrase("", normal);
                        PdfPCell cell = new PdfPCell(phrase)
                        {
                            Padding = 8f,
                            BorderColor = BaseColor.BLACK,
                            BorderWidthBottom = 0.5f,
                            Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER
                        };

                        // Apply yellow background to "Quantité livrée" (index 3) and "Observations" (index 4)
                        if (col == 3 || col == 4)
                        {
                            cell.BackgroundColor = yellow;
                        }

                        // Apply dotted bottom border
                        cell.CellEvent = new DottedBorderCellEvent();

                        table.AddCell(cell);
                    }
                }



                // Add empty rows to reach a reasonable height (3 rows)
                int rowCount = 3;
                float pageHeight = doc.PageSize.Height - doc.TopMargin - doc.BottomMargin;
                float targetTableHeight = (pageHeight * 0.35f); // further reduced height
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        var cell = new PdfPCell(new Phrase("", normal)) { Padding = 12f, FixedHeight = targetTableHeight / rowCount };
                        cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                        cell.BorderColor = BaseColor.BLACK;
                        table.AddCell(cell);
                    }
                }
                // Add a bottom border to the table
                PdfPCell bottomBorderCell = new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColorBottom = BaseColor.BLACK,
                    BorderWidthBottom = 2f,
                    Colspan = 5,
                    FixedHeight = 2f
                };
                table.AddCell(bottomBorderCell);

                table.TotalWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
                table.LockedWidth = true;

                doc.Add(table);

                // Add instruction paragraph below the table and above signature                
                var instructionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK); // Increased font size
                Paragraph instr = new Paragraph("MERCI DE PROCEDER AU TRI ET AU PESAGE DES PRODUITS FIGURANT SUR CE BL\n ET D'ENREGISTRER LEURS POIDS DANS LES RAMASSES AAIDA", instructionFont);
                instr.Alignment = Element.ALIGN_CENTER;
                instr.SpacingAfter = 15f;
                doc.Add(instr);

                // Signature and Colisage section
                PdfPTable sigColisTable = new PdfPTable(2);
                sigColisTable.WidthPercentage = 100;
                sigColisTable.SetWidths(new float[] { 2f, 3f });

                // Left: Signature and instructions
                PdfPTable leftSig = new PdfPTable(1);
                leftSig.WidthPercentage = 100;
                PdfPCell sigCell = new PdfPCell(new Phrase("Signature et cachet expéditeur", bold));
                sigCell.Border = Rectangle.NO_BORDER;
                sigCell.PaddingTop = 5f;
                sigCell.PaddingBottom = 40f;
                leftSig.AddCell(sigCell);
                sigColisTable.AddCell(new PdfPCell(leftSig) { Border = Rectangle.NO_BORDER });

                // Right: Colisage
                PdfPTable colisageBlock = new PdfPTable(1);
                colisageBlock.WidthPercentage = 100;
                PdfPCell colisageLabel = new PdfPCell(new Phrase("Colisage:", bold));
                colisageLabel.Border = Rectangle.NO_BORDER;
                colisageLabel.PaddingBottom = 4f;
                colisageBlock.AddCell(colisageLabel);

                // Colisage table: 3 columns, aligned with last 3 columns of main table, with full borders
                float[] colisColWidths = { 0.7f, 1.2f, 1.5f };
                PdfPTable colisTable = new PdfPTable(colisColWidths);

                float totalColisWidth = table.TotalWidth * (0.7f + 1.2f + 1.5f) / (1f + 2.5f + 0.7f + 1.2f + 1.5f);
                colisTable.TotalWidth = totalColisWidth;
                colisTable.LockedWidth = true;

                colisTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                // Headers
                PdfPCell nbColisHeader = new PdfPCell(new Phrase("Nb colis", bold)) { Padding = 5f, BackgroundColor = new BaseColor(220, 230, 241), HorizontalAlignment = Element.ALIGN_CENTER };
                nbColisHeader.Border = Rectangle.BOX;
                nbColisHeader.BorderColor = BaseColor.BLACK;
                colisTable.AddCell(nbColisHeader);
                PdfPCell poidsHeader = new PdfPCell(new Phrase("Poids (Kg)", bold)) { Padding = 5f, BackgroundColor = new BaseColor(220, 230, 241), HorizontalAlignment = Element.ALIGN_CENTER };
                poidsHeader.Border = Rectangle.BOX;
                poidsHeader.BorderColor = BaseColor.BLACK;
                colisTable.AddCell(poidsHeader);
                PdfPCell emptyHeader = new PdfPCell(new Phrase("", bold)) { Padding = 5f, BackgroundColor = new BaseColor(220, 230, 241) };
                emptyHeader.Border = Rectangle.BOX;
                emptyHeader.BorderColor = BaseColor.BLACK;
                colisTable.AddCell(emptyHeader);
                // Data row
                PdfPCell nbColisCell = new PdfPCell(new Phrase("", normal)) { Padding = 5f };
                nbColisCell.Border = Rectangle.BOX;
                nbColisCell.BorderColor = BaseColor.BLACK;
                nbColisCell.BackgroundColor = yellow;
                colisTable.AddCell(nbColisCell);
                PdfPCell poidsCell = new PdfPCell(new Phrase($"{bon.Poids}", normal)) { Padding = 5f };
                poidsCell.Border = Rectangle.BOX;
                poidsCell.BorderColor = BaseColor.BLACK;
                poidsCell.BackgroundColor = yellow;
                poidsCell.HorizontalAlignment = Element.ALIGN_CENTER;
                colisTable.AddCell(poidsCell);
                PdfPCell emptyCell = new PdfPCell(new Phrase("", normal)) { Padding = 5f };
                emptyCell.Border = Rectangle.BOX;
                emptyCell.BorderColor = BaseColor.BLACK;
                colisTable.AddCell(emptyCell);
                colisageBlock.AddCell(new PdfPCell(colisTable) { Border = Rectangle.NO_BORDER });
                sigColisTable.AddCell(new PdfPCell(colisageBlock) { Border = Rectangle.NO_BORDER });

                doc.Add(sigColisTable);

                doc.Close();

                var content = stream.ToArray();
                return File(content, "application/pdf", $"BonLivraison_{bon.NumBulletin}.pdf");
            }
        }


    }
}



