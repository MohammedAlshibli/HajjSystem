using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Constants;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HajjSystem.Web.Controllers;

public class MedicalController : BaseController
{
    private readonly IMedicalService _medicalSvc;
    private readonly IPilgrimService _pilgrimSvc;
    private readonly HajjSettings    _settings;

    public MedicalController(
        IMedicalService medicalSvc,
        IPilgrimService pilgrimSvc,
        IOptions<HajjSettings> settings)
    {
        _medicalSvc = medicalSvc;
        _pilgrimSvc = pilgrimSvc;
        _settings   = settings.Value;
    }

    [PilgrimageFilter]
    public IActionResult Index() => View();

    public async Task<IActionResult> Read()
    {
        int year = _settings.ActiveHajjYear;
        var data = await _pilgrimSvc.Query()
            .Include(p => p.Unit)
            .Where(p => p.HajjYear == year && p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved)
            .Select(p => new {
                p.PilgrimId, p.FullName, p.ServiceNumber, p.NIC,
                p.RankDesc, p.BloodGroup, p.FitResult, p.DoctorNote, p.TypeId,
                InjectionDate  = p.InjectionDate != default ? p.InjectionDate.ToString("yyyy-MM-dd") : "",
                PassportExpire = p.PassportExpire.HasValue ? p.PassportExpire.Value.ToString("yyyy-MM-dd") : "",
                UnitNameAr     = p.Unit != null ? p.Unit.UnitNameAr : ""
            }).ToListAsync();
        return Json(data);
    }

    public async Task<IActionResult> MedicalStats()
    {
        int year = _settings.ActiveHajjYear;
        var all  = await _pilgrimSvc.Query()
            .Where(p => p.HajjYear == year && p.ConfirmCode == HajjConstants.ConfirmCode.HQApproved)
            .Select(p => new { p.FitResult, p.InjectionDate })
            .ToListAsync();

        return Json(new {
            total         = all.Count,
            pending       = all.Count(p => p.FitResult == HajjConstants.FitResult.Pending),
            fit           = all.Count(p => p.FitResult == HajjConstants.FitResult.Fit || p.FitResult == HajjConstants.FitResult.DoctorApproved),
            conditionally = all.Count(p => p.FitResult == HajjConstants.FitResult.ConditionallyFit),
            notFit        = all.Count(p => p.FitResult == HajjConstants.FitResult.NotFit),
            vaccinated    = all.Count(p => p.InjectionDate != default)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Update(int pilgrimId, int fitResult, string? doctorNote, string? injectionDate)
    {
        var r = await _medicalSvc.UpdateMedicalAsync(pilgrimId, fitResult, doctorNote, injectionDate);
        return ServiceResult(r, new { message = "تم تحديث نتيجة الفحص الطبي", pilgrimId, fitResult });
    }

    [HttpPost]
    public async Task<IActionResult> BulkMarkFit([FromBody] List<int> ids)
    {
        var r = await _medicalSvc.BulkMarkFitAsync(ids);
        return ServiceResult(r, new { message = $"تم تحديث {r.Value} سجل", count = r.Value });
    }
}
