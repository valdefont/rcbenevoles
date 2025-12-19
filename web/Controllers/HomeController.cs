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
using Microsoft.AspNetCore.Http;

namespace web.Controllers
{
    public class HomeController : RCBenevoleController
    {
        public HomeController(RCBenevoleContext context)
        {
            _context = context;
        }

        public IActionResult Index(string ReturnUrl = null)
        {

            const string HOME_MESSAGE_FILE = "external/home_message";
            if(System.IO.File.Exists(HOME_MESSAGE_FILE))
                ViewData["InformationMessage"] = System.IO.File.ReadAllText(HOME_MESSAGE_FILE);

            if(!string.IsNullOrEmpty(ReturnUrl))
                ViewData["ReturnUrl"] = ReturnUrl;

            if(HttpContext.Session.GetString("AppActive") != null)
            {
                if (HttpContext.Session.GetString("AppActive") == "Pointage")
                {
                    return View();
                }
                else
                {
                    return LaunchBonLivraison();
                }
            }
            else
            {
                return View();
            }
                
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginPasswordModel model)
        {
            model.TrimProperties();
            LogInfo("[LOGIN-TRY:{UserLogin}] Tentative de connexion de {UserLogin}", model.Login);

            if (!ModelState.IsValid)
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Echec de connexion de {UserLogin} : ModelState invalide ({@ModelState})", model.Login, ModelState);
                return View();
            }

            var dbuser = _context.Utilisateurs
                .Include(u => u.Centre)
                .SingleOrDefault(u => u.Login == model.Login);

            if (dbuser == null)
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Echec de connexion de {UserLogin} : Utilisateur inconnu", model.Login);
                ModelState.AddModelError("", "Échec de la connexion. Vérifiez votre login et votre mot de passe.");
                return View();
            }

            if (!dbuser.TestPassword(model.Password))
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Echec de connexion de {UserLogin} : Mot de passe invalide", model.Login);
                ModelState.AddModelError("", "Échec de la connexion. Vérifiez votre login et votre mot de passe.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Login),
                new Claim(ClaimTypes.Authentication, "true"),
            };

            if (dbuser.Centre != null)
                claims.Add(new Claim(ClaimTypes.Role, "BasicAdmin", ClaimValueTypes.String));
            else
                claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin", ClaimValueTypes.String));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            LogInfo("[LOGIN-SUCCESS:{UserLogin}] Succès de la connexion de {UserLogin}", model.Login);

            // Handle ReturnUrl if it's valid
            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                if (Uri.TryCreate(model.ReturnUrl, UriKind.Relative, out Uri uri) && !uri.IsAbsoluteUri)
                {
                    return Redirect(model.ReturnUrl);
                }
            }

            // Redirect based on app access
            if (dbuser.app_pointage_benevoles && dbuser.app_bon_livraison)
            {
                HttpContext.Session.SetString("UserHasAccessToAllApps", "true");
                return RedirectToAction("ChooseApp", "Home");
            }
            else if (dbuser.app_pointage_benevoles)
            {
                HttpContext.Session.SetString("AppActive", "Pointage");
                return RedirectToAction(nameof(Index));
            }
            else if (dbuser.app_bon_livraison)
            {
                HttpContext.Session.SetString("AppActive", "Livraison");
                return RedirectToAction("Index", "BonLivraison"); 
            }
            else
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Aucun accès autorisé pour {UserLogin}", model.Login);
                ModelState.AddModelError("", "Aucune application n'est disponible pour votre compte.");
                return View();
            }
        }


        public IActionResult Legal()
        {
            return View();
        }


        /*public IActionResult BonLivraison()
        {
            return View();
        }*/

        public IActionResult ChooseApp()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LaunchPointage()
        {

            HttpContext.Session.SetString("AppActive", "Pointage");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LaunchBonLivraison()
        {

            HttpContext.Session.SetString("AppActive", "Livraison");
            return RedirectToAction("Index", "BonLivraison"); 
        }



        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
