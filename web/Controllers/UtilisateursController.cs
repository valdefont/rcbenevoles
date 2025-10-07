using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using Microsoft.AspNetCore.Authorization;
using web.Models;
using Microsoft.AspNetCore.Http;

namespace web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class UtilisateursController : RCBenevoleController
    {
        public UtilisateursController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Utilisateurs
        public async Task<IActionResult> Index()
        {
            List<Utilisateur> users = null;
            if (HttpContext.Session.GetString("AppActive") != null)
            {

                if (HttpContext.Session.GetString("AppActive") == "Pointage")
                {
                    users = await _context.Utilisateurs.Include(u => u.Centre).Where(s=>s.app_pointage_benevoles==true).ToListAsync();
                }
                else if (HttpContext.Session.GetString("AppActive") == "Livraison")
                {
                    users = await _context.Utilisateurs.Include(u => u.Centre).Where(s => s.app_bon_livraison == true).ToListAsync();
                }
                
            }
            else
            {
                users = await _context.Utilisateurs.Include(u => u.Centre).ToListAsync();
            }          

            return View(users);
        }

        // GET: Utilisateurs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _context.Utilisateurs.Include(u => u.Centre)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        // GET: Utilisateurs/Create
        public IActionResult Create()
        {
            ViewBag.Centres = _context.Centres
                .OrderBy(c => c.Nom)
                .AsEnumerable();

            return View();
        }

        // POST: Utilisateurs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtilisateurModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Utilisateur.Password != model.PasswordConfirm)
            {
                ModelState.AddModelError("PasswordConfirm", "Les mots de passe ne correspondent pas");
                return View(model);
            }

            var existingUser = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Login == model.Utilisateur.Login);

            var appActive = HttpContext.Session.GetString("AppActive") ?? "Pointage";

            if (existingUser != null)
            {
                // Store the existing user and appActive in TempData or ViewData for the popup
                TempData["ExistingUserId"] = existingUser.ID;
                TempData["AppActive"] = appActive;
                TempData["ShowAssignPopup"] = true;
                return View(model); // Return to view with popup
            }

            model.Utilisateur.app_pointage_benevoles = appActive == "Pointage";
            model.Utilisateur.app_bon_livraison = appActive == "Livraison";

            model.Utilisateur.SetPassword(model.Utilisateur.Password);
            _context.Add(model.Utilisateur);
            await _context.SaveChangesAsync();

            SetGlobalMessage("L'utilisateur a été créé avec succès", EGlobalMessageType.Success);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToApp(int userId)
        {
            var user = await _context.Utilisateurs.FindAsync(userId);
            if (user == null)
            {
                SetGlobalMessage("Utilisateur introuvable", EGlobalMessageType.Error);
                return RedirectToAction(nameof(Index));
            }

            var appActive = HttpContext.Session.GetString("AppActive") ?? "Pointage";

            if (appActive == "Pointage")
                user.app_pointage_benevoles = true;
            else if (appActive == "Livraison")
                user.app_bon_livraison = true;

            await _context.SaveChangesAsync();
            SetGlobalMessage("L'utilisateur a été assigné à l'application", EGlobalMessageType.Success);
            return RedirectToAction(nameof(Index));
        }


        // GET: Utilisateurs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            ViewBag.Centres = _context.Centres
                .OrderBy(c => c.Nom)
                .AsEnumerable();

            var utilisateur = await _context.Utilisateurs.SingleOrDefaultAsync(m => m.ID == id);

            if (utilisateur == null)
                return NotFound();

            return View(utilisateur);
        }

        // POST: Utilisateurs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Login,CentreID")] Utilisateur utilisateur)
        {
            if (id != utilisateur.ID)
                return NotFound();

            var existing = _context.Utilisateurs.SingleOrDefault(u => u.ID == id);
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                try
                {
                    existing.CentreID = utilisateur.CentreID;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtilisateurExists(utilisateur.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                SetGlobalMessage("L'utilisateur a été modifié avec succès", EGlobalMessageType.Success);
                return RedirectToAction(nameof(Index));
            }
            return View(utilisateur);
        }

        // GET: Utilisateurs/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
                return NotFound();

            var utilisateur = await _context.Utilisateurs.SingleOrDefaultAsync(m => m.ID == id);

            if (utilisateur == null)
                return NotFound();

            return View(new ChangePasswordModel { Login = utilisateur.Login });
        }

        // POST: Utilisateurs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordModel model)
        {
            var utilisateur = _context.Utilisateurs.SingleOrDefault(u => u.Login == model.Login);

            if (utilisateur == null)
                return NotFound();

            if (id != utilisateur.ID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            if (model.NewPassword.Trim().Length == 0)
                return BadRequest();

            try
            {
                if (model.NewPassword != model.NewPasswordConfirm)
                {
                    ModelState.AddModelError("NewPasswordConfirm", "Les mots de passe ne correspondent pas");
                    return View(model);
                }

                utilisateur.SetPassword(model.NewPassword);

                _context.Update(utilisateur);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            SetGlobalMessage("Le mot de passe a été changé avec succès", EGlobalMessageType.Success);
            return RedirectToAction(nameof(Index));
        }

        // GET: Utilisateurs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _context.Utilisateurs.Include(u => u.Centre)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        // POST: Utilisateurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var utilisateur = await _context.Utilisateurs.SingleOrDefaultAsync(m => m.ID == id);
            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();
            SetGlobalMessage("L'utilisateur a été supprimé avec succès", EGlobalMessageType.Success);
            return RedirectToAction(nameof(Index));
        }

        private bool UtilisateurExists(int id)
        {
            return _context.Utilisateurs.Any(e => e.ID == id);
        }
    }
}
