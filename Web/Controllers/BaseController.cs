using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HajjSystem.Web.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected ICurrentUserService CurrentUser =>
        HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

    protected string LoggedUserName => CurrentUser.UserName;
    protected bool   IsSysAdmin     => CurrentUser.IsSysAdmin;

    protected void StampNew(BaseEntity entity)
    {
        entity.TenantId  = CurrentUser.TenantId;
        entity.CreatedBy = CurrentUser.UserName;
        entity.CreatedOn = DateTime.Now;
        entity.IsDeleted = false;
    }

    protected void StampUpdate(BaseEntity entity)
    {
        entity.UpdatedBy = CurrentUser.UserName;
        entity.UpdatedOn = DateTime.Now;
    }

    protected IActionResult ServiceResult(Application.Common.Models.Result result, object? okValue = null)
    {
        if (result.Succeeded)
            return okValue is null ? Ok() : Ok(okValue);
        return BadRequest(result.Error);
    }
}
