using dal;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dal.models;

namespace web.Controllers
{
    public abstract class RCBenevoleController : Controller
    {
        protected RCBenevoleContext _context;

        public RCBenevoleContext GetDbContext() => _context;

        public dal.models.Utilisateur GetCurrentUser() => _context.Utilisateurs.Include(u => u.Centre).SingleOrDefault(u => u.Login == User.Identity.Name);

        protected bool IsCentreIdAllowed(int centreId)
        {
            var user = GetCurrentUser();

            return user.Centre == null || user.CentreID == centreId;
        }

        protected bool IsCentreAllowed(Centre centre)
        {
            if (centre == null) throw new ArgumentNullException(nameof(centre));

            var user = GetCurrentUser();

            return user.Centre == null || user.Centre == centre;
        }

        protected bool IsBenevoleAllowed(Benevole benevole)
        {
            if (benevole == null) throw new ArgumentNullException(nameof(benevole));

            return IsCentreIdAllowed(benevole.CentreID);
        }
    }
}
