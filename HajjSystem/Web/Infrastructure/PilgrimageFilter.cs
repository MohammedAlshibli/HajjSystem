using HajjSystem.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HajjSystem.Web.Infrastructure;

/// <summary>
/// Action filter — checks permission before executing any decorated action.
/// Replaces the old [PligrimageFiltter] attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PilgrimageFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        // If not authenticated at all, redirect to login
        if (ctx.HttpContext.User?.Identity?.IsAuthenticated != true)
        {
            ctx.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }
        base.OnActionExecuting(ctx);
    }
}
