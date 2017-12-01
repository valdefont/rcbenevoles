using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web.Controllers;

namespace web.Filters
{
    public class AtLeastOneCenterExistsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var dbContext = ((RCBenevoleController)context.Controller).GetDbContext();

            if (dbContext.Centres.Count() == 0)
                context.Result = new ViewResult { ViewName = "ErrorNoCenter" };
        }
    }
}
