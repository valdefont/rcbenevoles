using dal;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web.Controllers
{
    public abstract class RCBenevoleController : Controller
    {
        protected RCBenevoleContext _context;

        public RCBenevoleContext GetDbContext() => _context;
    }
}
