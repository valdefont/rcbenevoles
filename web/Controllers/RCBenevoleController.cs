﻿using dal;
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
        private const string LOG_PREFIX = "({COMMON_UserLogin}) [{COMMON_ControllerName}.{COMMON_ActionName}] ";

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

            return IsCentreIdAllowed(benevole.CurrentAdresse.CentreID);
        }


        protected void LogDebug(string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Debug(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(propertyValues));
        }

        protected void LogInfo(string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Information(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(propertyValues));
        }
                
        protected void LogWarning(string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Warning(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(propertyValues));
        }

        protected void LogError(string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Error(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(propertyValues));
        }

        private object[] GenerateFullPropertyValues(params object[] propertyValues)
        {
            const int NB_PROPERTIES_ADDED = 3;

            var realProps = new object[propertyValues.Length + NB_PROPERTIES_ADDED];

            // 1 - Utilisateur
            if(!User.Identity.IsAuthenticated)
                realProps[0] = "<anonymous>";
            else
                realProps[0] = User.Identity.Name;

            // 2 - Controlleur/Action
            var actionDesc = this.ControllerContext?.ActionDescriptor;
            if(actionDesc == null)
            {
                realProps[1] = "";
                realProps[2] = "";
            }
            else
            {
                realProps[1] = actionDesc.ControllerName;
                realProps[2] = actionDesc.ActionName;
            }

            propertyValues.CopyTo(realProps, NB_PROPERTIES_ADDED);

            return realProps;
        }
    }
}
