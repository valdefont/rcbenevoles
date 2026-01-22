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

                EnseigneDetails = _context.EnseigneDetail
                    .Include(e => e.Enseigne)
                    .Include(e => e.CodeCommune)
                    .OrderBy(e => e.Enseigne.Name)
                    .ThenBy(e => e.CodeCommune.NomCommune)
                    .ToList()
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
                            .Include(b => b.EnseigneDetail)
                                .ThenInclude(e => e.Enseigne)
                            .Include(b => b.EnseigneDetail)
                                .ThenInclude(e => e.CodeCommune)
                            .Include(b => b.EnseigneDetail)
                                .ThenInclude(e => e.Centre)      //  <-- THIS WAS MISSING
                            .Include(b => b.Utilisateur)
                            .OrderByDescending(b => b.DateCreation)
                            .Take(100)
                            .ToList();


            return PartialView("_CollecteTable", results);
        }



        // GET: Collecte/Create
        public IActionResult Create()
        {
            var model = new Collecte();

            ViewBag.EnseigneDetailItems = _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .OrderBy(e => e.Enseigne.Name)
                .ThenBy(e => e.CodeCommune.NomCommune)
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.ID.ToString(),
                    Text = $"{e.Enseigne.Name} - {e.CodeCommune.NomCommune}"
                })
                .ToList();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Collecte collecte)
        {
            // We will set this manually → don't validate it
            ModelState.Remove("UtilisateurID");

            if (!ModelState.IsValid)
            {
                ViewBag.EnseigneDetailItems = _context.EnseigneDetail
                    .Include(e => e.Enseigne)
                    .Include(e => e.CodeCommune)
                    .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = e.ID.ToString(),
                        Text = $"{e.Enseigne.Name} - {e.CodeCommune.NomCommune}"
                    })
                    .ToList();

                return View(collecte);
            }

            collecte.UtilisateurID = GetCurrentUserId();
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

            ViewBag.EnseigneDetailItems = _context.EnseigneDetail
                .Include(e => e.Enseigne)
                .Include(e => e.CodeCommune)
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.ID.ToString(),
                    Text = $"{e.Enseigne.Name} - {e.CodeCommune.NomCommune}"
                })
                .ToList();

            return View(collecte);
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Collecte collecte)
        {
            if (id != collecte.ID)
                return BadRequest();

            // Ignore fields user does NOT edit
            ModelState.Remove("UtilisateurID");
            ModelState.Remove("DateCreation");

            if (!ModelState.IsValid)
            {
                ViewBag.EnseigneDetailItems = _context.EnseigneDetail
                    .Include(e => e.Enseigne)
                    .Include(e => e.CodeCommune)
                    .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = e.ID.ToString(),
                        Text = $"{e.Enseigne.Name} - {e.CodeCommune.NomCommune}"
                    })
                    .ToList();

                return View(collecte);
            }

            var dbCollecte = _context.Collecte.Find(id);
            if (dbCollecte == null)
                return NotFound();

            // Update only editable fields
            dbCollecte.EnseigneDetailID = collecte.EnseigneDetailID;
            dbCollecte.Poids = collecte.Poids;

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




    }
}



