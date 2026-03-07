using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HajjSystem.Web.Controllers;

public class CentralConfirmController : BaseController
{
    private readonly IConfirmationService _confirmSvc;
    private readonly IPilgrimService      _pilgrimSvc;
    private readonly HajjSettings         _settings;

    public CentralConfirmController(
        IConfirmationService confirmSvc,
        IPilgrimService      pilgrimSvc,
        IOptions<HajjSettings> settings)
    {
        _confirmSvc = confirmSvc;
        _pilgrimSvc = pilgrimSvc;
        _settings   = settings.Value;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> ReadConfirmed()
    {
        int year = _settings.ActiveHajjYear;
        var data = await _pilgrimSvc.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear == year && p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed)
            .Select(p => new {
                p.PilgrimId, p.FullName, p.ServiceNumber, p.NIC,
                p.RankDesc, p.Region, p.Passport,
                PassportExpire = p.PassportExpire.HasValue ? p.PassportExpire.Value.ToString("yyyy-MM-dd") : "",
                p.FitResult, p.ConfirmCode, p.TypeId, p.BloodGroup, p.RegistrationDate,
                UnitNameAr = p.Unit != null ? p.Unit.UnitNameAr : "",
                UnitId     = p.UnitId ?? 0
            }).ToListAsync();
        return Json(data);
    }

    public async Task<IActionResult> Stats()
    {
        int year  = _settings.ActiveHajjYear;
        var today = DateTime.Today;
        var all   = await _pilgrimSvc.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear == year)
            .Select(p => new { p.ConfirmCode, p.FitResult, p.PassportExpire,
                               UnitNameAr = p.Unit != null ? p.Unit.UnitNameAr : "غير محدد",
                               p.TypeId })
            .ToListAsync();

        var byUnit = all.Where(p => p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed)
            .GroupBy(p => p.UnitNameAr)
            .Select(g => new {
                unit     = g.Key,
                count    = g.Count(),
                original = g.Count(x => x.TypeId == HajjConstants.PilgrimType.Regular),
                standby  = g.Count(x => x.TypeId == HajjConstants.PilgrimType.StandBy),
                passport = g.Count(x => x.PassportExpire.HasValue &&
                                         (x.PassportExpire.Value - today).TotalDays < 180)
            }).OrderByDescending(x => x.count).ToList();

        return Json(new {
            total           = all.Count,
            branchConfirmed = all.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed),
            pending         = all.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Pending),
            returned        = all.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Cancelled),
            passportIssues  = all.Count(p =>
                p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed &&
                p.PassportExpire.HasValue &&
                (p.PassportExpire.Value - today).TotalDays < 180),
            byUnit
        });
    }

    [HttpPost]
    public async Task<IActionResult> FinalApproveAll()
    {
        var r = await _confirmSvc.FinalApproveAllAsync();
        return ServiceResult(r, new { message = $"تمت الموافقة على {r.Value} حاج", count = r.Value });
    }

    [HttpPost]
    public async Task<IActionResult> FinalApproveSelected([FromBody] List<int> ids)
    {
        var r = await _confirmSvc.FinalApproveSelectedAsync(ids);
        return ServiceResult(r, new { message = $"تمت الموافقة على {r.Value} حاج", count = r.Value });
    }

    [HttpPost]
    public async Task<IActionResult> ReturnToBranch(int pilgrimId, string? returnNote) =>
        ServiceResult(await _confirmSvc.ReturnToBranchAsync(pilgrimId, returnNote));
}
