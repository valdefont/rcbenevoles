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

    }
}