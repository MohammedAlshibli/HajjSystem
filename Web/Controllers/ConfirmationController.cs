using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HajjSystem.Web.Controllers;

public class ConfirmationController : BaseController
{
    private readonly IConfirmationService _confirmSvc;
    private readonly IPilgrimService      _pilgrimSvc;
    private readonly HajjSettings         _settings;

    public ConfirmationController(
        IConfirmationService confirmSvc,
        IPilgrimService      pilgrimSvc,
        IOptions<HajjSettings> settings)
    {
        _confirmSvc = confirmSvc;
        _pilgrimSvc = pilgrimSvc;
        _settings   = settings.Value;
    }

    [PilgrimageFilter]
    public IActionResult Index() => View();

    // ── READ ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Read()
    {
        int year = _settings.ActiveHajjYear;
        var data = await _pilgrimSvc.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear == year)
            .Select(p => new {
                p.PilgrimId, p.FullName, p.ServiceNumber, p.NIC,
                p.RankDesc, p.Region, p.Passport,
                PassportExpire  = p.PassportExpire.HasValue ? p.PassportExpire.Value.ToString("yyyy-MM-dd") : "",
                NICExpire       = p.NICExpire.HasValue ? p.NICExpire.Value.ToString("yyyy-MM-dd") : "",
                p.FitResult, p.ConfirmCode, p.CancelNote, p.RegistrationDate,
                UnitNameAr      = p.Unit != null ? p.Unit.UnitNameAr : "",
                TypeId          = p.TypeId
            }).ToListAsync();
        return Json(data);
    }

    // ── SUMMARY ───────────────────────────────────────────────────────
    public async Task<IActionResult> Summary()
    {
        int year = _settings.ActiveHajjYear;
        var all  = await _pilgrimSvc.Query()
            .Where(p => p.HajjYear == year)
            .Select(p => new { p.ConfirmCode, p.PassportExpire })
            .ToListAsync();

        var today = DateTime.Today;
        return Json(new {
            total          = all.Count,
            pending        = all.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Pending),
            confirmed      = all.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Confirmed),
            cancelled      = all.Count(p => p.ConfirmCode == HajjConstants.ConfirmCode.Cancelled),
            passportIssues = all.Count(p =>
                p.PassportExpire.HasValue &&
                (p.PassportExpire.Value - today).TotalDays < 180)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(int pilgrimId) =>
        ServiceResult(await _confirmSvc.ConfirmAsync(pilgrimId), new { message = "تم التأكيد بنجاح", pilgrimId });

    [HttpPost]
    public async Task<IActionResult> ConfirmBulk([FromBody] List<int> ids)
    {
        var r = await _confirmSvc.ConfirmBulkAsync(ids);
        return ServiceResult(r, new { message = $"تم تأكيد {r.Value} حاج", count = r.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int pilgrimId, string cancelNote) =>
        ServiceResult(await _confirmSvc.CancelAsync(pilgrimId, cancelNote));

    [HttpPost]
    public async Task<IActionResult> Restore(int pilgrimId) =>
        ServiceResult(await _confirmSvc.RestoreAsync(pilgrimId));
}
