using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace web.Controllers
{
    public class AccountController : RCBenevoleController
    {
        public AccountController(dal.RCBenevoleContext dbcontext)
        {
            _context = dbcontext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var dbuser = _context.Utilisateurs.Include(u => u.Centre).Where(u => u.Login == model.Login).SingleOrDefault();

            if(dbuser == null || !dbuser.TestPassword(model.Password))
            {
                ModelState.AddModelError("", "Echec de la connexion. Vérifier votre login et votre mot de passe");
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

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout(LoginPasswordModel model)
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}