using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using web.Models;
using Microsoft.AspNetCore.Authentication;

namespace web.Controllers
{
    public class AccountController : Controller
    {
        protected readonly dal.RCBenevoleContext _db;

        public AccountController(dal.RCBenevoleContext dbcontext)
        {
            _db = dbcontext;
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

            var dbuser = _db.Utilisateurs.Where(u => u.Login == model.Login).SingleOrDefault();

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

            if (dbuser.CentreGere != null)
                claims.Add(new Claim("http://rcbenevole/claims/centeradmin", dbuser.CentreGere.ID.ToString()));
            else
                claims.Add(new Claim("http://rcbenevole/claims/superadmin", "true"));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }
    }
}