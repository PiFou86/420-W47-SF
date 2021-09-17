using DSED_Module04_Preparation_cours.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DSED_Module04_Preparation_cours.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        ApplicationDbContext m_applicationDbContext;

        const string clefValide = "59604896-66a4-4a9b-8f7b-94a5d16bbdaf";
        public async Task OnActionExecutionAsync(ActionExecutingContext p_context, ActionExecutionDelegate p_next)
        {
            ApplicationDbContext appContext = p_context.HttpContext.RequestServices.GetService<ApplicationDbContext>();

            StringValues clefAPI;
            if (!p_context.HttpContext.Request.Headers.TryGetValue("clefAPI", out clefAPI))
            {
                p_context.Result = new UnauthorizedResult();
                return;
            }

            if (!clefValide.Equals(clefAPI))
            {
                p_context.Result = new UnauthorizedResult();
                return;
            }

            // Excécute la suite des filtres
            await p_next();
        }
    }
}
