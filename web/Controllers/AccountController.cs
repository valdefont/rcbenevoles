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
        public async Task<IActionResult> LogIn([FromBody] LoginPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var user = new AppUser
            {
                Name = model.Login,
            };

            var dbuser = _db.Utilisateurs.Where(u => u.Login == model.Login).SingleOrDefault();

            if(dbuser == null || !dbuser.TestPassword(model.Password))
            {
                ModelState.AddModelError("", "Echec de la connexion. Vérifier votre login et votre mot de passe");
                return View();
            }

            var identity = new ClaimsIdentity(user);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(user));

            return View();
        }
    }
}