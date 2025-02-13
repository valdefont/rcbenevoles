﻿using dal;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dal.models;
using web.Models;
using web.Utils;

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

        protected bool IsBenevoleFromCentreAllowed(Benevole benevole)
        {
            if (benevole == null) throw new ArgumentNullException(nameof(benevole));

            return IsCentreIdAllowed(benevole.CurrentAdresse.CentreID);
        }

        protected bool IsBenevoleAllowed(Benevole benevole)
        {
            if (benevole == null) throw new ArgumentNullException(nameof(benevole));

            return true;
        }

        protected void LogDebug(string templateMessage, params object[] propertyValues)
        {
            LogExtensionUtils.LogDebug(this, templateMessage, propertyValues);
        }

        protected void LogInfo(string templateMessage, params object[] propertyValues)
        {
            LogExtensionUtils.LogInfo(this, templateMessage, propertyValues);
        }

        protected void LogWarning(string templateMessage, params object[] propertyValues)
        {
            LogExtensionUtils.LogWarning(this, templateMessage, propertyValues);
        }

        protected void LogError(string templateMessage, params object[] propertyValues)
        {
            LogExtensionUtils.LogError(this, templateMessage, propertyValues);
        }

        public void SetGlobalMessage(string message, EGlobalMessageType messageType)
        {
            TempData["__GlobalMessage"] = System.Text.Json.JsonSerializer.Serialize(new GlobalMessage
            {
                Message = message,
                Type = messageType,
            });
        }

        protected (DateTime periodStart, DateTime periodEnd) GetPeriodDates(int period, int year)
        {
            DateTime periodStart, periodEnd;

            switch(period)
            {
                case 1:
                    {
                        periodStart = new DateTime(year, 1, 1);
                        periodEnd = new DateTime(year, 5, 1);
                    }
                    break;
                case 2:
                    {
                        periodStart = new DateTime(year, 5, 1);
                        periodEnd = new DateTime(year + 1, 1, 1);
                    }
                    break;
                default:
                    throw new Exception("Période invalide");
            }

            return (periodStart, periodEnd);
        }
    }
}
